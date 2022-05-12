using System.Diagnostics;
using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class TopCoinsETF
{
    private readonly Dictionary<string, List<float>> _purchases = new();
    private Dictionary<string, Dictionary<DateTime, CoinPrice>> _prices = new();
    private readonly Dictionary<string, float> _lastKnownPrices = new();
    private readonly Dictionary<string, CoinPriceHolder> _profits = new();
    private readonly List<PortfolioSnapshot> _portfolioValue = new();
    private Stopwatch _timer = new Stopwatch();

    internal SimulationResult? Run(int dcaAmnt, int numCoins, DcaInterval interval, DateTime startDate, DateTime endDate, string[] ignored)
    {
        var db = new CryptoToolsDbContext();
        var mcapData = db.MarketCapRankings.Where(x => x.Date >= startDate && x.Date <= endDate).ToList().Select(x => { x.Coins = x.Coins.Take(numCoins).ToList(); return x; });
        var coins = mcapData.SelectMany(x => x.Coins);
        _prices = db.CoinPrices.Where(x => coins.Contains(x.CoinSymbol) && x.Date >= startDate && x.Date <= endDate).ToList().GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.ToDictionary(z => z.Date, z => z));
        var dates = GetDateRange(startDate, endDate, mcapData, interval).ToList();
        var carriedFunds = 0f;

        for (var i = 0; i < dates.Count; i++)
        {
            carriedFunds = 0f;
            var carryingFunds = 0f;
            var currentValue = 0f;
            var extraFunds = SellDropped(dates[i].Coins.Take((int)(numCoins + Math.Round(numCoins * 0.1f)))!, dates[i].Date);
            var coinList = dates[i].Coins.Where(x => !ignored.Contains(x.ToLower()));


            var min = Math.Min(numCoins, coinList.Count());
            var totDcaAmnt = (extraFunds + dcaAmnt + carriedFunds) / min;

            if (coinList.Count() == 0)
            {

            }
            foreach (var coin in coinList)
            {
                var coinPrice = GetPriceOnDate(coin, dates[i].Date);
                if (coinPrice == null || coinPrice == 0 || (_lastKnownPrices.ContainsKey(coin) && coinPrice < (decimal)_lastKnownPrices[coin] * (decimal)0.001))
                {
                    carryingFunds += totDcaAmnt;
                    continue;
                }

                if (!_purchases.ContainsKey(coin)) _purchases.Add(coin, new List<float>());
                if (!_profits.ContainsKey(coin)) _profits.Add(coin, new CoinPriceHolder());

                _lastKnownPrices[coin] = (float)coinPrice;
                _profits[coin].Spent += totDcaAmnt;
                _purchases[coin].Add(totDcaAmnt / (float)coinPrice);
                _profits[coin].Value = (float)coinPrice * _purchases[coin].Sum();
                currentValue += (float)coinPrice * _purchases[coin].Sum();
                if (currentValue == 0)
                {

                }
            }

            var snapshot = new PortfolioSnapshot
            {
                Date = dates[i].Date,
                Value = currentValue,
                Spent = ((i + 1) * dcaAmnt) - carryingFunds
            };

            _portfolioValue.Add(snapshot);
            carriedFunds += carryingFunds;
        }
        return new SimulationResult(_portfolioValue, _profits.Select(x => new CoinProfit(x.Key, (decimal)x.Value.Value - (decimal)x.Value.Spent)).ToList());
    }

    public IEnumerable<MarketCapRanking> GetDateRange(DateTime startDate, DateTime endDate, IEnumerable<MarketCapRanking> data, DcaInterval interval)
    {
        if (endDate.Date < startDate.Date)
            throw new ArgumentException("endDate must be greater than or equal to startDate");

        var dataDict = data.GroupBy(x => x.Date).ToDictionary(x => x.Key, x => x.First());
        while (startDate.Date <= endDate.Date)
        {
            if (!dataDict.ContainsKey(startDate.Date))
            {
                startDate = interval switch
                {
                    DcaInterval.weekly => startDate.AddDays(7),
                    DcaInterval.monthly => startDate.AddMonths(1),
                    _ => throw new Exception()
                };
                continue;
            }
            yield return dataDict[startDate];
            startDate = interval switch
            {
                DcaInterval.weekly => startDate.AddDays(7),
                DcaInterval.monthly => startDate.AddMonths(1),
                _ => throw new Exception()
            };
        }
    }

    private decimal? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var dict = _prices[coin];
        if (!dict.ContainsKey(date.Date)) return null;
        return dict[date.Date].Price;
    }

    private float SellDropped(IEnumerable<string> monthCoins, DateTime month)
    {
        var missingCoins = _purchases.Keys.Where(x => !monthCoins.Contains(x));
        var total = 0f;
        foreach (var coin in missingCoins)
        {
            var coinPrice = GetPriceOnDate(coin, month.Date);
            if (coinPrice == null && _lastKnownPrices.ContainsKey(coin)) coinPrice = (decimal)_lastKnownPrices[coin];
            if (coinPrice != null) total += _purchases[coin].Sum() * (float)coinPrice;
            _purchases.Remove(coin);
        }
        return total;
    }
}

