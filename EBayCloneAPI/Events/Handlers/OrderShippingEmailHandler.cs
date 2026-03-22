using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// Listens to OrderShippingEvent and sends a "Your order is on the way" email to the buyer.
/// </summary>
public sealed class OrderShippingEmailHandler : IEventHandler<OrderShippingEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<OrderShippingEmailHandler> _logger;

    public OrderShippingEmailHandler(IEmailService email, ILogger<OrderShippingEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(OrderShippingEvent @event)
    {
        _logger.LogInformation(
            "[OrderShippingEmailHandler] Sending shipping notification for Order #{OrderId} to {Email}",
            @event.OrderId, @event.BuyerEmail);

        await _email.SendOrderShippingAsync(@event);

        _logger.LogInformation(
            "[OrderShippingEmailHandler] Shipping notification sent for Order #{OrderId}",
            @event.OrderId);
    }
}
