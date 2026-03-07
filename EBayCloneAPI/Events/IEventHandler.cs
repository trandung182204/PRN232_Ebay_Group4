namespace EBayAPI.Events;

/// <summary>Handles a specific domain event of type <typeparamref name="T"/>.</summary>
public interface IEventHandler<in T> where T : IEvent
{
    Task HandleAsync(T @event);
}
