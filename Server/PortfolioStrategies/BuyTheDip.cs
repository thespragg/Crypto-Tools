using Crypto_Tools.DAL;
using Crypto_Tools.Models;

namespace Crypto_Tools.PortfolioStrategies;

public class BuyTheDip
{
    private readonly ICoinPriceService _priceService;

    public BuyTheDip(ICoinPriceService coinPriceService) => (_priceService) = (coinPriceService);

    public async Task<(float, float, List<CoinProfit>)> Run(int amnt, DateTime start, DateTime end, int top, int dropPercent, int returnPercent)
    {
        var profit = 0f;
        var totalSpent = 0f;
        var portfolio = new Dictionary<string, float>();
        var results = new List<CoinProfit>();
        try
        {
            var coins = await _priceService.Get();
            
            int periodCount = 0;
            DateTime? period = start;
            while (period != null)
            {

                periodCount += 1;
                foreach (var coin in coins)
                {
                    var price = coin.Prices.FirstOrDefault(x => x.Date == period);
                    if (price == null) continue;
                    if (portfolio.ContainsKey(coin.Symbol))
                    {
                        if (portfolio[coin.Symbol] * price!.Price / 10 * 100 > returnPercent)
                        {
                            profit += portfolio[coin.Symbol] * price!.Price;
                            results.Add(new CoinProfit(coin.Symbol, (portfolio[coin.Symbol] * price!.Price) - 10));
                            portfolio.Remove(coin.Symbol);
                        }
                        continue;
                    }
                    if (price!.CMCRank > top) continue;
                    if (!coin.Prices.Any(x => x.Date == period)) continue;

                    var pricesInPeriod = coin.Prices.Where(x => x.Date <= period && x.Date >= period.Value.AddDays(-3));
                    if (100 - (pricesInPeriod.Last().Price / pricesInPeriod.First().Price * 100) > dropPercent)
                    {
                        portfolio.Add(coin.Symbol, amnt * price!.Price);
                        totalSpent += amnt;
                    }
                }
                period = period.Value.AddDays(1);
                if (period.Value > end) period = null;
            }
        }
        catch (Exception ex)
        {
            return (0f, 0f, new List<CoinProfit>());
        }

        foreach(var coin in portfolio)
        {
            results.Add(new CoinProfit(coin.Key, -10f));
        }

        return (results.Sum(x=>x.Profit), totalSpent, results);
    }
}
