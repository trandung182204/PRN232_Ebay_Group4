using EBayCloneAPI.Data;
using EBayCloneAPI.Models;
using EBayCloneAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPaymentService _payment;
        private readonly IShippingService _shipping;
        private readonly IEmailService _email;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext db, IPaymentService payment, IShippingService shipping, IEmailService email, ILogger<OrderService> logger)
        {
            _db = db;
            _payment = payment;
            _shipping = shipping;
            _email = email;
            _logger = logger;
        }

        public async Task<OrderTable> CreateOrderAsync(int userId, int productId, int quantity, string region, string paymentMethod, string authToken, string secureKey, string? couponCode = null)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) throw new ArgumentException("Product not found");

            decimal productTotal = (product.Price ?? 0) * quantity;
            decimal shippingFee = CalculateShippingFee(region);
            decimal discount = 0;
            if (!string.IsNullOrEmpty(couponCode))
            {
                // simple coupon simulation
                discount = 5;
            }

            decimal total = productTotal + shippingFee - discount;

            // Create order
            var order = new OrderTable
            {
                BuyerId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalPrice = total
            };
            _db.OrderTables.Add(order);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Created order {orderId} total {total}", order.Id, total);

            // Process payment
            var (success, tx) = await _payment.PayAsync(paymentMethod, total, authToken, secureKey);
            _logger.LogInformation("Payment attempt for order {orderId} success={success} tx={tx}", order.Id, success, tx);

            if (!success)
            {
                // leave pending
                return order;
            }

            // mark paid
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                Amount = total,
                Method = paymentMethod,
                Status = "Paid",
                PaidAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            // Log transaction id separately (not stored in DB scaffolded model)
            _logger.LogInformation("Payment transaction id for order {orderId}: {tx}", order.Id, tx);
            order.Status = "Paid";
            await _db.SaveChangesAsync();

            // Send payment confirmation
            var user = await _db.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await _email.SendPaymentConfirmationAsync(user.Email, order.Id.ToString());
            }

            // Create shipment
            var (shipSuccess, tracking) = await _shipping.CreateShipmentAsync(order.Id, region, authToken, "SHIP_SECURE_456");
            if (shipSuccess)
            {
                _db.ShippingInfos.Add(new ShippingInfo { OrderId = order.Id, TrackingNumber = tracking, Status = "Created" });
                order.Status = "Shipped";
                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogWarning("Failed to create shipment for order {orderId}", order.Id);
            }

            return order;
        }

        public async Task CancelUnpaidOrdersAsync()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-30);
            var stale = await _db.OrderTables.Where(o => o.Status == "Pending" && o.OrderDate <= cutoff).ToListAsync();
            foreach (var o in stale)
            {
                o.Status = "Cancelled";
                _logger.LogInformation("Auto-cancelling order {orderId}", o.Id);
                var user = await _db.Users.FindAsync(o.BuyerId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _email.SendOrderStatusChangeAsync(user.Email, o.Id.ToString(), "Cancelled");
                }
            }
            await _db.SaveChangesAsync();
        }

        private decimal CalculateShippingFee(string region)
        {
            // simple region-based fee
            return region.ToLower() switch
            {
                "north" => 5,
                "south" => 7,
                "central" => 6,
                _ => 10,
            };
        }
    }
}
