using CryptoTools.Core.Models;

namespace CryptoTools.Core.Interfaces;

public interface IPortfolio
{
    decimal Balance { get; }
    void AddCash(decimal qty);
    void RemoveCash(decimal qty);
    Dictionary<string, IPortfolioCoin> Coins { get; }
    bool Buy(string symbol, decimal price, decimal spent, DateTime day);
    bool Sell(string symbol, decimal price, DateTime day, float? quantity = null);
}