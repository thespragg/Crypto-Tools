using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using Crypto_Tools.Helpers;

namespace Crypto_Tools.PortfolioStrategies;

internal class TopCoinsETF
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    public TopCoinsETF(IMarketCapService mcapService, ICoinPriceService coinPriceService) => (_mcapService, _priceService) = (mcapService, coinPriceService);
    public enum DcaInterval
    {
        Weekly,
        Monthly
    }

    private readonly Dictionary<string, List<float>>  _purchases = new();
    private Dictionary<string, List<TimestampedPrice>> _prices = new();

    internal async Task<List<float>?> Run(int dcaAmnt, int numCoins, DcaInterval interval, DateTime startDate, DateTime endDate)
    {
        var carriedFunds = 0f;
        var portfolioValue = new List<float>();

        var mcapData = await _mcapService.GetBetweenDates(startDate, endDate);
        var coins = mcapData.Select(x => x.Coins.Take(numCoins)).SelectMany(x => x).Distinct().ToList();
        _prices = await GetPrices(coins, startDate, endDate);

        var monthCount = 0;
        TopMarketCap? period = mcapData.First();
        var currMcapPosition = 1;

        if (period == null) return null;

        while (period != null)
        {
            monthCount += 1;
            var extraFunds = SellDropped(period.Coins.Take((int)(numCoins + Math.Round(numCoins * 0.1f)))!, period.Date); // Sell any coins that have left the selected total num coins + 10% and reallocate

            var totDcaAmnt = (extraFunds + dcaAmnt + carriedFunds) / numCoins;
            carriedFunds = 0f;
            var carryingFunds = 0f;
            portfolioValue.Add(0f);

            for (var i = 0; i < period.Coins.Count; i++)
            {
                try
                {
                    var coin = period.Coins[i];
                    var coinPrice = GetPriceOnDate(coin, period.Date);
                    if (coinPrice == null)
                    {
                        if (i < numCoins) carryingFunds += totDcaAmnt;
                        continue;
                    }

                    if (!_purchases.ContainsKey(coin)) _purchases.Add(coin, new List<float>());
                    if (i < numCoins) _purchases[coin].Add(totDcaAmnt / (float)coinPrice);
                    portfolioValue[^1] += (float)coinPrice * _purchases[coin].Sum();
                    var progress = Math.Round(((double)i / period.Coins.Count) * 100);
                }
                catch
                {

                }
            }
            var portfolioBalance = portfolioValue[^1] + carryingFunds;
            carriedFunds += carryingFunds;

            period = interval switch
            {
                DcaInterval.Weekly => mcapData.Skip(currMcapPosition).First(),
                DcaInterval.Monthly => mcapData.Skip(currMcapPosition).First(x => x.Date.Month > period.Date.Month),
                _ => null
            };
        }

        return portfolioValue;
    }

    public float? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var dict = _prices[coin];
        if (!dict.Any(x=>x.Date.Date == date.Date)) return null;
        return dict.FirstOrDefault(x => x.Date.Date == date.Date)!.Price;
    }

    private float? GetNearestDatePrice(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var closestDate = _prices[coin].OrderBy(t => Math.Abs((t.Date - date).Ticks)).FirstOrDefault();
        if (closestDate == null) return null;
        return _prices[coin].FirstOrDefault(x=>x.Date.Date == closestDate.Date)!.Price;
    }

    private async Task<Dictionary<string, List<TimestampedPrice>>> GetPrices(IList<string> coins, DateTime start, DateTime end)
    {
        var res = new Dictionary<string, List<TimestampedPrice>>();
        foreach (var coin in coins)
        {
            var storedCoin = await _priceService.Find(coin);
            if (storedCoin == null)
            {
                var newCoin = await CoinGeckoStaticHelpers.GetPrice(coin);
                storedCoin = await _priceService.Create(newCoin);
            }

            var prices = storedCoin!.Prices.Where(x => x.Date >= start && x.Date <= end).ToList();
            if (prices.Count == 0)
            {
                var lastPrice = storedCoin.Prices.OrderBy(x => x.Date).Last();
                if (lastPrice.Date < end.AddDays(-7))
                {
                    var newCoin = await CoinGeckoStaticHelpers.GetPrice(coin);
                    storedCoin = await _priceService.Create(newCoin);
                }
            }
            res.Add(coin, prices);
        }
        return res;
    }

    private float SellDropped(IEnumerable<string> monthCoins, DateTime month)
    {
        var missingCoins = _purchases.Keys.Where(x => !monthCoins.Contains(x)).ToList();
        var total = 0f;
        foreach (var coin in missingCoins)
        {
            var coinPrice = GetPriceOnDate(coin, month.Date);
            if (coinPrice == null) coinPrice = GetNearestDatePrice(coin, month.Date);
            if (coinPrice != null) total += _purchases[coin].Sum() * (float)coinPrice;
            _purchases.Remove(coin);
        }
        return total;
    }
}

