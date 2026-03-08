using System.Collections.Concurrent;

namespace EBayCloneAPI.Services;

/// <summary>
/// In-memory store for fake external shipping API (Create Shipment → tracking code, Update Status, Get Tracking).
/// </summary>
public class FakeShippingProviderService
{
    private readonly ConcurrentDictionary<string, ShipmentRecord> _shipments = new();
    private readonly ILogger<FakeShippingProviderService> _logger;
    private readonly Random _rnd = new();

    public FakeShippingProviderService(ILogger<FakeShippingProviderService> logger) => _logger = logger;

    public (bool success, string? trackingCode) CreateShipment(int orderId, string region)
    {
        // Simulate transient failure ~30% for retry testing
        if (_rnd.NextDouble() < 0.3)
        {
            _logger.LogWarning("Fake shipping API: simulated failure for order {OrderId}", orderId);
            return (false, null);
        }

        var trackingCode = $"TRK-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        _shipments[trackingCode] = new ShipmentRecord
        {
            OrderId = orderId,
            Region = region,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _logger.LogInformation("Fake shipping API: created shipment {TrackingCode} for order {OrderId}", trackingCode, orderId);
        return (true, trackingCode);
    }

    public bool UpdateDeliveryStatus(string trackingCode, string status)
    {
        if (!_shipments.TryGetValue(trackingCode, out var record))
            return false;
        record.Status = status;
        record.UpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Fake shipping API: updated status {Status} for {TrackingCode}", status, trackingCode);
        return true;
    }

    public (bool found, string? status, int? orderId) GetTracking(string trackingCode)
    {
        if (!_shipments.TryGetValue(trackingCode, out var record))
            return (false, null, null);
        return (true, record.Status, record.OrderId);
    }

    private sealed class ShipmentRecord
    {
        public int OrderId { get; set; }
        public string Region { get; set; } = "";
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
