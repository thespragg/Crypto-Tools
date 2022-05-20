using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class Portfolio : IPortfolio
{
    public decimal StartingBalance { get; private set; } = 500;
    public Dictionary<string, PortfolioCoin> Coins { get; set; } = new Dictionary<string, PortfolioCoin>();
    public Dictionary<string, PortfolioCoin> CoinList { get => Coins; }
    private decimal SoldValue { get; set; } = 0;
    public decimal CashBalance { get; private set; }
    private List<PortfolioSnapshot> Snapshots { get; set; } = new List<PortfolioSnapshot>();
    private readonly CryptoToolsDbContext _db;
    public Portfolio(CryptoToolsDbContext db) => (_db, CashBalance) = (db, StartingBalance);
    private int NumberOfTrades { get; set; } = 0;
    private List<Trade> Trades { get; set; } = new List<Trade>();
    public int TotalTrades { get => NumberOfTrades; }
    public List<Trade> TradeList { get=> Trades; }
    public Dictionary<string, PortfolioCoin> GetCoins() => Coins;
    public PortfolioCoin? GetCoin(string symbol)
    {
        if(!Coins.ContainsKey(symbol)) return null;
        return Coins[symbol];
    }
    private decimal GetValue(DateTime date)
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

    private decimal GetSpent() => Coins.Values.Sum(x => x.Spent);

    public bool Buy(string symbol, decimal price, decimal spent, DateTime day)
    {
        if(spent > CashBalance) return false;
        CashBalance -= spent;
        if (Coins.ContainsKey(symbol)) Coins[symbol].AddPurchase((float)(spent / price), price);
        else
        {
            var newCoin = new PortfolioCoin(symbol);
            newCoin.AddPurchase((float)(spent / price), price);
            Coins.Add(symbol, newCoin);
            NumberOfTrades += 1;
        }
        Trades.Add(new Trade
        {
            Symbol = symbol,
            Quantity = (float)(spent / price),
            Price = price,
            Date = day,
            Direction = TradeDirection.BUY
        });
        return true;
    }

    public void Sell(string symbol, decimal price, DateTime day, float? quantity = null)
    {
        if (quantity == null) quantity = Coins[symbol].CurrentQuantity;
        SoldValue += (decimal)quantity.Value * price;
        Coins[symbol].RemoveQuantity();
        Coins[symbol].SoldValue += (decimal)quantity.Value * price;
        CashBalance += (decimal)quantity.Value * price;

        Trades.Add(new Trade
        {
            Symbol = symbol,
            Quantity = quantity.Value,
            Price = price,
            Date = day,
            Direction = TradeDirection.SELL
        });
    }

    public List<PortfolioSnapshot> GetSnapshots() => Snapshots;

    public void TakeSnapshot(DateTime date) => Snapshots.Add(new PortfolioSnapshot
    {
        Date = date,
        Spent = GetSpent(),
        CoinValue = GetValue(date),
        CashValue = CashBalance
    });

    public void TakeSnapshot(DateTime date, Dictionary<string, decimal> prices) => Snapshots.Add(new PortfolioSnapshot
    {
        Date = date,
        Spent = GetSpent(),
        CoinValue = GetValue(date, prices),
        CashValue = CashBalance
    });

    public decimal GetCashBalance() => CashBalance;

    public decimal SetCashBalance(decimal balance) => CashBalance = balance;
}
