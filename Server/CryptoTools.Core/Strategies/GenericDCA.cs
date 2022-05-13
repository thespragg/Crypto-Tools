using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class GenericDCA
{
    private readonly CryptoToolsDbContext _db;
    public GenericDCA(CryptoToolsDbContext db) => _db = db;
    private Dictionary<string, List<CoinPrice>> _prices { get; set; } = new Dictionary<string, List<CoinPrice>>();
    public SimulationResult? Run(StrategyOptions opts)
    {
        var coins = opts.CoinPurchaseAllocations!.Select(x => x.Key).ToList();
        var lastKnownPrices = new Dictionary<string, float>();
        _prices = GetPrices(coins, (DateTime)opts.StartDate!, (DateTime)opts.EndDate!);

        DateTime? period = opts.StartDate;
        if (period == null) return null;
        var periodCount = 0;

        var portfolio = new Dictionary<string, float>();
        var portfolioValue = new List<PortfolioSnapshot>();
        var coinProfits = new Dictionary<string, CoinPurchase>();

        foreach (var coin in coins)
        {
            coinProfits.Add(coin, new CoinPurchase());
            lastKnownPrices.Add(coin, 0f);
            portfolio.Add(coin, 0f);
        }
        var notSpent = 0f;

        while (period != null)
        {
            periodCount += 1;

            var snapshot = new PortfolioSnapshot()
            {
                Date = (DateTime)period,
            };

            foreach (var (coin, allocation) in opts.CoinPurchaseAllocations)
            {
                var dcaAmnt = (float)opts.FiatPurchaseAmount! * ((float)allocation / 100);
                var currPrice = GetPriceOnDate(coin, (DateTime)period);
                if (currPrice != null && currPrice != 0)
                {
                    lastKnownPrices[coin] = (float)currPrice;
                    portfolio[coin] += dcaAmnt / (float)currPrice;
                    coinProfits[coin].Spent += (decimal)dcaAmnt;
                }
                else notSpent += dcaAmnt;

                if (currPrice == null || currPrice == 0) currPrice = (decimal)lastKnownPrices[coin];
                var value = portfolio[coin] * (float)currPrice!;
                coinProfits[coin].Value = (decimal)value;
                snapshot.Value += value;
            }

            snapshot.Spent = (periodCount * (float)opts.FiatPurchaseAmount!) - notSpent;
            portfolioValue.Add(snapshot);

            period = opts.DcaInterval switch
            {
                DcaInterval.weekly => period = period.Value.AddDays(7),
                DcaInterval.monthly => period = period.Value.AddMonths(1),
                _ => null
            };
            if (period > opts.EndDate) period = null;
        }

        return new SimulationResult(portfolioValue, coinProfits.Select(x => new CoinProfit(x.Key, (decimal)x.Value.Value - (decimal)x.Value.Spent)).ToList());
    }

    private decimal? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        return _prices[coin].FirstOrDefault(x => x.Date == date)?.Price;
    }

    private Dictionary<string, List<CoinPrice>> GetPrices(IList<string> coins, DateTime start, DateTime end) => _db.CoinPrices.GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.ToList());
}


