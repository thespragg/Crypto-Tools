namespace CryptoTools.Core.Models;

public class PortfolioSnapshot
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal Spent { get; set; }
    public decimal Profit
    {
        get => Value - Spent;
    }
}