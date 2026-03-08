namespace EBayAPI.Events;

/// <summary>
/// In-memory event bus. Resolves all registered IEventHandler&lt;T&gt; and
/// dispatches the event to each one in order.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : IEvent;
}
