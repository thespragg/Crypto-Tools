namespace CryptoTools.Core.Interfaces;

public interface IPortfolioCoin
{
    List<ITrade> Trades { get; }
    float Quantity { get; }
}