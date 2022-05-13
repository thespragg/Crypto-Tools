using CryptoTools.Core.Models;

namespace CryptoTools.Core.Strategies;

public interface ITradeStrategy<T>
{
    SimulationResult Run(StrategyOptions opts);
    bool ValidateOptions(StrategyOptions opts);
}
