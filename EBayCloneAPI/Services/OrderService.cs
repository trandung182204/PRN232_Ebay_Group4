using System;
using System.Linq;
using System.Threading.Tasks;
using EBayAPI.Configurations;
using EBayAPI.Enums;
using EBayAPI.Models.Hooks;
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
        private readonly IEnumerable<IPaymentEventHook> _paymentHooks;
        private readonly IEnumerable<IShippingEventHook> _shippingHooks;

        public OrderService(ApplicationDbContext db, IPaymentService payment,
            IShippingService shipping, IEmailService email, ILogger<OrderService> logger,
            IOptions<OrderCleanupSettings> options, IEnumerable<IShippingEventHook> shippingHooks,
            IEnumerable<IPaymentEventHook> paymentHooks)
        {
            _db = db;
            _payment = payment;
            _shipping = shipping;
            _email = email;
            _logger = logger;
            _settings = options.Value;
            _shippingHooks = shippingHooks;
            _paymentHooks = paymentHooks;
            _shippingHooks = shippingHooks;
        }

        public async Task<OrderTable> CreateOrderAsync(
    int userId,
    int productId,
    int quantity,
    string addressText,
    string paymentMethod,
    string region)
        {
            var product = await _db.Products.FindAsync(productId);

            if (product == null)
                throw new ArgumentException("Product not found");

            // 1️⃣ Create Address from text (State dùng luôn để lưu region, không cần cột mới)
            var address = new Address
            {
                UserId = userId,
                Street = addressText,
                City = "Unknown",
                State = region,
                Country = "Unknown",
                FullName = "Customer",
                Phone = "Unknown",
                IsDefault = false
            };

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            decimal productTotal = (product.Price ?? 0) * quantity;
            decimal shippingFee = CalculateShippingFee(region);
            decimal total = productTotal + shippingFee;

            // 2️⃣ Create Order
            var order = new OrderTable
            {
                BuyerId = userId,
                AddressId = address.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.PendingPayment,
                TotalPrice = total
            };

            _db.OrderTables.Add(order);
            await _db.SaveChangesAsync();

            // 3️⃣ Create OrderItem
            var item = new OrderItem
            {
                OrderId = order.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.Price ?? 0
            };

            _db.OrderItems.Add(item);

            // 4️⃣ Create Payment (Pending)
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = userId,
                Amount = total,
                Method = paymentMethod,
                Status = "Pending",
                PaidAt = null
            };

            _db.Payments.Add(payment);

            await _db.SaveChangesAsync();

            return order;
        }
        public async Task<bool> PayOrderAsync(
    int orderId,
    string paymentMethod,
    string authToken,
    string secureKey)
        {
            var order = await _db.OrderTables.Include(o => o.Address).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status != OrderStatus.PendingPayment)
                return false;

            var (success, transactionId) =
                await _payment.PayAsync(paymentMethod, order.TotalPrice ?? 0, authToken, secureKey);

            if (!success)
                return false;

            var payment = new Payment
            {
                OrderId = order.Id,
                UserId = order.BuyerId,
                Amount = order.TotalPrice,
                Method = paymentMethod,
                Status = "Paid",
                PaidAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);

            order.Status = OrderStatus.Paid;

            await _db.SaveChangesAsync();

            // Khi Paid → auto create shipment (region lấy từ Address.State; dùng key nội bộ cho shipping)
            var region = order.Address?.State ?? "north";
            var (shipSuccess, trackingCode) = await _shipping.CreateShipmentAsync(order.Id, region, authToken, "SHIP_SECURE_456");
            if (shipSuccess && !string.IsNullOrEmpty(trackingCode))
            {
                _db.ShippingInfos.Add(new ShippingInfo
                {
                    OrderId = order.Id,
                    Carrier = "FakeCarrier",
                    TrackingNumber = trackingCode,
                    Status = "Pending",
                    EstimatedArrival = DateTime.UtcNow.AddDays(3)
                });
                order.Status = OrderStatus.Shipping;
                await _db.SaveChangesAsync();
                foreach (var hook in _shippingHooks)
                    await hook.OnShipmentCreatedAsync(order, trackingCode);
                _logger.LogInformation("Order {OrderId} shipment created, tracking {TrackingCode}", order.Id, trackingCode);
            }

            return true;
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
            // Phí ship theo khu vực (VN, đơn vị nghìn VND)
            return (region ?? "").ToLowerInvariant() switch
            {
                "hanoi" or "hà nội" or "ha noi" => 20,
                "mienbac" or "miền bắc" or "mien bac" or "north" => 30,
                "mientrung" or "miền trung" or "mien trung" or "central" => 40,
                "miennam" or "miền nam" or "mien nam" or "south" => 50,
                _ => 50,
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
