using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ILogger<ShippingService> _logger;
        public ShippingService(ILogger<ShippingService> logger) => _logger = logger;

        // Simulate external API with retry
        public async Task<(bool success, string trackingCode)> CreateShipmentAsync(int orderId, string region, string authToken, string secureKey)
        {
            if (string.IsNullOrEmpty(authToken) || secureKey != "SHIP_SECURE_456")
            {
                _logger.LogWarning("Shipping auth failed");
                return (false, string.Empty);
            }

            int attempts = 0;
            while (attempts < 3)
            {
                attempts++;
                try
                {
                    // Simulate possible transient failure
                    var rnd = new Random();
                    if (rnd.NextDouble() < 0.7)
                    {
                        // success
                        var code = $"TRK-{Guid.NewGuid().ToString().Substring(0,8)}";
                        _logger.LogInformation("Created shipment {code} for order {orderId}", code, orderId);
                        return (true, code);
                    }
                    else
                    {
                        throw new Exception("Transient shipping API error");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Attempt {attempt} failed creating shipment for order {orderId}", attempts, orderId);
                    await Task.Delay(500 * attempts);
                }
            }

            return (false, string.Empty);
        }

        public Task<(bool success, string status)> GetShipmentStatusAsync(string trackingCode)
        {
            // Simulated progression
            var rnd = new Random();
            var value = rnd.Next(0, 3);
            var status = value switch
            {
                0 => "Pending",
                1 => "InTransit",
                2 => "Delivered",
                _ => "Pending",
            };
            return Task.FromResult((true, status));
        }
    }
}
