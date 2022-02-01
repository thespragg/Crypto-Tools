namespace Crypto_Tools.Models;

public class PortfolioSnapshot
{
    public DateTime Date { get; set; }
    public float Value { get; set; }
    public float Spent { get; set; }
    public float Profit
    {
        get
        {
            return Value - Spent;
        }
    }
}