namespace Crypto_Tools.Models;
public class CoinProfit
{
    public string Name { get; set; }
    public float Profit { get; set; }

    public CoinProfit(string name, float profit) => (Name, Profit) = (name, profit);
}

