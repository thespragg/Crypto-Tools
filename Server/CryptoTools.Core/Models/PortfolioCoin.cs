namespace CryptoTools.Core.Models;

public class PortfolioCoin
{
    public string Symbol { get; private set; }
    public float CurrentQuantity { get => CurrentPurchases.Sum(x => x.Quantity); }
    public float TotalQuantity { get => AllPurchases.Sum(x => x.Quantity); }
    public List<CoinPurchase> CurrentPurchases { get; private set; }
    public List<CoinPurchase> AllPurchases { get; private set; }
    public decimal Value { get; set; }
    public decimal Spent
    {
        get => CurrentPurchases.Sum(x => (decimal)x.Quantity * x.Price);
    }
    public decimal AveragePurchasePrice
    {
        get => Spent / (decimal)CurrentQuantity;
    }
    public PortfolioCoin(string symbol) => (Symbol, CurrentPurchases, AllPurchases) = (symbol, new List<CoinPurchase>(), new List<CoinPurchase>());
    public void AddPurchase(CoinPurchase purchase)
    {
        CurrentPurchases.Add(purchase);
        AllPurchases.Add(purchase);
    }

    public void RemoveQuantity(float? qty = null)
    {
        if (qty == null) qty = CurrentQuantity;
        for (var i = 0; i < CurrentPurchases.Count; i++)
        {
            if (qty > CurrentPurchases[i].Quantity)
            {
                qty -= CurrentPurchases[i].Quantity;
                CurrentPurchases.RemoveAt(i);
            }
            else
            {
                CurrentPurchases.Last().Quantity -= qty.Value;
                return;
            }
        }
    }
}
