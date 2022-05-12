using CryptoTools.Core.DAL;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class BuyTheDip
{
    public (decimal, float, List<CoinProfit>) Run(int amnt, DateTime start, DateTime end, int top, int dropPercent, int returnPercent)
    {
        var profit = (decimal)0;
        var totalSpent = 0f;
        var portfolio = new Dictionary<string, decimal>();
        var results = new List<CoinProfit>();
        var db = new CryptoToolsDbContext();
        try
        {
            var coins = db.CoinPrices.Select(x=>x.CoinSymbol).Distinct().ToList();
            
            int periodCount = 0;
            DateTime? period = start;
            while (period != null)
            {

                periodCount += 1;
                foreach (var coin in coins)
                {
                    var price = db.CoinPrices.FirstOrDefault(x=>x.CoinSymbol == coin && x.Date == period);
                    if (price == null) continue;
                    if (portfolio.ContainsKey(coin))
                    {
                        if (portfolio[coin] * price!.Price / 10 * 100 > returnPercent)
                        {
                            profit += portfolio[coin] * price!.Price;
                            results.Add(new CoinProfit(coin, (portfolio[coin] * price!.Price) - 10));
                            portfolio.Remove(coin);
                        }
                        continue;
                    }
                    if (price!.MarketCapRank > top) continue;

                    var pricesInPeriod = db.CoinPrices.Where(x => x.CoinSymbol == coin && x.Date <= period && x.Date >= period.Value.AddDays(-3)).ToList();
                    if (100 - (pricesInPeriod.Last().Price / pricesInPeriod.First().Price * 100) > dropPercent)
                    {
                        portfolio.Add(coin, amnt * price!.Price);
                        totalSpent += amnt;
                    }
                }
                period = period.Value.AddDays(1);
                if (period.Value > end) period = null;
            }
        }
        catch
        {
            return (0, 0f, new List<CoinProfit>());
        }

        foreach(var coin in portfolio)
        {
            results.Add(new CoinProfit(coin.Key, -10));
        }

        return (results.Sum(x=>x.Profit), totalSpent, results);
    }
}
