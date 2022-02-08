using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using Crypto_Tools.Models;

namespace Crypto_Tools.PortfolioStrategies;

internal class TopCoinsETF
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    public TopCoinsETF(IMarketCapService mcapService, ICoinPriceService coinPriceService) => (_mcapService, _priceService) = (mcapService, coinPriceService);
    private readonly Dictionary<string, List<float>> _purchases = new();
    private Dictionary<string, List<TimestampedPrice>> _prices = new();

    internal async Task<SimulationResult?> Run(int dcaAmnt, int numCoins, DcaInterval interval, DateTime startDate, DateTime endDate, string[] ignored)
    {
        var mcapData = (await _mcapService.GetBetweenDates(startDate, endDate));
        mcapData = RemoveUneccesaryCoins(mcapData, numCoins);

        var coins = mcapData.Select(x => x.Coins.Take(numCoins)).SelectMany(x => x).Distinct().ToList();
        _prices = await GetPrices(coins, startDate, endDate);

        var periodCount = 0;
        var lastKnownPrices = new Dictionary<string, float>();
        var coinProfits = new Dictionary<string, CoinPriceHolder>();
        var carriedFunds = 0f;
        var portfolioValue = new List<PortfolioSnapshot>();

        TopMarketCap? period = mcapData.First();
        if (period == null) return null;
        var currMcapPosition = 1;
        while (period != null)
        {
            periodCount += 1;

            var extraFunds = SellDropped(period.Coins.Take((int)(numCoins + Math.Round(numCoins * 0.1f)))!, period.Date); // Sell any coins that have left the selected total num coins + 10% and reallocate
            var totDcaAmnt = (extraFunds + dcaAmnt + carriedFunds) / numCoins;



            carriedFunds = 0f;
            var carryingFunds = 0f;
            var currentValue = 0f;

            var coinList = period.Coins.Where(x => !ignored.Contains(x.ToLower())).ToList();
            for (var i = 0; i < coinList.Count; i++)
            {
                try
                {
                    var coin = coinList[i];
                    var coinPrice = GetPriceOnDate(coin, period.Date);
                    if (coinPrice == null || coinPrice == 0 || (lastKnownPrices.ContainsKey(coin) && coinPrice < lastKnownPrices[coin] * 0.001))
                    {
                        if (i < numCoins) carryingFunds += totDcaAmnt;
                        continue;
                    }

                    lastKnownPrices[coin] = (float)coinPrice;
                    if (!_purchases.ContainsKey(coin)) {
                        _purchases.Add(coin, new List<float>());
                    }
                    
                    if (!coinProfits.ContainsKey(coin)) coinProfits.Add(coin, new CoinPriceHolder());
                    if (i < numCoins)
                    {
                        coinProfits[coin].Spent += totDcaAmnt;
                        _purchases[coin].Add(totDcaAmnt / (float)coinPrice);
                    }

                    coinProfits[coin].Value = (float)coinPrice * _purchases[coin].Sum();
                    currentValue += (float)coinPrice * _purchases[coin].Sum();
                }
                catch
                {

                }
            }

            var snapshot = new PortfolioSnapshot
            {
                Date = period.Date,
                Value = currentValue,
                Spent = periodCount * dcaAmnt
            };

            portfolioValue.Add(snapshot);
            carriedFunds += carryingFunds;

            period = interval switch
            {
                DcaInterval.weekly => mcapData.Skip(currMcapPosition + 1).FirstOrDefault(),
                DcaInterval.monthly => mcapData.Skip(currMcapPosition).FirstOrDefault(x =>
                {
                    if (period.Date.Month == 12) return x.Date.Month == 1;
                    else return x.Date.Month > period.Date.Month;
                }),
                _ => null
            };

            if (period != null)
            {
                var newIndex = mcapData.FindIndex(x => x.Date == period!.Date);
                if (newIndex != -1) currMcapPosition = newIndex;
            }
        }
        var res = new SimulationResult(portfolioValue, coinProfits.Select(x => new CoinProfit(x.Key, x.Value.Value - x.Value.Spent)).ToList());

        return res;
    }

    private List<TopMarketCap> RemoveUneccesaryCoins(List<TopMarketCap> data, int top)
    {
        foreach (var month in data) month.Coins = month.Coins.Take(top).ToList();
        return data;
    }

    private float? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var dict = _prices[coin];
        if (!dict.Any(x => x.Date.Date == date.Date)) return null;
        return (float)dict.FirstOrDefault(x => x.Date.Date == date.Date)!.Price!.Price!.Price;
    }

    private float? GetNearestDatePrice(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var closestDate = _prices[coin].OrderBy(t => Math.Abs((t.Date - date).Ticks)).FirstOrDefault();
        if (closestDate == null) return null;
        var nearest = _prices[coin].FirstOrDefault(x => x.Date.Date == closestDate.Date);
        if (nearest == null) return 0f;
        return (float)nearest!.Price!.Price!.Price;
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

