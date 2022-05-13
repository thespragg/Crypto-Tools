using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.PortfolioStrategies;

public class OneTimeBuyTheTop
{
    private readonly CryptoToolsDbContext _db;
    public OneTimeBuyTheTop(CryptoToolsDbContext db) => _db = db;
    private Dictionary<string, List<CoinPrice>> _prices { get; set; } = new Dictionary<string, List<CoinPrice>>();
    public SimulationResult? Run(StrategyOptions opts)
    {
        var portfolioValue = new List<PortfolioSnapshot>();
        var portfolio = new Dictionary<string, float>();
        var lastKnownPrices = new Dictionary<string, decimal>();

        var mcapData = _db.MarketCapRankings.Where(x => x.Date >= opts.StartDate && x.Date <= opts.EndDate).ToList().Select(x => { x.Coins = x.Coins.Take((int)opts.MarketCapRankingMax!).ToList(); return x; });

        var coins = mcapData.Select(x => x.Coins.Take((int)opts.MarketCapRankingMax!)).SelectMany(x => x).Distinct().ToList();
        _prices = GetPrices(coins, (DateTime)opts.StartDate!, (DateTime)opts.EndDate!);

        foreach (var period in mcapData)
        {
            var periodCoins = period.Coins.Take((int)opts.MarketCapRankingMax!);
            var missing = periodCoins.Where(x => !opts.IgnoredCoins!.Contains(x) && !portfolio.ContainsKey(x));

            foreach (var coin in missing)
            {
                portfolio.Add(coin, 0f);
                lastKnownPrices.Add(coin, 0);
            }
            foreach (var coin in portfolio)
            {
                var price = GetPriceOnDate(coin.Key, period.Date);
                if (price != null) lastKnownPrices[coin.Key] = (decimal)price;
                else price = lastKnownPrices[coin.Key];

                //Check if its been purchased yet
                if (coin.Value == 0 && price != 0) portfolio[coin.Key] = (float)opts.FiatPurchaseAmount! / (float)price;
            }

            portfolioValue.Add(new PortfolioSnapshot
            {
                Date = period.Date,
                Value = portfolio.Sum(x => x.Value * (float)lastKnownPrices[x.Key]),
                Spent = portfolio.Count * (float)opts.FiatPurchaseAmount!
            });
        }

        return new SimulationResult(portfolioValue, portfolio.Select(x => new CoinProfit(x.Key, (lastKnownPrices[x.Key] * (decimal)x.Value) - (decimal)opts.FiatPurchaseAmount!)).ToList());
    }

    private decimal? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        return _prices[coin].FirstOrDefault(x => x.Date == date)?.Price;
    }

    private Dictionary<string, List<CoinPrice>> GetPrices(IList<string> coins, DateTime start, DateTime end) => _db.CoinPrices.Where(x => coins.Contains(x.CoinSymbol) && x.Date >= start && x.Date <= end).GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.ToList());
}
