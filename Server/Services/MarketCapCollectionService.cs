using Crypto_Tools.DAL;
using Crypto_Tools.Helpers;

namespace Crypto_Tools.Services;

public class MarketCapCollectionService : IHostedService
{
    private readonly IMarketCapService _mcapService;
    public readonly ICoinPriceService _priceService;
    private readonly ILogger<MarketCapCollectionService> _logger;
    public MarketCapCollectionService(IMarketCapService mcapService, ICoinPriceService priceService, ILogger<MarketCapCollectionService> logger) => (_mcapService,_priceService, _logger) = (mcapService,priceService, logger);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var mcapGathering = new CMCMarketCapGatherer(_mcapService,_priceService, _logger);
        mcapGathering.GetHistoricTopCoins();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

