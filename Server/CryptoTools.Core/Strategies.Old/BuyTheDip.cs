// using CryptoTools.Core.DAL;
// using CryptoTools.Core.DAL.Models;
// using CryptoTools.Core.Interfaces;
// using CryptoTools.Core.Models;
// using CryptoTools.Core.Strategies.StrategyOptions;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.DependencyInjection;
// using System.Diagnostics;
//
// namespace CryptoTools.Core.PortfolioStrategies;
//
// public class BuyTheDip
// {
//     private readonly CryptoToolsDbContext _db;
//     private readonly ILogger<BuyTheDip> _logger;
//     private readonly IServiceProvider _serviceProvider;
//     public BuyTheDip(CryptoToolsDbContext db, ILogger<BuyTheDip> logger, IServiceProvider serviceProvider) => (_db, _logger, _serviceProvider) = (db, logger, serviceProvider);
//
//     public async Task<Dictionary<int, List<SimulationResult>>> RunMany(Dictionary<int, List<BuyTheDipOptions>> opts)
//     {
//         using var db = _db;
//         var lowest = opts.SelectMany(x => x.Value).Min(x => x.StartDate);
//         var highest = opts.SelectMany(x => x.Value).Max(x => x.EndDate);
//         var mcapData = db.MarketCapRankings.Where(x => x.Date >= lowest && x.Date <= highest).ToList();
//         var allCoins = mcapData.SelectMany(x => x.Coins).Distinct();
//         var prices = GroupingToPriceDictionary(db.CoinPrices.Where(x => allCoins.Contains(x.CoinSymbol + "-" + x.CoinName)).GroupBy(x => new { x.CoinName, x.CoinSymbol }, (key, val) =>
//          KeyValuePair.Create(key.CoinSymbol + "-" + key.CoinName, val.Select(x => KeyValuePair.Create(x.Date, x.Price)))).AsEnumerable());
//         var res = new Dictionary<int, List<SimulationResult>>();
//         var count = 1;
//         foreach (var (key, value) in opts)
//         {
//             _logger.LogInformation("Running batch: {i}", count);
//             res.Add(key, new List<SimulationResult>());
//             var completed = 1;
//
//             var tasks = new List<Task>();
//             foreach (var opt in value)
//             {
//                 var task = Task.Factory.StartNew(() =>
//                 {
//                     var data = mcapData.Where(x => x.Date >= opt.StartDate && x.Date <= opt.EndDate).ToDictionary(x => x.Date);
//                     var portfolio = Run(opt, prices, data);
//                     res[key].Add(SimulationResult.FromIPortfolio(portfolio, opt));
//                     completed += 1;
//                     if (completed % 10 == 0) _logger.LogInformation("Finished {count}", completed);
//                     GC.Collect();
//                 });
//                 tasks.Add(task);
//             }
//             await Task.WhenAll(tasks);
//             count += 1;
//         }
//         return res;
//     }
//
//     private static Dictionary<string, Dictionary<DateTime, decimal>> GroupingToPriceDictionary(IEnumerable<KeyValuePair<string, IEnumerable<KeyValuePair<DateTime, decimal>>>> prices)
//     {
//         var resDict = new Dictionary<string, Dictionary<DateTime, decimal>>();
//         foreach (var price in prices)
//         {
//             resDict.Add(price.Key, price.Value.GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.First().Value));
//         }
//         return resDict;
//     }
//
//     public IPortfolio Run(BuyTheDipOptions opts, Dictionary<string, Dictionary<DateTime, decimal>> prices, IDictionary<DateTime, MarketCapRanking> mcapData)
//     {
//         using var scope = _serviceProvider.CreateScope();
//         var portfolio = scope.ServiceProvider.GetRequiredService<IPortfolio>();
//
//         DateTime? period = opts.StartDate;
//         while (period != null)
//         {
//             if (!mcapData.ContainsKey(period.Value))
//             {
//                 period = period.Value.AddDays(1);
//                 if (period.Value > opts.EndDate) period = null;
//                 continue;
//             }
//             var coinsInPeriod = new HashSet<string>(mcapData[period.Value].Coins.Where(x => prices.ContainsKey(x) && prices[x].ContainsKey((DateTime)period)));
//
//             foreach (var coin in coinsInPeriod)
//             {
//                 var currentPrice = prices[coin].ContainsKey(period!.Value) ? prices[coin][period!.Value] : 0;
//                 var previousPrice = prices[coin].ContainsKey(period.Value.AddDays(-opts.Timeframe)) ? prices[coin][period.Value.AddDays(-opts.Timeframe)] : 0;
//
//                 var portfolioCoin = portfolio.GetCoin(coin);
//
//                 //Buy
//                 bool shouldBuy = ShouldBuy(currentPrice, previousPrice, opts.PurchaseThresholdPercent);
//                 if (shouldBuy) portfolioCoin = PurchaseCoinAndUpdateValues(portfolio, coin, currentPrice, opts, period.Value);
//
//                 //Sell
//                 if (portfolioCoin == null) continue;
//                 if (opts.TrailingTakeProfit) SellOrUpdateTrailing(portfolio, portfolioCoin, currentPrice, opts.TakeProfitPercent, opts.StopLossPercent, period.Value);
//                 else CheckAndMakeSale(portfolio, portfolioCoin, currentPrice, opts.TakeProfitPercent, opts.StopLossPercent, period.Value);
//             }
//
//             portfolio.TakeSnapshot(period.Value, GetPriceDictionary(prices, coinsInPeriod, period.Value));
//             period = period.Value.AddDays(1);
//             if (period.Value > opts.EndDate) period = null;
//         }
//         return portfolio;
//     }
//
//     private static void CheckAndMakeSale(IPortfolio portfolio, PortfolioCoin coin, decimal price, float tp, float sl, DateTime day)
//     {
//         var shouldSell = coin.AveragePurchasePrice / price * 100 > (decimal)tp;
//         if (shouldSell)
//         {
//             portfolio.Sell(coin.Symbol, price, day);
//             coin.TakeProfitValue = 0;
//             coin.StopLossValue = 0;
//         }
//     }
//
//     private static void SellOrUpdateTrailing(IPortfolio portfolio, PortfolioCoin coin, decimal price, float tp, float sl, DateTime day)
//     {
//         if (coin.StopLossValue >= price) portfolio.Sell(coin.Symbol, price, day);
//         else
//         {
//             if (coin.TakeProfitValue <= price)
//             {
//                 coin.TakeProfitValue = price * ((100 + (decimal)tp) / 100);
//                 coin.StopLossValue = price * ((100 - (decimal)sl) / 100);
//             }
//         }
//     }
//
//     private static PortfolioCoin? PurchaseCoinAndUpdateValues(IPortfolio portfolio, string coin, decimal price, BuyTheDipOptions opts, DateTime day)
//     {
//         var success = portfolio.Buy(coin, price, opts.FiatPurchaseAmount, day);
//         var portfolioCoin = portfolio.GetCoin(coin);
//         if (opts.TrailingTakeProfit && success)
//         {
//             portfolioCoin = portfolio.GetCoin(coin)!;
//             portfolioCoin.TakeProfitValue = price * ((100 + (decimal)opts.TakeProfitPercent) / 100);
//             portfolioCoin.StopLossValue = price * ((100 - (decimal)opts.StopLossPercent) / 100);
//         }
//         return portfolioCoin;
//     }
//
//     private static bool ShouldBuy(decimal currentPrice, decimal previousPrice, float threshold)
//     {
//         try
//         {
//             return currentPrice != 0 && previousPrice != 0 && 100 - (currentPrice / previousPrice * 100) > (decimal)threshold && 100 - (currentPrice / previousPrice * 100) < 99m;
//         }
//         catch
//         {
//             return false;
//         }
//     }
//
//     private Dictionary<string, decimal> GetPriceDictionary(Dictionary<string, Dictionary<DateTime, decimal>> dict, HashSet<string> keys, DateTime date)
//     {
//         var dictionary2 = new Dictionary<string, decimal>(dict.Count);
//         foreach (var (key, value) in dict)
//         {
//             if (!keys.Contains(key)) continue;
//             if (!value.ContainsKey(date)) continue;
//             dictionary2.Add(key, value[date]);
//         }
//         return dictionary2;
//     }
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//     public IPortfolio Run(BuyTheDipOptions opts)
//     {
//         using var scope = _serviceProvider.CreateScope();
//         var portfolio = scope.ServiceProvider.GetRequiredService<IPortfolio>();
//
//         using var db = _db;
//         DateTime? period = opts.StartDate;
//         var mcapData = db.MarketCapRankings.Where(x => x.Date >= opts.StartDate && x.Date <= opts.EndDate).ToList();
//         var allCoins = mcapData.SelectMany(x => x.Coins).Distinct();
//         var prices = db.CoinPrices.Where(x => allCoins.Contains(x.CoinSymbol + "-" + x.CoinName)).AsEnumerable().GroupBy(x => $"{x.CoinSymbol}-{x.CoinName}").ToDictionary(x => x.Key, x => x.ToDictionary(x => x.Date, x => x));
//
//         while (period != null)
//         {
//             var coinsInPeriod = mcapData.FirstOrDefault(x => x.Date == period)?.Coins.Where(x => prices.ContainsKey(x) && prices[x].ContainsKey((DateTime)period)).ToList();
//             if (coinsInPeriod == null) continue;
//
//             foreach (var coin in coinsInPeriod)
//             {
//                 bool shouldBuy = false;
//                 try
//                 {
//                     if (prices[coin].ContainsKey(period.Value.AddDays(-opts.Timeframe))) shouldBuy = 100 - (prices[coin][(DateTime)period].Price / prices[coin][period.Value.AddDays(-opts.Timeframe)].Price * 100) > (decimal)opts.PurchaseThresholdPercent;
//                     shouldBuy = shouldBuy && prices[coin][(DateTime)period].Price != 0;
//                     shouldBuy = shouldBuy && prices[coin][period.Value.AddDays(-opts.Timeframe)].Price != 0;
//                 }
//                 catch { }
//                 var portfolioCoin = portfolio.GetCoin(coin);
//                 if (shouldBuy)
//                 {
//                     var success = portfolio.Buy(coin, prices[coin][(DateTime)period].Price, opts.FiatPurchaseAmount, period.Value);
//                     if (opts.TrailingTakeProfit && success)
//                     {
//                         portfolioCoin = portfolio.GetCoin(coin)!;
//                         portfolioCoin.TakeProfitValue = portfolioCoin.AveragePurchasePrice * ((100 + (decimal)opts.TakeProfitPercent) / 100);
//                         portfolioCoin.StopLossValue = portfolioCoin.AveragePurchasePrice * ((100 - (decimal)opts.StopLossPercent) / 100);
//                     }
//                 }
//                 if (portfolioCoin == null || portfolioCoin.CurrentPurchases.Count == 0) continue;
//                 if (opts.TrailingTakeProfit)
//                 {
//                     if (portfolioCoin.StopLossValue >= prices[coin][(DateTime)period].Price) portfolio.Sell(coin, prices[coin][(DateTime)period].Price, period.Value);
//                     else
//                     {
//                         if (portfolioCoin.TakeProfitValue <= prices[coin][(DateTime)period].Price)
//                         {
//                             portfolioCoin.TakeProfitValue = prices[coin][(DateTime)period].Price * ((100 + (decimal)opts.TakeProfitPercent) / 100);
//                             portfolioCoin.StopLossValue = prices[coin][(DateTime)period].Price * ((100 - (decimal)opts.StopLossPercent) / 100);
//                         }
//                     }
//                 }
//                 else
//                 {
//                     var shouldSell = portfolioCoin.AveragePurchasePrice / prices[coin][(DateTime)period].Price * 100 > (decimal)opts.TakeProfitPercent;
//                     if (!shouldSell) continue;
//                     portfolio.Sell(coin, prices[coin][(DateTime)period].Price, period.Value);
//                 }
//             }
//
//             portfolio.TakeSnapshot(period.Value, prices.Where(x => coinsInPeriod.Contains(x.Key) && x.Value.ContainsKey(period.Value)).ToDictionary(x => x.Key, x => x.Value[period.Value].Price));
//             period = period.Value.AddDays(1);
//             if (period.Value > opts.EndDate) period = null;
//         }
//         return portfolio;
//     }
// }
