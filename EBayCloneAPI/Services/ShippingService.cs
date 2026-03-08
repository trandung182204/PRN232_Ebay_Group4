using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ILogger<ShippingService> _logger;
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
        {
            if (string.IsNullOrEmpty(authToken) || secureKey != "SHIP_SECURE_456")
            {
                _logger.LogWarning("Shipping auth failed");
                return (false, string.Empty);
            }

            try
            {
                var (ok, code) = await _retryPipeline.ExecuteAsync(async _ =>
                {
                    var (success, trackingCode) = _provider.CreateShipment(orderId, region ?? "north");
                    if (!success)
                        throw new InvalidOperationException("Transient shipping API error");
                    return (true, trackingCode!);
                });

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
            var (found, status, _) = _provider.GetTracking(trackingCode ?? "");
            if (!found)
                return Task.FromResult((false, ""));
            return Task.FromResult((true, status ?? "Unknown"));
        }
    }
}
