namespace EBayAPI.Events;

/// <summary>
/// KAN-16 / KAN-18 — Fired when an order payment is confirmed (Status → Paid).
/// Carries all data needed to send the payment confirmation email with order summary.
/// </summary>
public sealed class OrderPaidEvent : IEvent
{
    public int    OrderId       { get; init; }
    public string BuyerEmail    { get; init; } = string.Empty;
    public string BuyerName     { get; init; } = string.Empty;
    public decimal TotalPrice   { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
    public DateTime PaidAt      { get; init; }
    public IReadOnlyList<OrderItemInfo> Items { get; init; } = Array.Empty<OrderItemInfo>();

    public sealed class OrderItemInfo
    {
        public string  ProductName { get; init; } = string.Empty;
        public int     Quantity    { get; init; }
        public decimal UnitPrice   { get; init; }
        public decimal LineTotal   => UnitPrice * Quantity;
    }
}
