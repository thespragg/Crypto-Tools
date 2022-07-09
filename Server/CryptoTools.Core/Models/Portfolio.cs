using CryptoTools.Core.Enums;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class Portfolio : IPortfolio
{
    public Dictionary<string, IPortfolioCoin> Coins { get; } = new();
    public decimal Balance { get; private set; }
    public void AddCash(decimal qty) => Balance += qty;
    public void RemoveCash(decimal qty) => Balance = Math.Max(0, Balance - qty);
    public bool Buy(string symbol, decimal price, decimal spent, DateTime day)
    {
        if (!Coins.ContainsKey(symbol)) Coins.Add(symbol, new PortfolioCoin());
        Coins[symbol].Trades.Add(new Trade
        {
            Date = day,
            Direction = TradeDirection.Buy,
            Price = price,
            Quantity = (float)(spent / price),
            Symbol = symbol
        });
        return true;
    }
    public bool Sell(string symbol, decimal price, DateTime day, float? quantity = null)
    {
        if (!Coins.ContainsKey(symbol)) return false;
        Coins[symbol].Trades.Add(new Trade
        {
            Date = day,
            Direction = TradeDirection.Sell,
            Price = price,
            Quantity = quantity ?? Coins[symbol].Quantity,
            Symbol = symbol
        });
        return true;
    }
    public Portfolio(decimal bal) => (Balance) = (bal);
}