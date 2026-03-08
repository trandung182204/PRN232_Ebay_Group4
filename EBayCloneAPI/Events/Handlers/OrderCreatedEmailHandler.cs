using EBayCloneAPI.Services;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events.Handlers;

/// <summary>
/// Listens to OrderCreatedEvent and immediately sends an order-confirmation
/// email to the buyer after they place an order.
/// </summary>
public sealed class OrderCreatedEmailHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IEmailService _email;
    private readonly ILogger<OrderCreatedEmailHandler> _logger;

    public OrderCreatedEmailHandler(IEmailService email, ILogger<OrderCreatedEmailHandler> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation(
            "[OrderCreatedEmailHandler] Sending order confirmation for Order #{OrderId} to {Email}",
            @event.OrderId, @event.BuyerEmail);

        await _email.SendOrderCreatedAsync(@event);

        _logger.LogInformation(
            "[OrderCreatedEmailHandler] Order confirmation sent for Order #{OrderId}",
            @event.OrderId);
    }
}
