using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class GenericDCA
{
    private Dictionary<string, List<CoinPrice>> _prices { get; set; } = new Dictionary<string, List<CoinPrice>>();
    public SimulationResult? Run(List<CoinAllocation> allocations, int amnt, DcaInterval interval, DateTime start, DateTime end)
    {
        var coins = allocations.Select(x => x.Name).Distinct().ToList();
        var lastKnownPrices = new Dictionary<string, float>();
        _prices = GetPrices(coins, start, end);

        DateTime? period = start;
        if (period == null) return null;
        var periodCount = 0;

        var portfolio = new Dictionary<string, float>();
        var portfolioValue = new List<PortfolioSnapshot>();
        var coinProfits = new Dictionary<string, CoinPriceHolder>();

        foreach (var coin in coins)
        {
            coinProfits.Add(coin, new CoinPriceHolder());
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

            foreach (var coinAllocation in allocations)
            {
                var dcaAmnt = amnt * ((float)coinAllocation.Allocation / 100);
                var currPrice = GetPriceOnDate(coinAllocation.Name, (DateTime)period);
                if (currPrice != null && currPrice != 0)
                {
                    lastKnownPrices[coinAllocation.Name] = (float)currPrice;
                    portfolio[coinAllocation.Name] += dcaAmnt / (float)currPrice;
                    coinProfits[coinAllocation.Name].Spent += dcaAmnt;
                }
                else notSpent += dcaAmnt;

                if (currPrice == null || currPrice == 0) currPrice = (decimal)lastKnownPrices[coinAllocation.Name];
                var value = portfolio[coinAllocation.Name] * (float)currPrice!;
                coinProfits[coinAllocation.Name].Value = value;
                snapshot.Value += value;
            }

            snapshot.Spent = (periodCount * amnt) - notSpent;
            portfolioValue.Add(snapshot);

            period = interval switch
            {
                DcaInterval.weekly => period = period.Value.AddDays(7),
                DcaInterval.monthly => period = period.Value.AddMonths(1),
                _ => null
            };
            if (period > end) period = null;
        }

        return new SimulationResult(portfolioValue, coinProfits.Select(x => new CoinProfit(x.Key, (decimal)x.Value.Value - (decimal)x.Value.Spent)).ToList());
    }

    private decimal? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        return _prices[coin].FirstOrDefault(x => x.Date == date)?.Price;
    }

    private static Dictionary<string, List<CoinPrice>> GetPrices(IList<string> coins, DateTime start, DateTime end)
    {
        var db = new CryptoToolsDbContext();
        return db.CoinPrices.GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.ToList());
    }
}


