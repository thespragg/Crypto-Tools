
namespace CryptoTools.Core.Models;

public class Trade
{
    public DateTime Date { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public float Quantity { get; set; }
    public decimal Price { get; set; }
    public TradeDirection Direction { get; set; }
    public decimal Value {  get => (decimal)Quantity * Price; }
}
