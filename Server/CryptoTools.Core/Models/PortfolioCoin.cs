namespace CryptoTools.Core.Models;

public class PortfolioCoin
{
    public string Symbol { get; private set; }
    public float CurrentQuantity { get => CurrentPurchases.Sum(x => x.Quantity); }
    public float TotalQuantity { get => AllPurchases.Sum(x => x.Quantity); }
    public decimal StopLossValue { get; set; }
    public decimal TakeProfitValue { get; set; }
    public List<CoinPurchase> CurrentPurchases { get; private set; }
    public List<CoinPurchase> AllPurchases { get; private set; }
    public decimal Value { get; set; }
    public decimal Spent
    {
        get
        {
            var spent = AllPurchases.Sum(x => (decimal)x.Quantity * x.Price);
            return spent;
        }
    }
    public decimal AveragePurchasePrice
    {
        get => CurrentQuantity == 0 ? 0 : Spent / (decimal)CurrentQuantity;
    }
    public decimal SoldValue { get; set; }
    public PortfolioCoin(string symbol) => (Symbol, CurrentPurchases, AllPurchases) = (symbol, new List<CoinPurchase>(), new List<CoinPurchase>());
    public void AddPurchase(float quantity, decimal price)
    {
        CurrentPurchases.Add(new CoinPurchase(quantity, price));
        AllPurchases.Add(new CoinPurchase(quantity, price));
    }

    public void RemoveQuantity(float? qty = null)
    {
        if (qty == null) qty = CurrentQuantity;
        var toRemove = new List<int>();
        for (var i = 0; i < CurrentPurchases.Count; i++)
        {
            if (qty >= CurrentPurchases[i].Quantity)
            {
                qty -= CurrentPurchases[i].Quantity;
                toRemove.Add(i);
            }
            else
            {
                CurrentPurchases.Last().Quantity -= qty.Value;
                break;
            }
        }
        toRemove.Reverse();
        foreach (var i in toRemove)
        {
            CurrentPurchases.RemoveAt(i);
        }
    }
}
