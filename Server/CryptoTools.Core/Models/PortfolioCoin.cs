namespace CryptoTools.Core.Models;

public class PortfolioCoin
{
    public string Symbol { get; set; }
    public decimal Value { get; set; }
    public List<CoinPurchase> Purchases { get; set; }
    public PortfolioCoin(string symbol) => (Symbol, Purchases) = (symbol, new List<CoinPurchase>());
}
