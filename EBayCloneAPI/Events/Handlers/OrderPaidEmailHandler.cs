using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// KAN-16: Listens to OrderPaidEvent and sends a payment confirmation email
/// that includes the full order summary.
/// </summary>
public sealed class OrderPaidEmailHandler : IEventHandler<OrderPaidEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<OrderPaidEmailHandler> _logger;

    public OrderPaidEmailHandler(IEmailService email, ILogger<OrderPaidEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(OrderPaidEvent @event)
    {
        _logger.LogInformation(
            "[OrderPaidEmailHandler] Sending payment confirmation for Order #{OrderId} to {Email}",
            @event.OrderId, @event.BuyerEmail);

        await _email.SendPaymentConfirmationAsync(@event);

        _logger.LogInformation(
            "[OrderPaidEmailHandler] Payment confirmation sent successfully for Order #{OrderId}",
            @event.OrderId);
    }
}
