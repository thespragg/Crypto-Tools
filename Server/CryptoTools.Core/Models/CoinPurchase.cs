namespace CryptoTools.Core.Models;

public class CoinPurchase
{
    public float Quantity { get; set; }
    public decimal Price { get; set; }
    public CoinPurchase(float qty, decimal price) => (Quantity, Price) = (qty, price);
}
