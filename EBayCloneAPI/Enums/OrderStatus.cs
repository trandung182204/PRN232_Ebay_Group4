namespace EBayAPI.Enums
{
    public enum OrderStatus
    {
        PendingPayment = 0,
        Paid = 1,
        Shipping = 2,
        Delivered = 3,
        Cancelled = 4,
        Failed = 5
    }
}