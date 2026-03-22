namespace EBayAPI.Events;

/// <summary>
/// Fired when an order status transitions to Failed.
/// Triggers the delivery-failed notification email to the buyer.
/// </summary>
public sealed class DeliveryFailedEvent : IEvent
{
    public int     OrderId    { get; init; }
    public string  BuyerEmail { get; init; } = string.Empty;
    public string  BuyerName  { get; init; } = string.Empty;
    public string  OldStatus  { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime FailedAt  { get; init; }
}
