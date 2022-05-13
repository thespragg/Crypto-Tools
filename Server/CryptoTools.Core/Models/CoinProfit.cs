namespace CryptoTools.Core.Models;
public class CoinProfit
{
    public string Name { get; set; }
    public decimal Profit { get; set; }
    public CoinProfit(string name, decimal profit) => (Name, Profit) = (name, profit);
}

