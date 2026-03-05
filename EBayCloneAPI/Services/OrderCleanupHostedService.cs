using System.Threading;
using System.Threading.Tasks;
using EBayAPI.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EBayCloneAPI.Services
{
    public class OrderCleanupHostedService : BackgroundService
    {
        private readonly ILogger<OrderCleanupHostedService> _logger;
        private readonly IServiceProvider _provider;
        private readonly OrderCleanupSettings _settings;

        public OrderCleanupHostedService(ILogger<OrderCleanupHostedService> logger, IServiceProvider provider, IOptions<OrderCleanupSettings> options)
        {
            _logger = logger;
            _provider = provider;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Order cleanup service started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var orders = scope.ServiceProvider.GetRequiredService<IOrderService>();
                    
                    await orders.AutoCancelOnlinePayments();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning orders");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(_settings.CleanupIntervalSeconds),
                    stoppingToken);
            }
        }
    }
}
