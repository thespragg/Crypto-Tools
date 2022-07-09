using CryptoTools.Core.Enums;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class PortfolioCoin : IPortfolioCoin
{
    public List<ITrade> Trades { get; } = new();
    public float Quantity => Trades.Sum(x => x.Direction == TradeDirection.Buy ? x.Quantity : -x.Quantity);
}