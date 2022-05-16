using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class Portfolio : IPortfolio
{
    public Dictionary<string, PortfolioCoin> Coins { get; set; } = new Dictionary<string, PortfolioCoin>();
    private decimal SoldValue { get; set; } = 0;
    private List<PortfolioSnapshot> Snapshots { get; set; } = new List<PortfolioSnapshot>();
    private readonly CryptoToolsDbContext _db;
    public Portfolio(CryptoToolsDbContext db) => (_db) = (db);

    public Dictionary<string, PortfolioCoin> GetCoins() => Coins;
    public PortfolioCoin? GetCoin(string symbol)
    {
        if(!Coins.ContainsKey(symbol)) return null;
        return Coins[symbol];
    }
    public decimal GetValue(DateTime date)
    {
        var value = (decimal)0;
        foreach (var (key, coin) in Coins)
        {
            var price = _db.CoinPrices.FirstOrDefault(x => x.CoinSymbol == key && x.Date == date);
            if (price == null) continue;
            value += (decimal)coin.CurrentQuantity * price.Price;
            coin.Value = (decimal)coin.CurrentQuantity * price.Price;
        }
        return value + SoldValue;
    }

    private decimal GetValue(DateTime date, Dictionary<string, decimal> prices)
    {
        var value = (decimal)0;
        foreach (var (key, coin) in Coins)
        {
            var success = prices.TryGetValue(key, out var price);
            if (!success) continue;
            value += (decimal)coin.CurrentQuantity * price;
            coin.Value = (decimal)coin.CurrentQuantity * price;
        }
        return value + SoldValue;
    }

    public decimal GetSpent() => Coins.Values.Sum(x => x.Spent);

    public decimal GetProfit(DateTime date) => GetValue(date) - GetSpent();

    public void Buy(string symbol, decimal price, decimal spent)
    {
        if (Coins.ContainsKey(symbol)) Coins[symbol].AddPurchase((float)(spent / price), price);
        else
        {
            var newCoin = new PortfolioCoin(symbol);
            newCoin.AddPurchase((float)(spent / price), price);
            Coins.Add(symbol, newCoin);
        }
    }

    public void Sell(string symbol, decimal price, float? quantity = null)
    {
        if (quantity == null) quantity = Coins[symbol].CurrentQuantity;
        SoldValue += (decimal)quantity.Value * price;
        Coins[symbol].RemoveQuantity(quantity);
        Coins[symbol].SoldValue += (decimal)quantity.Value * price;
    }

    public List<PortfolioSnapshot> GetSnapshots() => Snapshots;

    public void TakeSnapshot(DateTime date) => Snapshots.Add(new PortfolioSnapshot
    {
        Date = date,
        Spent = GetSpent(),
        Value = GetValue(date)
    });

    public void TakeSnapshot(DateTime date, Dictionary<string, decimal> prices) => Snapshots.Add(new PortfolioSnapshot
    {
        Date = date,
        Spent = GetSpent(),
        Value = GetValue(date, prices)
    });
}
