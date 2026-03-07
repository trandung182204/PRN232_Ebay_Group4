using System;
using System.Linq;
using System.Threading.Tasks;
using EBayAPI.Configurations;
using EBayAPI.Events;
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
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderCleanupSettings _settings;
        private readonly IEnumerable<IPaymentEventHook> _paymentHooks;
        private readonly IEnumerable<IShippingEventHook> _shippingHooks;

        public OrderService(
            ApplicationDbContext db,
            IPaymentService payment,
            IShippingService shipping,
            IEmailService email,
            IEventBus eventBus,
            ILogger<OrderService> logger,
            IOptions<OrderCleanupSettings> options,
            IEnumerable<IShippingEventHook> shippingHooks,
            IEnumerable<IPaymentEventHook> paymentHooks)
        {
            _db           = db;
            _payment      = payment;
            _shipping     = shipping;
            _email        = email;
            _eventBus     = eventBus;
            _logger       = logger;
            _settings     = options.Value;
            _shippingHooks = shippingHooks;
            _paymentHooks  = paymentHooks;
        }

        public async Task<OrderTable> CreateOrderAsync(
            int userId,
            int productId,
            int quantity,
            string? addressText,
            string? paymentMethod,
            string? region)
        {
            // Apply defaults for optional fields
            addressText   = string.IsNullOrWhiteSpace(addressText)   ? "Not specified"  : addressText.Trim();
            paymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "COD"            : paymentMethod.Trim();
            region        = string.IsNullOrWhiteSpace(region)        ? "north"          : region.Trim().ToLower();

            var product = await _db.Products.FindAsync(productId);

<<<<<<< HEAD
            if (product == null)
                throw new ArgumentException("Product not found");
=======
        // 4️⃣ Create Payment (Pending)
        var payment = new Payment
        {
            OrderId = order.Id,
            UserId = userId,
            Amount = total,
            Method = paymentMethod,
            Status = OrderStatus.PendingPayment,
            PaidAt = null
        };
>>>>>>> feature/payment

            // 1️⃣ Create Address from text
            var address = new Address
            {
                UserId   = userId,
                Street   = addressText,
                City     = "Unknown",
                State    = "Unknown",
                Country  = "Unknown",
                FullName = "Customer",
                Phone    = "Unknown",
                IsDefault = false
            };

            _db.Addresses.Add(address);
            await _db.SaveChangesAsync();

            decimal productTotal = (product.Price ?? 0) * quantity;
            decimal shippingFee  = CalculateShippingFee(region);
            decimal total        = productTotal + shippingFee;

            // 2️⃣ Create Order
            var order = new OrderTable
            {
                BuyerId   = userId,
                AddressId = address.Id,
                OrderDate = DateTime.UtcNow,
                Status    = EBayAPI.Enums.OrderStatus.PendingPayment,
                TotalPrice = total
            };

            _db.OrderTables.Add(order);
            await _db.SaveChangesAsync();

            // 3️⃣ Create OrderItem
            var item = new OrderItem
            {
                OrderId   = order.Id,
                ProductId = productId,
                Quantity  = quantity,
                UnitPrice = product.Price ?? 0
            };

            _db.OrderItems.Add(item);

            // 4️⃣ Create Payment (Pending)
            var payment = new Payment
            {
                OrderId = order.Id,
                UserId  = userId,
                Amount  = total,
                Method  = paymentMethod,
                Status  = "Pending",
                PaidAt  = null
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // ── Publish OrderCreatedEvent → sends confirmation email to buyer ──
            await PublishOrderCreatedEventAsync(order, product, quantity, addressText, paymentMethod, region, productTotal, shippingFee);

            return order;
        }

        /// <summary>
        /// Processes payment for an existing order.
        /// On success: sets Status = Paid and publishes OrderPaidEvent (KAN-16 / KAN-18).
        /// </summary>
        public async Task<bool> PayOrderAsync(
            int orderId,
            string paymentMethod,
            string authToken,
            string secureKey)
        {
<<<<<<< HEAD
            var order = await _db.OrderTables.FindAsync(orderId);
=======
            OrderId = order.Id,
            UserId = order.BuyerId,
            Amount = order.TotalPrice,
            Method = paymentMethod,
            Status = OrderStatus.Paid,
            PaidAt = DateTime.UtcNow
        };
>>>>>>> feature/payment

            if (order == null || order.Status != EBayAPI.Enums.OrderStatus.PendingPayment)
                return false;

            var (success, _) =
                await _payment.PayAsync(paymentMethod, order.TotalPrice, authToken, secureKey);

            if (!success)
                return false;

            var paidAt = DateTime.UtcNow;

            var payment = new Payment
            {
                OrderId = order.Id,
                UserId  = order.BuyerId,
                Amount  = order.TotalPrice,
                Method  = paymentMethod,
                Status  = "Paid",
                PaidAt  = paidAt
            };

            _db.Payments.Add(payment);
            order.Status = EBayAPI.Enums.OrderStatus.Paid;
            await _db.SaveChangesAsync();

            // ── KAN-16 / KAN-18: Publish OrderPaidEvent ──────────────────
            await PublishOrderPaidEventAsync(order, paymentMethod, paidAt);

            return true;
        }

        /// <summary>
        /// Updates order status. Publishes OrderStatusChangedEvent for
        /// Delivered and Failed transitions (KAN-17 / KAN-18).
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, EBayAPI.Enums.OrderStatus newStatus)
        {
            var order = await _db.OrderTables.FindAsync(orderId);

            if (order == null)
                return false;

            var current = order.Status;

            bool valid = current switch
            {
                EBayAPI.Enums.OrderStatus.PendingPayment =>
                    newStatus == EBayAPI.Enums.OrderStatus.Paid
                    || newStatus == EBayAPI.Enums.OrderStatus.Cancelled
                    || newStatus == EBayAPI.Enums.OrderStatus.Failed,

                EBayAPI.Enums.OrderStatus.Paid =>
                    newStatus == EBayAPI.Enums.OrderStatus.Shipping
                    || newStatus == EBayAPI.Enums.OrderStatus.Cancelled,

                EBayAPI.Enums.OrderStatus.Shipping =>
                    newStatus == EBayAPI.Enums.OrderStatus.Delivered,

                _ => false
            };

            if (!valid)
                throw new Exception($"Invalid status change {current} → {newStatus}");

            order.Status = newStatus;
            await _db.SaveChangesAsync();

            // ── KAN-17 / KAN-18: Publish OrderStatusChangedEvent ─────────
            await PublishOrderStatusChangedEventAsync(order, current, newStatus);

            return true;
        }

        public async Task AutoCancelOnlinePayments()
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-_settings.PaymentTimeoutMinutes);

            var candidates = await _db.OrderTables
                .Where(o =>
                    o.Status == EBayAPI.Enums.OrderStatus.PendingPayment &&
                    o.OrderDate != null &&
                    o.OrderDate <= cutoff &&
                    _db.Payments.Any(p => p.OrderId == o.Id && p.Method != "COD"))
                .Select(o => new { o.Id, o.BuyerId, o.TotalPrice })
                .ToListAsync();

            int cancelled = 0;

            foreach (var c in candidates)
            {
                var affected = await _db.OrderTables
                    .Where(o => o.Id == c.Id && o.Status == EBayAPI.Enums.OrderStatus.PendingPayment)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(o => o.Status, EBayAPI.Enums.OrderStatus.Cancelled));

                if (affected == 1)
                {
                    cancelled++;
                    _logger.LogInformation("Auto cancelling ONLINE order {orderId}", c.Id);

                    if (c.BuyerId != null)
                    {
                        var user = await _db.Users.FindAsync(c.BuyerId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            // Publish via event bus so the email handler fires
                            await _eventBus.PublishAsync(new OrderStatusChangedEvent
                            {
                                OrderId    = c.Id,
                                BuyerEmail = user.Email,
                                BuyerName  = user.Username ?? "Customer",
                                OldStatus  = EBayAPI.Enums.OrderStatus.PendingPayment.ToString(),
                                NewStatus  = "Cancelled",
                                TotalPrice = c.TotalPrice,
                                ChangedAt  = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            _logger.LogInformation("AutoCancelOnlinePayments finished. {count} orders cancelled", cancelled);
        }

        public async Task<OrderTable?> GetOrderDetailAsync(int id)
        {
            return await _db.OrderTables
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Include(o => o.Payments)
                .Include(o => o.ShippingInfos)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<object> GetOrdersAsync(int page, int pageSize, EBayAPI.Enums.OrderStatus? status)
        {
            var query = _db.OrderTables.AsQueryable();

            if (status != null)
                query = query.Where(o => o.Status == status);

            var total  = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new { o.Id, o.OrderDate, o.TotalPrice, o.Status, Buyer = o.BuyerId })
                .ToListAsync();

            return new { page, pageSize, total, data = orders };
        }

        // ─────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────

        private async Task PublishOrderCreatedEventAsync(
            OrderTable order,
            Product product,
            int quantity,
            string? addressText,
            string? paymentMethod,
            string? region,
            decimal productTotal,
            decimal shippingFee)
        {
            try
            {
                var buyer = await _db.Users.FindAsync(order.BuyerId);
                if (buyer == null || string.IsNullOrEmpty(buyer.Email))
                    return;

                await _eventBus.PublishAsync(new OrderCreatedEvent
                {
                    OrderId         = order.Id,
                    BuyerEmail      = buyer.Email,
                    BuyerName       = buyer.Username ?? "Customer",
                    ProductName     = product.Title ?? "Product",
                    Quantity        = quantity,
                    UnitPrice       = product.Price ?? 0,
                    ShippingFee     = shippingFee,
                    TotalPrice      = order.TotalPrice,
                    PaymentMethod   = paymentMethod,
                    ShippingAddress = addressText,
                    Region          = region,
                    OrderDate       = order.OrderDate ?? DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish OrderCreatedEvent for Order #{OrderId}", order.Id);
            }
        }

        private async Task PublishOrderPaidEventAsync(
            OrderTable order,
            string paymentMethod,
            DateTime paidAt)
        {
            try
            {
                var buyer = await _db.Users.FindAsync(order.BuyerId);
                if (buyer == null || string.IsNullOrEmpty(buyer.Email))
                    return;

                var items = await _db.OrderItems
                    .Include(i => i.Product)
                    .Where(i => i.OrderId == order.Id)
                    .ToListAsync();

                await _eventBus.PublishAsync(new OrderPaidEvent
                {
                    OrderId       = order.Id,
                    BuyerEmail    = buyer.Email,
                    BuyerName     = buyer.Username ?? "Customer",
                    TotalPrice    = order.TotalPrice,
                    PaymentMethod = paymentMethod,
                    PaidAt        = paidAt,
                    Items         = items.Select(i => new OrderPaidEvent.OrderItemInfo
                    {
                        ProductName = i.Product?.Title ?? "Product",
                        Quantity    = i.Quantity ?? 1,
                        UnitPrice   = i.UnitPrice ?? 0
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish OrderPaidEvent for Order #{OrderId}", order.Id);
            }
        }

        private async Task PublishOrderStatusChangedEventAsync(
            OrderTable order,
            EBayAPI.Enums.OrderStatus oldStatus,
            EBayAPI.Enums.OrderStatus newStatus)
        {
            // Only notify buyer for Delivered and Failed
            if (newStatus != EBayAPI.Enums.OrderStatus.Delivered &&
                newStatus != EBayAPI.Enums.OrderStatus.Failed)
                return;

            try
            {
                var buyer = await _db.Users.FindAsync(order.BuyerId);
                if (buyer == null || string.IsNullOrEmpty(buyer.Email))
                    return;

                await _eventBus.PublishAsync(new OrderStatusChangedEvent
                {
                    OrderId    = order.Id,
                    BuyerEmail = buyer.Email,
                    BuyerName  = buyer.Username ?? "Customer",
                    OldStatus  = oldStatus.ToString(),
                    NewStatus  = newStatus.ToString(),
                    TotalPrice = order.TotalPrice,
                    ChangedAt  = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish OrderStatusChangedEvent for Order #{OrderId}", order.Id);
            }
        }

        private static decimal CalculateShippingFee(string region) =>
            region.ToLower() switch
            {
                "north"   => 5,
                "south"   => 7,
                "central" => 6,
                _         => 10,
            };
    }
}
