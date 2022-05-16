using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using CryptoTools.Core.Strategies;

namespace CryptoTools.Console
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var _strategy = scope.ServiceProvider.GetRequiredService<ITradeStrategy<GenericDCA>>();
            if (_strategy == null) return;
            var portfolio = _strategy.Run(new StrategyOptions
            {
                CoinPurchaseAllocations = new Dictionary<string, float> { { "BTC", 0f } },
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                FiatPurchaseAmount = 10,
                DcaInterval = DcaInterval.weekly
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}