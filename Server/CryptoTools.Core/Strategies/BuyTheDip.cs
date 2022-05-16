using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.Strategies.StrategyOptions;
using Microsoft.Extensions.Logging;

namespace CryptoTools.Core.PortfolioStrategies;

public class BuyTheDip
{
    private readonly CryptoToolsDbContext _db;
    private readonly IPortfolio _portfolio;
    private readonly ILogger<BuyTheDip> _logger;
    public BuyTheDip(CryptoToolsDbContext db, IPortfolio portfolio, ILogger<BuyTheDip> logger) => (_db, _portfolio, _logger) = (db, portfolio, logger);
    public IPortfolio Run(BuyTheDipOptions opts)
    {
        using var db = _db;
        DateTime? period = opts.StartDate;
        var mcapData = db.MarketCapRankings.Where(x => x.Date >= opts.StartDate && x.Date <= opts.EndDate).ToList();
        var allCoins = mcapData.SelectMany(x => x.Coins).Distinct();
        var prices = db.CoinPrices.Where(x => allCoins.Contains(x.CoinSymbol + "-" + x.CoinName)).AsEnumerable().GroupBy(x => $"{x.CoinSymbol}-{x.CoinName}").ToDictionary(x => x.Key, x => x.ToDictionary(x => x.Date, x => x));

        while (period != null)
        {
            var coinsInPeriod = mcapData.FirstOrDefault(x => x.Date == period)?.Coins.Where(x=>prices.ContainsKey(x) && prices[x].ContainsKey((DateTime)period)).ToList();
            if (coinsInPeriod == null) continue;

            foreach (var coin in coinsInPeriod)
            {
                bool shouldBuy = false;
                try
                {
                    if(prices[coin].ContainsKey(period.Value.AddDays(-opts.Timeframe))) shouldBuy = 100 - (prices[coin][(DateTime)period].Price / prices[coin][period.Value.AddDays(-opts.Timeframe)].Price * 100) > (decimal)opts.PurchaseThresholdPercent;
                    shouldBuy = shouldBuy && prices[coin][(DateTime)period].Price != 0;
                    shouldBuy = shouldBuy && prices[coin][period.Value.AddDays(-opts.Timeframe)].Price != 0;
                }
                catch { }
                var portfolioCoin = _portfolio.GetCoin(coin);
                if (shouldBuy)
                {
                    _portfolio.Buy(coin, prices[coin][(DateTime)period].Price, opts.FiatPurchaseAmount);
                    if (opts.TrailingTakeProfit)
                    {
                        portfolioCoin = _portfolio.GetCoin(coin)!;
                        portfolioCoin.TakeProfitValue = portfolioCoin.AveragePurchasePrice * ((100 + (decimal)opts.TakeProfitPercent) / 100);
                        portfolioCoin.StopLossValue = portfolioCoin.AveragePurchasePrice * ((100 - (decimal)opts.StopLossPercent) / 100);
                    }
                }
                if (portfolioCoin == null || portfolioCoin.CurrentPurchases.Count == 0) continue;
                if (opts.TrailingTakeProfit)
                {
                    if (portfolioCoin.StopLossValue >= prices[coin][(DateTime)period].Price) _portfolio.Sell(coin, prices[coin][(DateTime)period].Price);
                    else
                    {
                        if (portfolioCoin.TakeProfitValue <= prices[coin][(DateTime)period].Price)
                        {
                           portfolioCoin.TakeProfitValue = prices[coin][(DateTime)period].Price * ((100 + (decimal)opts.TakeProfitPercent) / 100);
                           portfolioCoin.StopLossValue = prices[coin][(DateTime)period].Price * ((100 - (decimal)opts.StopLossPercent) / 100);
                        }
                    }
                }
                else
                {
                    var shouldSell = portfolioCoin.AveragePurchasePrice / prices[coin][(DateTime)period].Price * 100 > (decimal)opts.TakeProfitPercent;
                    if (!shouldSell) continue;
                    _portfolio.Sell(coin, prices[coin][(DateTime)period].Price);
                }
            }

            _portfolio.TakeSnapshot(period.Value, prices.Where(x => coinsInPeriod.Contains(x.Key) && x.Value.ContainsKey(period.Value)).ToDictionary(x => x.Key, x => x.Value[period.Value].Price));
            period = period.Value.AddDays(1);
            if (period.Value > opts.EndDate) period = null;
        }
        return _portfolio;
    }

    public bool ValidateOptions(StrategyOptions opts) => opts.StartDate.HasValue && opts.EndDate.HasValue && opts.TakeProfitPercent.HasValue && opts.PurchaseThresholdPercent.HasValue && opts.MarketCapRankingMax.HasValue && opts.FiatPurchaseAmount.HasValue && opts.Timeframe.HasValue;
}
