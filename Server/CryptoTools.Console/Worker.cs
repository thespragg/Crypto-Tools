using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using CryptoTools.Core.Strategies.StrategyOptions;
using System.Globalization;
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(() =>
              {
                  var runs = 1000;
                  var results = new List<Results>();
                  for (var i = 0; i < runs; i++)
                  {
                      _logger.LogInformation("Starting {i}/{runs}", i, runs);
                      var opts = GenerateOptions();
                      using var scope = _serviceProvider.CreateScope();
                      var _strategy = scope.ServiceProvider.GetRequiredService<BuyTheDip>();
                      var result = _strategy.Run(opts);
                      results.Add(new Results
                      {
                          Options = opts,
                          Profit = result.GetSnapshots().Last().Profit,
                          Unsold = result.GetCoins().Count(x => x.Value.CurrentPurchases.Count != 0),
                          Profitable = (result.GetCoins().Where(x => (x.Value.Value + x.Value.SoldValue) - x.Value.Spent > 0).Count() / (float)result.GetCoins().Count) * 100,
                          AverageProfit = result.GetCoins().Sum(x => (x.Value.Value + x.Value.SoldValue) - x.Value.Spent) / result.GetCoins().Count
                      });
                      GC.Collect();
                  }

                  foreach (var result in results.OrderByDescending(x => x.Profit).Take(20))
                  {
                      _logger.LogInformation("===================================================");
                      _logger.LogInformation("Threshold: {thresh}, Timeframe: {frame}, Mcap: {mcap}, TP: {tp}, SL: {sl}", result.Options.PurchaseThresholdPercent, result.Options.Timeframe, result.Options.MarketCapRankingMax, result.Options.TakeProfitPercent, result.Options.StopLossPercent);
                      _logger.LogInformation("Profit: {profit}, Unsold: {unsold}, Profitable: {profitable}%, Average Profit: {avg}", result.Profit, result.Unsold, result.Profitable, result.AverageProfit);
                      _logger.LogInformation("===================================================");
                  }
                  PrintResults(results);
              }, cancellationToken);
        }

        private void PrintResults(List<Results> results)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Amount,Purchase Threshold,Timeframe,Max Market Cap,Take Profit,Stop Loss, Profit, Profitable Percent");
            foreach(var result in results)
            {
                sb.AppendLine($"{result.Options.FiatPurchaseAmount},{result.Options.PurchaseThresholdPercent},{result.Options.Timeframe},{result.Options.MarketCapRankingMax},{result.Options.TakeProfitPercent},{result.Options.StopLossPercent},{result.Profit},{result.Profitable}");
            }
            File.WriteAllText("C:\\StrategyData\\Data.csv", sb.ToString());
        }

        private BuyTheDipOptions GenerateOptions()
        {
            var rnd = new Random();
            return new BuyTheDipOptions
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                FiatPurchaseAmount = 10,
                PurchaseThresholdPercent = rnd.Next(2, 10) * 10,
                Timeframe = rnd.Next(2, 10),
                MarketCapRankingMax = rnd.Next(1, 6) * 100,
                TakeProfitPercent = rnd.Next(1, 100),
                TrailingTakeProfit = true,
                StopLossPercent = rnd.Next(1, 100)
            };
        }

        public class Results
        {
            public BuyTheDipOptions Options { get; set; }
            public decimal Profit { get; set; }
            public int Unsold { get; set; }
            public float Profitable { get; set; }
            public decimal AverageProfit { get; set; }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}