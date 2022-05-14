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
            value += (decimal)coin.Quantity * price.Price;
        }
        return value + SoldValue;
    }

    public decimal GetSpent() => Coins.Values.Sum(x => x.Spent);

    public decimal GetProfit(DateTime date) => GetValue(date) - GetSpent();

    public void Buy(string symbol, decimal price, decimal spent)
    {
        var purchase = new CoinPurchase((float)(spent / price), price);
        if (Coins.ContainsKey(symbol)) Coins[symbol].Purchases.Add(purchase);
        else Coins.Add(symbol, new PortfolioCoin(symbol)
        {
            Purchases = new List<CoinPurchase> { purchase }
        });
    }

    public void Sell(string symbol, decimal price, float? quantity = null)
    {
        if (quantity == null) quantity = Coins[symbol].Quantity;
        SoldValue += (decimal)quantity.Value * price;
        Coins[symbol].RemoveQuantity(quantity);
    }

    public List<PortfolioSnapshot> GetSnapshots() => Snapshots;

    public void TakeSnapshot(DateTime date) => Snapshots.Add(new PortfolioSnapshot
    {
        Date = date,
        Spent = GetSpent(),
        Value = GetValue(date)
    });
}
