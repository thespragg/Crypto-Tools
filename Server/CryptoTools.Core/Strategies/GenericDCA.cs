using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Enums;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.Strategies;

namespace CryptoTools.Core.PortfolioStrategies;

public class GenericDCA
{
    private readonly CryptoToolsDbContext _db;
    private readonly IPortfolio _portfolio;
    public GenericDCA(CryptoToolsDbContext db, IPortfolio portfolio) => (_db, _portfolio) = (db, portfolio);
    public IPortfolio? Run(StrategyOptions opts)
    {
        var lastKnownPrices = opts.CoinPurchaseAllocations!.ToDictionary(x => x.Key, x => (decimal)0);
        var period = opts.StartDate;
        while (period != null)
        {
            var prices = _db.CoinPrices.Where(x => lastKnownPrices.Keys.Contains(x.CoinSymbol) && x.Date == period).ToDictionary(x => x.CoinSymbol, x => x.Price);
            var missing = opts.CoinPurchaseAllocations!.Where(x => !prices.ContainsKey(x.Key));
            var extraAllocation = missing.Sum(x => x.Value) / (opts.CoinPurchaseAllocations!.Count - missing.Count());
            if (float.IsNaN(extraAllocation)) extraAllocation = 0;
            
            foreach (var (coin, allocation) in opts.CoinPurchaseAllocations!)
            {
                var dcaAmnt = opts.FiatPurchaseAmount!.Value * (decimal)((allocation + extraAllocation) / 100);
                if (!prices.ContainsKey(coin)) continue;
                var price = prices[coin];
                lastKnownPrices[coin] = price;
                _portfolio.Buy(coin, price, dcaAmnt);
            }
            _portfolio.TakeSnapshot(period.Value);

            period = opts.DcaInterval switch
            {
                DcaInterval.weekly => period = period.Value.AddDays(7),
                DcaInterval.monthly => period = period.Value.AddMonths(1),
                _ => null
            };
            if (period > opts.EndDate) period = null;
        }

        return _portfolio;
    }

    public bool ValidateOptions(StrategyOptions opts)
    {
        return true;
    }
}


