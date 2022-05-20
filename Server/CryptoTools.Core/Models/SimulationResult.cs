using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Strategies.StrategyOptions;

namespace CryptoTools.Core.Models;

public class SimulationResult
{
    public BuyTheDipOptions Options { get; set; }
    public decimal RealizedProfit { get; set; }
    public decimal TotalProfit { get; set; }
    public int TotalTrades { get; set; }
    public decimal CashBalance { get; set; }
    public int ProfitableTrades { get; set; }
    public int TotalSales { get; set; }
    public decimal AverageProfit { get; set; }
    public decimal AverageLoss { get; set; }
    public List<Trade> Trades { get; set; }
    public static SimulationResult FromIPortfolio(IPortfolio portfolio, BuyTheDipOptions opts)
    {
        var res = new SimulationResult
        {
            Options = opts,
            RealizedProfit = portfolio.GetSnapshots().Last().CashValue - portfolio.StartingBalance,
            TotalProfit = portfolio.GetSnapshots().Last().TotalValue - portfolio.StartingBalance,
            TotalTrades = portfolio.TotalTrades,
            CashBalance = portfolio.CashBalance,
            TotalSales = portfolio.GetSnapshots().Last().TotalSales,
            ProfitableTrades = portfolio.GetSnapshots().Last().ProfitableSales,
            AverageLoss = portfolio.GetSnapshots().Last().AverageLoss,
            AverageProfit = portfolio.GetSnapshots().Last().AverageProfit,
            Trades = portfolio.TradeList
        };
        return res;
    }
}
