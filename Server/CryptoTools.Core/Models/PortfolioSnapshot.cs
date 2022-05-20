namespace CryptoTools.Core.Models;

public class PortfolioSnapshot
{
    public DateTime Date { get; set; }
    public decimal CoinValue { get; set; }
    public decimal CashValue { get; set; }
    public decimal Spent { get; set; }
    public int ProfitableSales { get; set; }
    public int TotalSales { get; set; }
    public decimal AverageProfit { get; set; }
    public decimal AverageLoss { get; set; }
    public int ProfitablePercent { get => ProfitableSales / TotalSales * 100; }
    public decimal TotalValue
    {
        get => (CoinValue + CashValue);
    }
}