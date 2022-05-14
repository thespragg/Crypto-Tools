using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;

namespace CryptoTools.Core.Strategies;

public interface ITradeStrategy<T>
{
    IPortfolio? Run(StrategyOptions opts);
    bool ValidateOptions(StrategyOptions opts);
}
