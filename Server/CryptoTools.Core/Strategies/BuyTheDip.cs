using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.Strategies;

namespace CryptoTools.Core.PortfolioStrategies;

public class BuyTheDip : ITradeStrategy<BuyTheDip>
{
    private readonly CryptoToolsDbContext _db;
    private readonly IPortfolio _portfolio;
    public BuyTheDip(CryptoToolsDbContext db, IPortfolio portfolio) => (_db, _portfolio) = (db, portfolio);
    public IPortfolio Run(StrategyOptions opts)
    {
        var coins = _db.MarketCapRankings.SelectMany(x => x.Coins).Distinct().ToList();

        DateTime? period = opts.StartDate;
        while (period != null)
        {
            var pricesInPeriod = _db.CoinPrices.Where(x => coins.Contains(x.CoinSymbol) && x.Date == period).ToDictionary(x=>x.CoinSymbol, x=>x);

            foreach(var coin in coins)
            {
                if(!pricesInPeriod.ContainsKey(coin)) continue;
                var portfolioCoin = _portfolio.GetCoin(coin);
                if (portfolioCoin != null)
                {
                    var shouldSell = SellThresholdReached(opts.TakeProfitPercent!.Value, portfolioCoin.Purchases.First().Price, pricesInPeriod[coin].Price);
                    _portfolio.Sell(coin, pricesInPeriod[coin].Price);
                    continue;
                }
                if (pricesInPeriod[coin].MarketCapRank > opts.MarketCapRankingMax) continue;

                var shouldBuy = DropThresholdReached(coin, period!.Value, opts.Timeframe!.Value, opts.PurchaseThresholdPercent!.Value);
                if(!shouldBuy) continue;
                _portfolio.Buy(coin, pricesInPeriod[coin].Price, opts.FiatPurchaseAmount!.Value);
            }

            _portfolio.TakeSnapshot(period.Value);
            period = period.Value.AddDays(1);
            if (period.Value > opts.EndDate) period = null;
        }
        return _portfolio;
    }

    private bool DropThresholdReached(string symbol, DateTime date, int timeframe, float dropPercent)
    {
        var pricesInTimeframe = _db.CoinPrices.Where(x => x.CoinSymbol == symbol && x.Date <= date && x.Date >= date.AddDays(timeframe)).OrderBy(x=>x.Date).ToList();
        return 100 - (pricesInTimeframe.Last().Price / pricesInTimeframe.First().Price * 100) > (decimal)dropPercent;
    }

    private bool SellThresholdReached(float takeProfitPercent, decimal currentPrice, decimal buyPrice) => currentPrice / currentPrice * 100 > (decimal)takeProfitPercent;

    public bool ValidateOptions(StrategyOptions opts) => opts.StartDate.HasValue && opts.EndDate.HasValue && opts.TakeProfitPercent.HasValue && opts.PurchaseThresholdPercent.HasValue && opts.MarketCapRankingMax.HasValue && opts.FiatPurchaseAmount.HasValue && opts.Timeframe.HasValue;
}
