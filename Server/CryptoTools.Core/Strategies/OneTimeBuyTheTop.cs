using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class OneTimeBuyTheTop
{
    private readonly CryptoToolsDbContext _db;
    private readonly IPortfolio _portfolio;
    public OneTimeBuyTheTop(CryptoToolsDbContext db, IPortfolio portfolio) => (_db, _portfolio) = (db, portfolio);
    public IPortfolio? Run(StrategyOptions opts)
    {
        var lastKnownPrices = new Dictionary<string, decimal>();

        DateTime? period = opts.StartDate;
        while (period != null)
        {
            var mcapData = _db.MarketCapRankings.FirstOrDefault(x => x.Date == period);
            if (mcapData == null) continue;

            var prices = _db.CoinPrices.Where(x => mcapData.Coins.Contains(x.CoinSymbol) && x.Date == period).ToDictionary(x => x.CoinSymbol, x => x);

            foreach (var coin in mcapData.Coins)
            {
                if (!lastKnownPrices.ContainsKey(coin)) lastKnownPrices.Add(coin, 0);
                decimal price;
                if (prices.ContainsKey(coin))
                {
                    lastKnownPrices[coin] = prices[coin].Price;
                    price = prices[coin].Price;
                }
                else price = lastKnownPrices[coin];

                if (_portfolio.GetCoin(coin) == null) _portfolio.Buy(coin, price, opts.FiatPurchaseAmount!.Value);
            }

            _portfolio.TakeSnapshot(period.Value);
            period.Value.AddDays(opts.Timeframe!.Value);
            if (period.Value > opts.EndDate) period = null;
        }

        return _portfolio;
    }

    public bool ValidateOptions(StrategyOptions opts)
    {
        throw new NotImplementedException();
    }
}
