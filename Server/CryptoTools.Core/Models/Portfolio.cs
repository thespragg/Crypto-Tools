using CryptoTools.Core.DAL;

namespace CryptoTools.Core.Models;

public class Portfolio
{
    public List<PortfolioCoin> Coins { get; set; } = new List<PortfolioCoin>();
    public decimal CoinValue { get; set; }
    public decimal SoldValue { get; set; }
    public decimal Spent { get; set; }
    public decimal PortfolioValue { get => SoldValue + CoinValue; }
    public decimal PortfolioProfit { get => PortfolioValue - Spent; }

    public void Buy(string symbol, decimal price, decimal spent)
    {
        var coin = Coins.FirstOrDefault(x => x.Symbol == symbol);
        if (coin == null)
        {
            coin = new PortfolioCoin(symbol);
            Coins.Add(coin);
        }
        var purchaseQuantity = spent / price;
        coin.Purchases.Add(new CoinPurchase((float)purchaseQuantity, price));
        Spent += spent;
    }

    public void Sell(string symbol, decimal price, float? quantity = null)
    {
        var coin = Coins.FirstOrDefault(x => x.Symbol == symbol);
        if (quantity == null)
        {
            var totalQuantity = coin!.Purchases.Sum(x => x.Quantity);
            var value = (decimal)totalQuantity * price;
            SoldValue += value;
            Coins.Remove(coin);
        }
        else
        {
            SoldValue += (decimal)quantity.Value * price;
            var purchases = new List<CoinPurchase>(coin!.Purchases);
            purchases.Reverse();
            foreach(var purchase in purchases)
            {
                if (quantity - purchase.Quantity >= 0)
                {
                    coin.Purchases.Remove(purchase);
                    quantity -= purchase.Quantity;
                }
                else coin.Purchases.Last().Quantity -= quantity.Value;
            }
        }
    }

    public void UpdatePortfolioValue(DateTime date)
    {
        var db = new CryptoToolsDbContext();
        foreach (var coin in Coins)
        {
            var price = db.CoinPrices.FirstOrDefault(x => x.CoinSymbol == coin.Symbol && x.Date == date);
            if (price == null) continue;
            coin.Value = (decimal)coin.Purchases.Sum(x => x.Quantity) * price.Price;
        }
        CoinValue = Coins.Sum(x => x.Value);
    }
}
