using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class SimulationResult
{
    public IPortfolio Portfolio { get; set; }
    public SimulationResult(IPortfolio portfolio) => (Portfolio) = (portfolio);
}

