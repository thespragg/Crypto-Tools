using Crypto_Tools.DAL;
using Crypto_Tools.Helpers;

namespace Crypto_Tools.Services;

public class CoinPriceTimerService : IHostedService
{
    private readonly IMarketCapService _mcapService;
    private readonly ICoinPriceService _coinPriceService;
    private readonly ILogger<CoinPriceTimerService> _logger;

    private Timer? _priceCheckTimer { get; set; }
    public CoinPriceTimerService(IMarketCapService mcapService, ICoinPriceService priceService, ILogger<CoinPriceTimerService> logger) => (_mcapService, _coinPriceService, _logger) = (mcapService, priceService, logger);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _priceCheckTimer = new Timer(CheckPrices, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    public void CheckPrices(object? state)
    {
        //new Thread(async () =>
        //{
        //    var coins = (await _mcapService.Get()).Select(x => x.Coins).SelectMany(x => x).Distinct().ToList();
        //    foreach (var coin in coins)
        //    {
        //        var storedCoin = await _coinPriceService.Find(coin);
        //        if (storedCoin != null && storedCoin.Prices.OrderBy(x => x.Date).Last().Date > DateTime.Today.AddDays(-7)) continue;
        //        var price = await CoinGeckoStaticHelpers.GetPrice(coin);
        //        if (price == null) continue;
        //        if (storedCoin == null) await _coinPriceService.Create(price);
        //        else
        //        {
        //            price.Id = storedCoin.Id;
        //            await _coinPriceService.Update(price);
        //        }
        //        Thread.Sleep(1200);
        //    }
        //}).Start();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
