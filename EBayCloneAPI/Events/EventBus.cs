using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EBayAPI.Events;

/// <summary>
/// Singleton in-memory event bus.
/// Dispatches events to all registered handlers on a background thread (fire-and-forget)
/// so the HTTP response is returned immediately without waiting for email/IO operations.
/// Uses IServiceScopeFactory to safely resolve scoped handlers (e.g. EmailService).
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

    public Task PublishAsync<T>(T @event) where T : IEvent
    {
        var eventName = typeof(T).Name;
        _logger.LogInformation("[EventBus] Queuing {Event} for background dispatch", eventName);

        // Fire-and-forget: chạy trên background thread, không block HTTP response
        _ = Task.Run(async () =>
        {
            try
            {
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
                        // Handler failure must NOT affect other handlers.
                        _logger.LogError(ex, "[EventBus] Handler {Handler} failed for {Event}", handlerName, eventName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EventBus] Background dispatch failed for {Event}", eventName);
            }
        });

        return Task.CompletedTask;
    }
}
