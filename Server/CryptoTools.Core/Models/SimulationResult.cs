namespace CryptoTools.Core.Models;

public class SimulationResult
{
    public List<PortfolioSnapshot> Snapshots { get; set; }
    public List<CoinProfit> Coins { get; set; }
    public SimulationResult(List<PortfolioSnapshot> snapshots, List<CoinProfit> profits) => (Snapshots, Coins) = (snapshots, profits);
}

