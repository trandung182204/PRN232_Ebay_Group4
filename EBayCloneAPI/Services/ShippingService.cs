<<<<<<< HEAD
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
=======
>>>>>>> bdc90dd1754ca98231229775d7c8fe8bcf29e5c1
using System;
using System.Threading.Tasks;
using EBayAPI.Models.Hooks;
using Microsoft.Extensions.Logging;

namespace EBayCloneAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ILogger<ShippingService> _logger;
<<<<<<< HEAD
        private readonly FakeShippingProviderService _provider;
        private readonly ResiliencePipeline _retryPipeline;

        public ShippingService(
            ILogger<ShippingService> logger,
            FakeShippingProviderService provider)
        {
            _logger = logger;
            _provider = provider;
            // Polly: 3 retries, exponential backoff (1s, 2s, 4s)
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(1),
                    UseJitter = true,
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "Shipping retry attempt {Attempt} after failure",
                            args.AttemptNumber + 1);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public async Task<(bool success, string trackingCode)> CreateShipmentAsync(
            int orderId, string region, string authToken, string secureKey)
=======
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
>>>>>>> bdc90dd1754ca98231229775d7c8fe8bcf29e5c1
        {
            if (string.IsNullOrEmpty(authToken) || secureKey != "SHIP_SECURE_456")
            {
                _logger.LogWarning("Shipping auth failed");
                return (false, string.Empty);
            }

<<<<<<< HEAD
            try
            {
                var (ok, code) = await _retryPipeline.ExecuteAsync(async _ =>
                {
                    var (success, trackingCode) = _provider.CreateShipment(orderId, region ?? "north");
                    if (!success)
                        throw new InvalidOperationException("Transient shipping API error");
                    return (true, trackingCode!);
                });
=======
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
>>>>>>> bdc90dd1754ca98231229775d7c8fe8bcf29e5c1

                _logger.LogInformation("Created shipment {Code} for order {OrderId}", code, orderId);
                return (ok, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create shipment failed for order {OrderId} after retries", orderId);
                return (false, string.Empty);
            }
        }

        public Task<(bool success, string status)> GetShipmentStatusAsync(string trackingCode)
        {
<<<<<<< HEAD
            var (found, status, _) = _provider.GetTracking(trackingCode ?? "");
            if (!found)
                return Task.FromResult((false, ""));
            return Task.FromResult((true, status ?? "Unknown"));
=======
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
>>>>>>> bdc90dd1754ca98231229775d7c8fe8bcf29e5c1
        }
    }
}