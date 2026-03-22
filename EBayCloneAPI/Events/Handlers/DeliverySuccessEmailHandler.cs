using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// Listens to DeliverySuccessEvent and sends a "Your order has been delivered" email to the buyer.
/// </summary>
public sealed class DeliverySuccessEmailHandler : IEventHandler<DeliverySuccessEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<DeliverySuccessEmailHandler> _logger;

    public DeliverySuccessEmailHandler(IEmailService email, ILogger<DeliverySuccessEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(DeliverySuccessEvent @event)
    {
        _logger.LogInformation(
            "[DeliverySuccessEmailHandler] Sending delivery-success email for Order #{OrderId} to {Email}",
            @event.OrderId, @event.BuyerEmail);

        await _email.SendOrderStatusChangeAsync(new OrderStatusChangedEvent
        {
            OrderId    = @event.OrderId,
            BuyerEmail = @event.BuyerEmail,
            BuyerName  = @event.BuyerName,
            OldStatus  = @event.OldStatus,
            NewStatus  = "Delivered",
            TotalPrice = @event.TotalPrice,
            ChangedAt  = @event.DeliveredAt
        });

        _logger.LogInformation(
            "[DeliverySuccessEmailHandler] Delivery-success email sent for Order #{OrderId}",
            @event.OrderId);
    }
}
