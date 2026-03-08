using EBayAPI.Events;
using EBayCloneAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace EBayCloneAPI.Controllers;

/// <summary>
/// Test endpoints for email notifications.
/// Only intended for development / QA — remove or guard in production.
/// </summary>
[ApiController]
[Route("api/test-email")]
[Tags("Email Testing")]
public class TestEmailController : ControllerBase
{
    private readonly IEventBus    _eventBus;
    private readonly IEmailService _email;
    private readonly ILogger<TestEmailController> _logger;

    public TestEmailController(
        IEventBus eventBus,
        IEmailService email,
        ILogger<TestEmailController> logger)
    {
        _eventBus = eventBus;
        _email    = email;
        _logger   = logger;
    }

    // ─────────────────────────────────────────────────────────────
    //  Order Created confirmation
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Test: Send order-placed confirmation email (fires when user clicks "Place Order").
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("order-created")]
    [SwaggerOperation(
        Summary     = "Test order-created confirmation email",
        Description = "Publishes a mock OrderCreatedEvent. The buyer receives an order summary " +
                      "with product details, total price, shipping address and payment instructions.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> TestOrderCreated([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        var mockEvent = new OrderCreatedEvent
        {
            OrderId         = 2001,
            BuyerEmail      = toEmail,
            BuyerName       = "Test Buyer",
            ProductName     = "Apple Watch Series 9 — Midnight Aluminium",
            Quantity        = 1,
            UnitPrice       = 199.99m,
            ShippingFee     = 7.00m,
            TotalPrice      = 206.99m,
            PaymentMethod   = "PayPal",
            ShippingAddress = "123 Test Street, Ho Chi Minh City",
            Region          = "south",
            OrderDate       = DateTime.UtcNow
        };

        try
        {
            await _eventBus.PublishAsync(mockEvent);
            return Ok(new
            {
                success = true,
                message = $"OrderCreatedEvent published. Confirmation email sent to: {toEmail}",
                orderId = mockEvent.OrderId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] Order-created test failed");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-16  Payment confirmation + order summary
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// [KAN-16] Test: Send payment-confirmation email with order summary via EventBus.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("payment-confirmation")]
    [SwaggerOperation(
        Summary     = "KAN-16 — Test payment confirmation email",
        Description = "Publishes a mock OrderPaidEvent through the EventBus. " +
                      "The OrderPaidEmailHandler picks it up and sends a real email via SMTP.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> TestPaymentConfirmation([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        _logger.LogInformation("[TestEmail] Publishing mock OrderPaidEvent to {Email}", toEmail);

        var mockEvent = new OrderPaidEvent
        {
            OrderId       = 1001,
            BuyerEmail    = toEmail,
            BuyerName     = "Test Buyer",
            TotalPrice    = 259.99m,
            PaymentMethod = "PayPal",
            PaidAt        = DateTime.UtcNow,
            Items         = new[]
            {
                new OrderPaidEvent.OrderItemInfo
                {
                    ProductName = "Apple Watch Series 9",
                    Quantity    = 1,
                    UnitPrice   = 199.99m
                },
                new OrderPaidEvent.OrderItemInfo
                {
                    ProductName = "Screen Protector",
                    Quantity    = 2,
                    UnitPrice   = 25.00m
                },
                new OrderPaidEvent.OrderItemInfo
                {
                    ProductName = "Shipping Fee",
                    Quantity    = 1,
                    UnitPrice   = 10.00m
                }
            }
        };

        try
        {
            await _eventBus.PublishAsync(mockEvent);
            return Ok(new
            {
                success = true,
                message = $"OrderPaidEvent published. Email sent to: {toEmail}",
                orderId = mockEvent.OrderId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] Payment confirmation test failed");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-17  Status change — Delivered
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// [KAN-17] Test: Send order-delivered notification email via EventBus.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("order-delivered")]
    [SwaggerOperation(
        Summary     = "KAN-17 — Test order delivered email",
        Description = "Publishes a mock OrderStatusChangedEvent (Delivered) through the EventBus.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> TestOrderDelivered([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        _logger.LogInformation("[TestEmail] Publishing mock OrderStatusChangedEvent (Delivered) to {Email}", toEmail);

        var mockEvent = new OrderStatusChangedEvent
        {
            OrderId    = 1001,
            BuyerEmail = toEmail,
            BuyerName  = "Test Buyer",
            OldStatus  = "Shipping",
            NewStatus  = "Delivered",
            TotalPrice = 259.99m,
            ChangedAt  = DateTime.UtcNow
        };

        try
        {
            await _eventBus.PublishAsync(mockEvent);
            return Ok(new
            {
                success   = true,
                message   = $"OrderStatusChangedEvent (Delivered) published. Email sent to: {toEmail}",
                orderId   = mockEvent.OrderId,
                newStatus = mockEvent.NewStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] Order delivered test failed");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-17  Status change — Failed
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// [KAN-17] Test: Send payment-failed notification email via EventBus.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("order-failed")]
    [SwaggerOperation(
        Summary     = "KAN-17 — Test order failed email",
        Description = "Publishes a mock OrderStatusChangedEvent (Failed) through the EventBus.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> TestOrderFailed([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        _logger.LogInformation("[TestEmail] Publishing mock OrderStatusChangedEvent (Failed) to {Email}", toEmail);

        var mockEvent = new OrderStatusChangedEvent
        {
            OrderId    = 1002,
            BuyerEmail = toEmail,
            BuyerName  = "Test Buyer",
            OldStatus  = "PendingPayment",
            NewStatus  = "Failed",
            TotalPrice = 89.50m,
            ChangedAt  = DateTime.UtcNow
        };

        try
        {
            await _eventBus.PublishAsync(mockEvent);
            return Ok(new
            {
                success   = true,
                message   = $"OrderStatusChangedEvent (Failed) published. Email sent to: {toEmail}",
                orderId   = mockEvent.OrderId,
                newStatus = mockEvent.NewStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] Order failed test failed");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-17  Status change — Cancelled
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// [KAN-17] Test: Send order-cancelled notification email via EventBus.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("order-cancelled")]
    [SwaggerOperation(
        Summary     = "KAN-17 — Test order cancelled email",
        Description = "Publishes a mock OrderStatusChangedEvent (Cancelled) through the EventBus.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> TestOrderCancelled([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        var mockEvent = new OrderStatusChangedEvent
        {
            OrderId    = 1003,
            BuyerEmail = toEmail,
            BuyerName  = "Test Buyer",
            OldStatus  = "PendingPayment",
            NewStatus  = "Cancelled",
            TotalPrice = 45.00m,
            ChangedAt  = DateTime.UtcNow
        };

        try
        {
            await _eventBus.PublishAsync(mockEvent);
            return Ok(new
            {
                success   = true,
                message   = $"OrderStatusChangedEvent (Cancelled) published. Email sent to: {toEmail}",
                orderId   = mockEvent.OrderId,
                newStatus = mockEvent.NewStatus
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] Order cancelled test failed");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  KAN-18  Direct SMTP ping (bypass EventBus)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// [KAN-16] Test: Send a minimal email DIRECTLY via EmailService (bypasses EventBus).
    /// Use this first to verify SMTP credentials are working.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    [HttpPost("smtp-ping")]
    [SwaggerOperation(
        Summary     = "SMTP connectivity test (direct — no EventBus)",
        Description = "Sends a minimal test email directly via EmailService.SendPaymentConfirmationAsync " +
                      "to confirm that the SMTP credentials and Gmail App Password are working correctly.")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SmtpPing([FromQuery] string toEmail)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return BadRequest("toEmail is required.");

        _logger.LogInformation("[TestEmail] SMTP ping to {Email}", toEmail);

        var pingEvent = new OrderPaidEvent
        {
            OrderId       = 9999,
            BuyerEmail    = toEmail,
            BuyerName     = "SMTP Ping Test",
            TotalPrice    = 1.00m,
            PaymentMethod = "Test",
            PaidAt        = DateTime.UtcNow,
            Items         = new[]
            {
                new OrderPaidEvent.OrderItemInfo
                {
                    ProductName = "SMTP Ping — eBayClone email system is working!",
                    Quantity    = 1,
                    UnitPrice   = 1.00m
                }
            }
        };

        try
        {
            // Direct call — no EventBus, immediate feedback on SMTP errors
            await _email.SendPaymentConfirmationAsync(pingEvent);
            return Ok(new
            {
                success = true,
                message = $"SMTP ping successful. Email delivered to: {toEmail}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[TestEmail] SMTP ping failed");
            return StatusCode(500, new
            {
                success = false,
                error   = ex.Message,
                hint    = "Check SmtpServer, SmtpPort, SenderEmail and Password in appsettings.json"
            });
        }
    }
}
