using CryptoTools.Core.Enums;

namespace CryptoTools.Core.Interfaces;

public interface ITrade
{
    DateTime Date { get; }
    string Symbol { get; }
    float Quantity { get; }
    decimal Price { get; }
    TradeDirection Direction { get; }
    decimal Value { get; }
}