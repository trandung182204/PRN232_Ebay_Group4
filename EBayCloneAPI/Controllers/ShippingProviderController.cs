using EBayCloneAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EBayCloneAPI.Controllers;

/// <summary>
/// Fake external Shipping API: Create Shipment (returns tracking code), Update Delivery Status, Get Tracking.
/// </summary>
[ApiController]
[Route("api/shipping-provider")]
public class ShippingProviderController : ControllerBase
{
    private readonly FakeShippingProviderService _provider;

    public ShippingProviderController(FakeShippingProviderService provider) => _provider = provider;

    /// <summary>Create shipment → returns tracking code.</summary>
    [HttpPost("shipments")]
    public IActionResult CreateShipment([FromBody] CreateShipmentRequest request)
    {
        if (request == null || request.OrderId <= 0)
            return BadRequest(new { error = "OrderId required" });

        var (success, trackingCode) = _provider.CreateShipment(request.OrderId, request.Region ?? "north");
        if (!success)
            return StatusCode(503, new { error = "Shipping provider temporarily unavailable" });

        return Ok(new { trackingCode });
    }

    /// <summary>Update delivery status for a tracking code.</summary>
    [HttpPut("shipments/{trackingCode}/status")]
    public IActionResult UpdateDeliveryStatus(string trackingCode, [FromBody] UpdateStatusRequest body)
    {
        if (string.IsNullOrWhiteSpace(trackingCode) || body?.Status == null)
            return BadRequest(new { error = "trackingCode and status required" });

        var updated = _provider.UpdateDeliveryStatus(trackingCode.Trim(), body.Status.Trim());
        if (!updated)
            return NotFound(new { error = "Tracking code not found" });

        return Ok(new { trackingCode, status = body.Status });
    }

    /// <summary>Get tracking info by tracking code.</summary>
    [HttpGet("shipments/{trackingCode}")]
    public IActionResult GetTracking(string trackingCode)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            return BadRequest(new { error = "trackingCode required" });

        var (found, status, orderId) = _provider.GetTracking(trackingCode.Trim());
        if (!found)
            return NotFound(new { error = "Tracking code not found" });

        return Ok(new { trackingCode = trackingCode.Trim(), status, orderId });
    }
}

public class CreateShipmentRequest
{
    public int OrderId { get; set; }
    public string? Region { get; set; }
}

public class UpdateStatusRequest
{
    public string? Status { get; set; }
}
