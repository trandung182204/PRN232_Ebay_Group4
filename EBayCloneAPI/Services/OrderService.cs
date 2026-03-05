using System;
using System.Linq;
using System.Threading.Tasks;
using EBayAPI.Configurations;
using EBayAPI.Enums;
using EBayCloneAPI.Data;
using EBayCloneAPI.Models;
using EBayCloneAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EBayCloneAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPaymentService _payment;
        private readonly IShippingService _shipping;
        private readonly IEmailService _email;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderCleanupSettings _settings;

        public OrderService(ApplicationDbContext db, IPaymentService payment,
            IShippingService shipping, IEmailService email, ILogger<OrderService> logger,
            IOptions<OrderCleanupSettings> options)
        {
            _db = db;
            _payment = payment;
            _shipping = shipping;
            _email = email;
            _logger = logger;
            _settings = options.Value;
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
                Status = OrderStatus.PendingPayment,
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
            order.Status = OrderStatus.Paid;
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
                order.Status = OrderStatus.Shipping;
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
            var cutoff = DateTime.UtcNow.AddMinutes(-_settings.PaymentTimeoutMinutes);

            // Get candidates first (Id + BuyerId) to avoid loading full entities
            var candidates = await _db.OrderTables
                .Where(o => o.Status == OrderStatus.PendingPayment
                            && o.OrderDate != null && o.OrderDate <= cutoff)
                .Select(o => new { o.Id, o.BuyerId })
                .ToListAsync();

            int cancelled = 0;
            foreach (var c in candidates)
            {
                // Atomic DB-side update: set status to Cancelled only if still PendingPayment
                var affected = await _db.OrderTables
                    .Where(o => o.Id == c.Id && o.Status == OrderStatus.PendingPayment)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));

                if (affected == 1)
                {
                    cancelled++;
                    _logger.LogInformation("Auto-cancelling order {orderId}", c.Id);

                    if (c.BuyerId != null)
                    {
                        var user = await _db.Users.FindAsync(c.BuyerId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await _email.SendOrderStatusChangeAsync(user.Email, c.Id.ToString(), "Cancelled");
                        }
                    }
                }
            }

            _logger.LogInformation("AutoCancel completed. {count} orders cancelled", cancelled);
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
        public async Task<OrderTable?> GetOrderDetailAsync(int id)
        {
            return await _db.OrderTables
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .Include(o => o.ShippingInfos)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task<object> GetOrdersAsync(int page, int pageSize, OrderStatus? status)
        {
            var query = _db.OrderTables.AsQueryable();

            if (status != null)
                query = query.Where(o => o.Status == status);

            var total = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalPrice,
                    o.Status,
                    Buyer = o.BuyerId
                })
                .ToListAsync();

            return new
            {
                page,
                pageSize,
                total,
                data = orders
            };
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _db.OrderTables.FindAsync(orderId);

            if (order == null)
                return false;

            var current = order.Status;

            bool valid = current switch
            {
                OrderStatus.PendingPayment => newStatus == OrderStatus.Paid
                                              || newStatus == OrderStatus.Cancelled
                                              || newStatus == OrderStatus.Failed,

                OrderStatus.Paid => newStatus == OrderStatus.Shipping
                                    || newStatus == OrderStatus.Cancelled,

                OrderStatus.Shipping => newStatus == OrderStatus.Delivered,

                _ => false
            };

            if (!valid)
                throw new Exception($"Invalid status change {current} → {newStatus}");

            order.Status = newStatus;

            await _db.SaveChangesAsync();

            return true;
        }
    }

}
