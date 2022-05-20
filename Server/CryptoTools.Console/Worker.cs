using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using CryptoTools.Core.Strategies.StrategyOptions;
using System.Reflection;
using System.Text;

namespace CryptoTools.Console
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
              {
                  using var scope = _serviceProvider.CreateScope();
                  var _strategy = scope.ServiceProvider.GetRequiredService<BuyTheDip>();

                  //var opts = OptionsGenerator.SingleOption();
                  //var opts = OptionsGenerator.FromFileAsSingle("Best.csv");
                  var opts = OptionsGenerator.FromFileWithRandomDates("Best.csv",50); //.Take(1).ToDictionary(x=>x.Key, x=>x.Value);
                  //var opts = OptionsGenerator.RandomOptions(100);
                  //var opts = OptionsGenerator.RandomOptionsWithRandomDates(100, 50);

                  var results = await _strategy.RunMany(opts);
                  PrintResults(results);
                  Environment.Exit(0);
              }, cancellationToken);

            return Task.CompletedTask;
        }

        private void PrintTrades(Dictionary<int, List<SimulationResult>> results)
        {
            var sb = new StringBuilder();
            var trades = results.First().Value.First().Trades;
            sb.AppendLine("Date,Symbol,Price,Quantity,Value,Direction");
            var tradeDict = trades.GroupBy(x => x.Symbol).ToDictionary(x => x.Key, x => x.OrderBy(x => x.Date));
            foreach (var (key, value) in tradeDict)
            {
                foreach (var val in value)
                {
                    sb.AppendLine($"{val.Date},{val.Symbol},{val.Price},{val.Quantity},{val.Value},{val.Direction.ToString()}");
                }

            }
            File.WriteAllText($"C:\\StrategyData\\Trades.csv", sb.ToString());
        }

        private void PrintTrades(SimulationResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Date,Symbol,Price,Quantity,Value,Direction");
            var tradeDict = result.Trades.GroupBy(x => x.Symbol).ToDictionary(x => x.Key, x => x.OrderBy(x => x.Date));
            foreach (var (key, value) in tradeDict)
            {
                foreach (var val in value)
                {
                    sb.AppendLine($"{val.Date},{val.Symbol},{val.Price},{val.Quantity},{val.Value},{val.Direction.ToString()}");
                }

            }
            File.WriteAllText($"C:\\StrategyData\\Trades.csv", sb.ToString());
        }

        private void PrintResults(Dictionary<int, List<SimulationResult>> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Amount,Purchase Threshold,Timeframe,Max Market Cap,Take Profit,Stop Loss,Realised Profit,Total Profit,Profitable Percent,Number of trades,TotalProfitable, TotalSales, Average Profit, Average Loss");
            foreach (var (key, res) in results)
            {
                var max = res.OrderByDescending(x => x.RealizedProfit).First();
                PrintTrades(max);
                sb.AppendLine($"{res.First().Options.FiatPurchaseAmount},{res.First().Options.PurchaseThresholdPercent},{res.First().Options.Timeframe},{res.First().Options.MarketCapRankingMax},{res.First().Options.TakeProfitPercent},{res.First().Options.StopLossPercent},{res.Sum(x => x.RealizedProfit) / res.Count},{res.Sum(x => x.TotalProfit) / res.Count},{res.Count(x => x.RealizedProfit > 0) / (float)res.Count}, {res.Sum(x => x.TotalTrades) / res.Count}, {res.Sum(x=>x.ProfitableTrades)/ res.Count*100}, {res.Sum(x => x.TotalSales) / res.Count * 100}, {res.Sum(x => x.AverageProfit) / res.Count * 100}, {res.Sum(x => x.AverageLoss) / res.Count * 100}");
            }

            File.WriteAllText($"C:\\StrategyData\\Top{results.Count}.csv", sb.ToString());
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}