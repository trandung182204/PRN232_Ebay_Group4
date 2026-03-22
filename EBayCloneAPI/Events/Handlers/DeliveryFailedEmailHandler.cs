using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// Listens to DeliveryFailedEvent and sends a "Payment/delivery failed" email to the buyer.
/// </summary>
public sealed class DeliveryFailedEmailHandler : IEventHandler<DeliveryFailedEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<DeliveryFailedEmailHandler> _logger;

    public DeliveryFailedEmailHandler(IEmailService email, ILogger<DeliveryFailedEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(DeliveryFailedEvent @event)
    {
        _logger.LogInformation(
            "[DeliveryFailedEmailHandler] Sending delivery-failed email for Order #{OrderId} to {Email}",
            @event.OrderId, @event.BuyerEmail);

        await _email.SendOrderStatusChangeAsync(new OrderStatusChangedEvent
        {
            OrderId    = @event.OrderId,
            BuyerEmail = @event.BuyerEmail,
            BuyerName  = @event.BuyerName,
            OldStatus  = @event.OldStatus,
            NewStatus  = "Failed",
            TotalPrice = @event.TotalPrice,
            ChangedAt  = @event.FailedAt
        });

        _logger.LogInformation(
            "[DeliveryFailedEmailHandler] Delivery-failed email sent for Order #{OrderId}",
            @event.OrderId);
    }
}
