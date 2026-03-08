using EBayAPI.Enums;
using EBayCloneAPI.Data;
using EBayCloneAPI.Models;

namespace EBayCloneAPI.Services;

public class CodPaymentProvider : IPaymentProvider
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CodPaymentProvider> _logger;

    public string Method => "COD";

    public CodPaymentProvider(ApplicationDbContext context, ILogger<CodPaymentProvider> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<object> CreatePayment(int orderId, int userId, decimal amount)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Amount = amount,
            Method = "COD",
            Status = OrderStatus.Paid,
            PaidAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        var order = await _context.OrderTables.FindAsync(orderId);
        order.Status = OrderStatus.Paid;

        await _context.SaveChangesAsync();

        _logger.LogInformation($"COD Payment success Order:{orderId}");

        return payment;
    }

    public Task<bool> ConfirmPayment(int paymentId)
    {
        return Task.FromResult(true);
    }
}