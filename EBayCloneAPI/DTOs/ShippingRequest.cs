namespace EBayAPI.DTOs
{
    public class ShippingRequest
    {
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public double Weight { get; set; }
    }
}
