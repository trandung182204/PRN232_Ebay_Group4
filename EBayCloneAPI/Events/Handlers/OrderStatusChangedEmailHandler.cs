using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// KAN-17: Listens to OrderStatusChangedEvent and sends a notification email
/// when the order status becomes Delivered or Failed.
/// </summary>
public sealed class OrderStatusChangedEmailHandler : IEventHandler<OrderStatusChangedEvent>
{
    private static readonly HashSet<string> _notifyStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Delivered", "Failed", "Cancelled" };

    private readonly IEmailService _email;
    private readonly ILogger<OrderStatusChangedEmailHandler> _logger;

    public OrderStatusChangedEmailHandler(
        IEmailService email,
        ILogger<OrderStatusChangedEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(OrderStatusChangedEvent @event)
    {
        // Only send email for meaningful status changes (Delivered / Failed / Cancelled)
        if (!_notifyStatuses.Contains(@event.NewStatus))
        {
            _logger.LogDebug(
                "[OrderStatusChangedEmailHandler] Skipping email for status '{Status}' on Order #{OrderId}",
                @event.NewStatus, @event.OrderId);
            return;
        }

        _logger.LogInformation(
            "[OrderStatusChangedEmailHandler] Sending status-change email for Order #{OrderId} → {Status} to {Email}",
            @event.OrderId, @event.NewStatus, @event.BuyerEmail);

        await _email.SendOrderStatusChangeAsync(@event);

        _logger.LogInformation(
            "[OrderStatusChangedEmailHandler] Status-change email sent for Order #{OrderId}",
            @event.OrderId);
    }
}
