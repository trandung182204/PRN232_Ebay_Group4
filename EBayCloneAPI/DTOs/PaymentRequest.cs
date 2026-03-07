namespace EBayAPI.DTOs
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string OrderId { get; set; }
    }
}
