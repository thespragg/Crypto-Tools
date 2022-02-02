using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using Crypto_Tools.Models;

namespace Crypto_Tools.PortfolioStrategies;

public class GenericDCA
{
    private readonly ICoinPriceService _priceService;
    private Dictionary<string, List<TimestampedPrice>> _prices = new();
    public GenericDCA(ICoinPriceService coinPriceService) => (_priceService) = (coinPriceService);
    public async Task<SimulationResult?> Run(List<CoinAllocation> allocations, int amnt, DcaInterval interval, DateTime start, DateTime end)
    {
        var coins = allocations.Select(x => x.Name).Distinct().ToList();
        var lastKnownPrices = new Dictionary<string,float>();
        _prices = await GetPrices(coins, start, end);

        DateTime? period = start;
        if (period == null) return null;
        var periodCount = 0;

        var portfolio = new Dictionary<string, float>();
        var portfolioValue = new List<PortfolioSnapshot>();
        var coinProfits = new Dictionary<string, CoinPriceHolder>();

        foreach (var coin in coins)
        {
            coinProfits.Add(coin, new CoinPriceHolder());
            lastKnownPrices.Add(coin, 0f);
            portfolio.Add(coin, 0f);
        }
        var notSpent = 0f;

        while (period != null)
        {
            periodCount += 1;

            var snapshot = new PortfolioSnapshot()
            {
                Date = (DateTime)period,

            };

            foreach (var coinAllocation in allocations)
            {
                var dcaAmnt = amnt * ((float)coinAllocation.Allocation / 100);
                var currPrice = GetPriceOnDate(coinAllocation.Name, (DateTime)period);
                if (currPrice != null && currPrice != 0)
                {
                    lastKnownPrices[coinAllocation.Name] = (float)currPrice;
                    portfolio[coinAllocation.Name] += dcaAmnt / (float)currPrice;
                    coinProfits[coinAllocation.Name].Spent += dcaAmnt;
                }
                else notSpent += dcaAmnt;
                
                if(currPrice == null || currPrice == 0) currPrice = lastKnownPrices[coinAllocation.Name];
                var value = portfolio[coinAllocation.Name] * (float)currPrice!;
                coinProfits[coinAllocation.Name].Value = value;
                snapshot.Value += value;
            }

            snapshot.Spent = (periodCount * amnt) - notSpent;
            portfolioValue.Add(snapshot);

            period = interval switch
            {
                DcaInterval.weekly => period = period.Value.AddDays(7),
                DcaInterval.monthly => period = period.Value.AddMonths(1),
                _ => null
            };
            if (period > end) period = null;
        }

        return new SimulationResult(portfolioValue, coinProfits.Select(x => new CoinProfit(x.Key, x.Value.Value - x.Value.Spent)).ToList());
    }

    private float? GetPriceOnDate(string coin, DateTime date)
    {
        if (!_prices.ContainsKey(coin)) return null;
        var dict = _prices[coin];
        if (!dict.Any(x => x.Date.Date == date.Date)) return null;
        return dict.FirstOrDefault(x => x.Date.Date == date.Date)!.Price;
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


