using CryptoTools.Core.Models;

namespace CryptoTools.Core.Interfaces;

public interface IPortfolio
{
    int TotalTrades { get; }
    List<Trade> TradeList { get; }
    decimal CashBalance { get; }
    decimal StartingBalance { get; }
    Dictionary<string, PortfolioCoin> CoinList { get; }
    PortfolioCoin? GetCoin(string symbol);
    List<PortfolioSnapshot> GetSnapshots();
    void TakeSnapshot(DateTime date);
    void TakeSnapshot(DateTime date, Dictionary<string, decimal> prices);
    bool Buy(string symbol, decimal price, decimal spent, DateTime day);
    void Sell(string symbol, decimal price, DateTime day, float? quantity = null);
}
