namespace EBayAPI.Events;

/// <summary>
/// Fired immediately after a new order is placed (Status = PendingPayment).
/// Triggers the order-confirmation email to the buyer.
/// </summary>
public sealed class OrderCreatedEvent : IEvent
{
    public int     OrderId       { get; init; }
    public string  BuyerEmail    { get; init; } = string.Empty;
    public string  BuyerName     { get; init; } = string.Empty;
    public string  ProductName   { get; init; } = string.Empty;
    public int     Quantity      { get; init; }
    public decimal UnitPrice     { get; init; }
    public decimal ShippingFee   { get; init; }
    public decimal TotalPrice    { get; init; }
    public string  PaymentMethod { get; init; } = string.Empty;
    public string  ShippingAddress { get; init; } = string.Empty;
    public string  Region        { get; init; } = string.Empty;
    public DateTime OrderDate    { get; init; }
}
