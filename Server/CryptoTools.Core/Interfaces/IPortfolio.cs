using CryptoTools.Core.Models;

namespace CryptoTools.Core.Interfaces;

public interface IPortfolio
{
    Dictionary<string, PortfolioCoin> GetCoins();
    PortfolioCoin? GetCoin(string symbol);
    decimal GetValue(DateTime date);
    decimal GetSpent();
    decimal GetProfit(DateTime date);
    List<PortfolioSnapshot> GetSnapshots();
    void TakeSnapshot(DateTime date);
    void TakeSnapshot(DateTime date, Dictionary<string, decimal> prices);
    void Buy(string symbol, decimal price, decimal spent);
    void Sell(string symbol, decimal price, float? quantity = null);
}
