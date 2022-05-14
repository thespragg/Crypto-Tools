namespace CryptoTools.Core.Models;

public class PortfolioCoin
{
    public string Symbol { get; private set; }
    public float Quantity { get => Purchases.Sum(x => x.Quantity); }
    public List<CoinPurchase> Purchases { get; set; }
    public decimal Spent
    {
        get => Purchases.Sum(x => (decimal)x.Quantity * x.Price);
    }
    public decimal AveragePurchasePrice
    {
        get => Spent / (decimal)Quantity;
    }
    public PortfolioCoin(string symbol) => (Symbol, Purchases) = (symbol, new List<CoinPurchase>());
    public void RemoveQuantity(float? qty = null)
    {
        if (qty == null) qty = Quantity;
        for (var i = 0; i < Purchases.Count; i++)
        {
            if (qty > Purchases[i].Quantity)
            {
                qty -= Purchases[i].Quantity;
                Purchases.RemoveAt(i);
            }
            else
            {
                Purchases.Last().Quantity -= qty.Value;
                return;
            }
        }
    }
}
