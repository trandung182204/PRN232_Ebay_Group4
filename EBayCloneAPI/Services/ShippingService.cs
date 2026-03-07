using System;
using System.Threading.Tasks;
using EBayAPI.Models.Hooks;
using Microsoft.Extensions.Logging;

namespace EBayCloneAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ILogger<ShippingService> _logger;
        private readonly IEnumerable<IShippingEventHook> _shippingHooks;

        public ShippingService(
            ILogger<ShippingService> logger,
            IEnumerable<IShippingEventHook> shippingHooks)
        {
            _logger = logger;
            _shippingHooks = shippingHooks;
        }

        public async Task<(bool success, string trackingCode)> CreateShipmentAsync(
            int orderId,
            string region,
            string authToken,
            string secureKey)
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
                    var rnd = new Random();

                    if (rnd.NextDouble() < 0.7)
                    {
                        var code = $"TRK-{Guid.NewGuid().ToString().Substring(0, 8)}";

                        _logger.LogInformation(
                            "Created shipment {code} for order {orderId}",
                            code,
                            orderId);

                        // Trigger shipping hooks
                        foreach (var hook in _shippingHooks)
                        {
                            await hook.OnShipmentCreated(orderId, code);
                        }

                        return (true, code);
                    }
                    else
                    {
                        throw new Exception("Transient shipping API error");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Attempt {attempt} failed creating shipment for order {orderId}",
                        attempts,
                        orderId);

                    await Task.Delay(500 * attempts);
                }
            }

            return (false, string.Empty);
        }

        public Task<(bool success, string status)> GetShipmentStatusAsync(string trackingCode)
        {
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