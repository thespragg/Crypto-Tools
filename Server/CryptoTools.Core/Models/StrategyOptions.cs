using CryptoTools.Core.Enums;

namespace CryptoTools.Core.Models;

public class StrategyOptions
{
    public decimal? FiatPurchaseAmount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? MarketCapRankingMax { get; set; }
    public float? StopLossPercent { get; set; }
    public float? TakeProfitPercent { get; set; }
    public bool? TrailingTakeProfit { get; set; }
    public float? PurchaseThresholdPercent { get; set; }
    public Dictionary<string, float>? CoinPurchaseAllocations { get; set; }
    public DcaInterval? DcaInterval { get; set; }
    public List<string>? IgnoredCoins { get; set; }
    public int? Timeframe { get; set; }
}
