using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using Crypto_Tools.Models;

namespace Crypto_Tools.PortfolioStrategies;

public class OneTimeBuyTheTop
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    public OneTimeBuyTheTop(IMarketCapService mcapService, ICoinPriceService coinPriceService) => (_mcapService, _priceService) = (mcapService, coinPriceService);
    private readonly Dictionary<string, List<float>> _purchases = new();
    private Dictionary<string, List<TimestampedPrice>> _prices = new();

    internal async Task<SimulationResult?> Run(int buyAmnt, int numCoins, DateTime startDate, DateTime endDate, string[] ignored)
    {
        var portfolioValue = new List<PortfolioSnapshot>();
        var portfolio = new Dictionary<string, float>();
        var lastKnownPrices = new Dictionary<string, float>();

        var mcapData = (await _mcapService.GetBetweenDates(startDate, endDate));
        mcapData = RemoveUneccesaryCoins(mcapData, numCoins);

        var coins = mcapData.Select(x => x.Coins.Take(numCoins)).SelectMany(x => x).Distinct().ToList();
        _prices = await GetPrices(coins, startDate, endDate);

        foreach(var period in mcapData)
        {
            var periodCoins = period.Coins.Take(numCoins);
            var missing = periodCoins.Where(x => !ignored.Contains(x) && !portfolio.ContainsKey(x));

            foreach (var coin in missing)
            {
                portfolio.Add(coin, 0f);
                lastKnownPrices.Add(coin, 0f);
            }
            foreach(var coin in portfolio)
            {
                var price = GetPriceOnDate(coin.Key, period.Date);
                if (price != null) lastKnownPrices[coin.Key] = (float)price;
                else price = lastKnownPrices[coin.Key];

                //Check if its been purchased yet
                if (coin.Value == 0 && price != 0) portfolio[coin.Key] = buyAmnt / (float)price;
            }

            portfolioValue.Add(new PortfolioSnapshot
            {
                Date = period.Date,
                Value = portfolio.Sum(x=>x.Value * lastKnownPrices[x.Key]),
                Spent = portfolio.Count * buyAmnt
            });
        }

        return new SimulationResult(portfolioValue, portfolio.Select(x=>new CoinProfit(x.Key, (lastKnownPrices[x.Key]*x.Value)-buyAmnt)).ToList());
    }

    private float? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var dict = _prices[coin];
        if (!dict.Any(x => x.Date.Date == date.Date)) return null;
        return (float)dict.FirstOrDefault(x => x.Date.Date == date.Date)!.PriceHolder!.Price!.Price;
    }

    private List<TopMarketCap> RemoveUneccesaryCoins(List<TopMarketCap> data, int top)
    {
        foreach (var month in data) month.Coins = month.Coins.Take(top).ToList();
        return data;
    }
    private async Task<Dictionary<string, List<TimestampedPrice>>> GetPrices(IList<string> coins, DateTime start, DateTime end)
    {
        var res = new Dictionary<string, List<TimestampedPrice>>();
        foreach (var coin in coins)
        {
            var storedCoin = await _priceService.Find(coin);
            if (storedCoin == null) continue;
            var prices = storedCoin!.Prices.Where(x => x.Date >= start && x.Date <= end).ToList();
            if (prices.Count == 0) continue;
            res.Add(coin, prices);
        }
        return res;
    }
}
