
using CryptoTools.Core.Enums;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class Trade: ITrade
{
    public DateTime Date { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public float Quantity { get; init; }
    public decimal Price { get; init; }
    public TradeDirection Direction { get; init; }
    public decimal Value => (decimal)Quantity * Price;
}
