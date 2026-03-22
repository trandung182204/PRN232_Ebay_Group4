using EBayAPI.Events;

namespace EBayCloneAPI.Services;

public interface IEmailService
{
    /// <summary>
    /// Send order-placed confirmation email (immediately after order is created).
    /// </summary>
    Task SendOrderCreatedAsync(OrderCreatedEvent data);

    /// <summary>
    /// KAN-16: Send payment confirmation email with full order summary.
    /// </summary>
    Task SendPaymentConfirmationAsync(OrderPaidEvent data);

    /// <summary>
    /// KAN-17: Send order status-change email (used for Delivered / Failed / Cancelled).
    /// </summary>
    Task SendOrderStatusChangeAsync(OrderStatusChangedEvent data);

    /// <summary>
    /// Send "Your order is on the way" email when order transitions to Shipping.
    /// </summary>
    Task SendOrderShippingAsync(OrderShippingEvent data);
}
