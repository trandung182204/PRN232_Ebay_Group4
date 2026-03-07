namespace EBayAPI.DTO;

public class CreatePaymentDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Method { get; set; }
}