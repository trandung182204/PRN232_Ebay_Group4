namespace EBayAPI.Events;

/// <summary>
/// KAN-17 / KAN-18 — Fired when an order status changes to Delivered or Failed.
/// Carries all data needed to send the status-change notification email.
/// </summary>
public sealed class OrderStatusChangedEvent : IEvent
{
    public int     OrderId    { get; init; }
    public string  BuyerEmail { get; init; } = string.Empty;
    public string  BuyerName  { get; init; } = string.Empty;
    public string  OldStatus  { get; init; } = string.Empty;
    public string  NewStatus  { get; init; } = string.Empty;
    public decimal TotalPrice { get; init; }
    public DateTime ChangedAt { get; init; }
}
