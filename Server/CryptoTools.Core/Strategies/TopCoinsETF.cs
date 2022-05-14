using System.Diagnostics;
using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Enums;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.Strategies;

namespace CryptoTools.Core.PortfolioStrategies;

public class TopCoinsETF : ITradeStrategy<TopCoinsETF>
{
    private readonly Dictionary<string, List<float>> _purchases = new();
    private Dictionary<string, Dictionary<DateTime, CoinPrice>> _prices = new();
    private readonly Dictionary<string, float> _lastKnownPrices = new();
    private readonly Dictionary<string, CoinPurchase> _profits = new();
    private readonly List<PortfolioSnapshot> _portfolioValue = new();

    private readonly CryptoToolsDbContext _db;
    private readonly IPortfolio _portfolio;
    public TopCoinsETF(CryptoToolsDbContext db, IPortfolio portfolio) => (_db, _portfolio) = (db, portfolio);
    public IPortfolio? Run(StrategyOptions opts)
    {
        //    var mcapData = _db.MarketCapRankings.Where(x => x.Date >= opts.StartDate && x.Date <= opts.EndDate).ToList().Select(x => { x.Coins = x.Coins.Take((int)opts.MarketCapRankingMax!).ToList(); return x; });
        //    var coins = mcapData.SelectMany(x => x.Coins);
        //    _prices = _db.CoinPrices.Where(x => coins.Contains(x.CoinSymbol) && x.Date >= opts.StartDate && x.Date <= opts.EndDate).ToList().GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.ToDictionary(z => z.Date, z => z));
        //    var dates = GetDateRange((DateTime)opts.StartDate!, (DateTime)opts.EndDate!, mcapData, (DcaInterval)opts.DcaInterval!).ToList();
        //    var carriedFunds = 0f;

        //    for (var i = 0; i < dates.Count; i++)
        //    {
        //        carriedFunds = 0f;
        //        var carryingFunds = 0f;
        //        var currentValue = 0f;
        //        var extraFunds = SellDropped(dates[i].Coins.Take((int)((int)opts.MarketCapRankingMax! + Math.Round((int)opts.MarketCapRankingMax! * 0.1f)))!, dates[i].Date);
        //        var coinList = dates[i].Coins.Where(x => !opts.IgnoredCoins!.Contains(x.ToLower()));


        //        var min = Math.Min((int)opts.MarketCapRankingMax!, coinList.Count());
        //        var totDcaAmnt = (extraFunds + (float)opts.FiatPurchaseAmount! + carriedFunds) / min;

        //        if (coinList.Count() == 0)
        //        {

        //        }
        //        foreach (var coin in coinList)
        //        {
        //            var coinPrice = GetPriceOnDate(coin, dates[i].Date);
        //            if (coinPrice == null || coinPrice == 0 || (_lastKnownPrices.ContainsKey(coin) && coinPrice < (decimal)_lastKnownPrices[coin] * (decimal)0.001))
        //            {
        //                carryingFunds += totDcaAmnt;
        //                continue;
        //            }

        //            if (!_purchases.ContainsKey(coin)) _purchases.Add(coin, new List<float>());
        //            if (!_profits.ContainsKey(coin)) _profits.Add(coin, new CoinPurchase());

        //            _lastKnownPrices[coin] = (float)coinPrice;
        //            _profits[coin].Spent += (decimal)totDcaAmnt;
        //            _purchases[coin].Add(totDcaAmnt / (float)coinPrice);
        //            _profits[coin].Value = (decimal)coinPrice * (decimal)_purchases[coin].Sum();
        //            currentValue += (float)coinPrice * _purchases[coin].Sum();
        //            if (currentValue == 0)
        //            {

        //            }
        //        }

        //        var snapshot = new PortfolioSnapshot
        //        {
        //            Date = dates[i].Date,
        //            Value = currentValue,
        //            Spent = ((i + 1) * (float)opts.FiatPurchaseAmount!) - carryingFunds
        //        };

        //        _portfolioValue.Add(snapshot);
        //        carriedFunds += carryingFunds;
        //    }
        //    return new SimulationResult(_portfolioValue, _profits.Select(x => new CoinProfit(x.Key, (decimal)x.Value.Value - (decimal)x.Value.Spent)).ToList());
        return _portfolio;
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

    public bool ValidateOptions(StrategyOptions opts)
    {
        throw new NotImplementedException();
    }
}

