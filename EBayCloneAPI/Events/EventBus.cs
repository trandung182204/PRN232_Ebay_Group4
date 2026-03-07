using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events;

/// <summary>
/// Singleton in-memory event bus.
/// Uses IServiceScopeFactory so scoped handlers (e.g. EmailService) are
/// resolved safely inside a dedicated scope per publish call.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EventBus> _logger;

    public EventBus(IServiceScopeFactory scopeFactory, ILogger<EventBus> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event) where T : IEvent
    {
        var eventName = typeof(T).Name;
        _logger.LogInformation("[EventBus] Publishing {Event}", eventName);

        using var scope = _scopeFactory.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<T>>();

        foreach (var handler in handlers)
        {
            var handlerName = handler.GetType().Name;
            try
            {
                _logger.LogInformation("[EventBus] {Event} -> {Handler}", eventName, handlerName);
                await handler.HandleAsync(@event);
            }
            catch (Exception ex)
            {
                // A handler failure must NOT stop the main flow.
                _logger.LogError(ex, "[EventBus] Handler {Handler} failed for {Event}", handlerName, eventName);
            }
        }
    }
}
