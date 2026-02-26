using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EBayCloneAPI.Services
{
    public class OrderCleanupHostedService : BackgroundService
    {
        private readonly ILogger<OrderCleanupHostedService> _logger;
        private readonly IServiceProvider _provider;

        public OrderCleanupHostedService(ILogger<OrderCleanupHostedService> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
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
                    await orders.CancelUnpaidOrdersAsync();
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning orders");
                }

                await Task.Delay(60_000, stoppingToken);
            }
        }
    }
}
