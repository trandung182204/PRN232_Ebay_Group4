using EBayCloneAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBayCloneAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public TrackingController(ApplicationDbContext db) => _db = db;

    /// <summary>Tra cứu đơn ship theo mã vận đơn (từ bảng ShippingInfo).</summary>
    [HttpGet("{trackingCode}")]
    public async Task<IActionResult> GetByCode(string trackingCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingCode))
            return BadRequest(new { error = "Tracking code is required" });

        var info = await _db.ShippingInfos
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingCode.Trim(), cancellationToken);

        if (info == null)
            return NotFound(new { error = "Tracking code not found" });

        return Ok(new
        {
            trackingNumber = info.TrackingNumber,
            status = info.Status ?? "Pending",
            orderId = info.OrderId,
            carrier = info.Carrier,
            estimatedArrival = info.EstimatedArrival
        });
    }

    /// <summary>Cập nhật trạng thái vận chuyển (admin). Ghi vào DB, khách tra cứu sẽ thấy trạng thái mới.</summary>
    [HttpPut("{trackingCode}/status")]
    public async Task<IActionResult> UpdateStatus(string trackingCode, [FromBody] UpdateTrackingStatusRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(trackingCode) || request?.Status == null)
            return BadRequest(new { error = "trackingCode and status are required" });

        var info = await _db.ShippingInfos
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingCode.Trim(), cancellationToken);

        if (info == null)
            return NotFound(new { error = "Tracking code not found" });

        info.Status = request.Status.Trim();
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { trackingNumber = info.TrackingNumber, status = info.Status });
    }
}

public class UpdateTrackingStatusRequest
{
    public string? Status { get; set; }
}
