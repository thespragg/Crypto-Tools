using Crypto_Tools.DAL;
using Crypto_Tools.Helpers;

namespace Crypto_Tools.Services
{
    public class MarketCapCollectionService : IHostedService
    {
        private readonly IMarketCapService _mcapService;
        public MarketCapCollectionService(IMarketCapService mcapService) => _mcapService = mcapService;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var mcapGathering = new CMCMarketCapGatherer(_mcapService);
            await mcapGathering.GetHistoricTopCoins();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
