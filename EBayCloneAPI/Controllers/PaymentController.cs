using EBayAPI.DTO;
using EBayAPI.Enums;
using EBayAPI.Services;
using EBayCloneAPI.Data;
using EBayCloneAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBayCloneAPI.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;

    public PaymentController(IPaymentService paymentService, ApplicationDbContext dbContext, IConfiguration config)
    {
        _paymentService = paymentService;
        _context = dbContext;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(CreatePaymentDto dto)
    {
        var order = await _context.OrderTables.FindAsync(dto.OrderId);
        if (order == null)
            return BadRequest("Order not found");
        if (order.BuyerId != dto.UserId)
            return BadRequest("Invalid user");
        if (order.Status == OrderStatus.Paid)
            return BadRequest("Order already paid");
        if (order.Status != OrderStatus.PendingPayment)
            return BadRequest("Order not available for payment");
        var provider = _paymentService.GetProvider(dto.Method);

        var result = await provider.CreatePayment(
            dto.OrderId,
            dto.UserId,
            order.TotalPrice // ✅ lấy từ DB
        );

        return Ok(result);
    }

    [HttpGet("{id}/confirm")]
    public async Task<IActionResult> Confirm(int id, string method)
    {
        var provider = _paymentService.GetProvider(method);

        await provider.ConfirmPayment(id);

        return Ok("Payment confirmed");
    }

    [HttpGet("paypal-success")]
    public async Task<IActionResult> PaypalSuccess(string token)
    {
        var payment = _context.Payments
            .FirstOrDefault(p => p.TransactionId == token);

        if (payment == null)
            return BadRequest("Payment not found");

        var provider = _paymentService.GetProvider("PAYPAL");

        await provider.ConfirmPayment(payment.Id);

        var baseUrl = _config["Frontend:BaseUrl"];
        return Redirect($"{baseUrl}/Payment/PaymentSuccess?orderId={payment.OrderId}");
    }
    
    [HttpGet("paypal-cancel")]
    public IActionResult PaypalCancel()
    {
        var baseUrl = _config["Frontend:BaseUrl"];
        return Redirect($"{baseUrl}/Payment/PaymentCancel");
    }
}