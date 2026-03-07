using EBayAPI.Services;
using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayCloneAPI.Services;

public class PaypalPaymentProvider : IPaymentProvider
{
    private readonly ApplicationDbContext _context;
    private readonly PaypalService _paypal;

    public string Method => "PAYPAL";

    public PaypalPaymentProvider(ApplicationDbContext context, PaypalService paypal)
    {
        _context = context;
        _paypal = paypal;
    }

    public async Task<object> CreatePayment(int orderId, int userId, decimal amount)
    {
        var (paypalOrderId, approveUrl) = await _paypal.CreateOrderAsync(amount);

        var payment = new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Amount = amount,
            Method = "PAYPAL",
            Status = "Pending",
            TransactionId = paypalOrderId
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return new
        {
            paymentId = payment.Id,
            approveUrl = approveUrl
        };
    }

    public async Task<bool> ConfirmPayment(int paymentId)
    {
        var payment = await _context.Payments.FindAsync(paymentId);

        if (payment == null)
            throw new Exception("Payment not found");

        var captureId = await _paypal.CaptureOrderAsync(payment.TransactionId);

        payment.Status = "Paid";
        payment.PaidAt = DateTime.UtcNow;
        payment.TransactionId = captureId;

        var order = await _context.OrderTables.FindAsync(payment.OrderId);
        order.Status = "Paid";

        await _context.SaveChangesAsync();

        return true;
    }
}