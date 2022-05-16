namespace CryptoTools.Core.Strategies.StrategyOptions;

public class BuyTheDipOptions
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public float TakeProfitPercent { get; set; }
    public float PurchaseThresholdPercent { get; set; }
    public decimal FiatPurchaseAmount { get; set; }
    public int MarketCapRankingMax { get; set; }
    public int Timeframe { get; set; }
    public bool TrailingTakeProfit { get; set; }
    public float StopLossPercent { get; set; }
}
