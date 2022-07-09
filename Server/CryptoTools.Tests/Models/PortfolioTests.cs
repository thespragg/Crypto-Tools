using CryptoTools.Core.Models;

namespace CryptoTools.Tests.Models;

public class PortfolioTests
{
    private const int StartingBalance = 500;
    [Fact]
    public void Balance_ShouldBeSetInConstructor()
    {
        var portfolio = new Portfolio(StartingBalance);
        Assert.Equal(StartingBalance, portfolio.Balance);
    }

    [Fact]
    public void AddCash_ShouldAddToBalance()
    {
        const int addition = 100;
        var portfolio = new Portfolio(StartingBalance);
        portfolio.AddCash(addition);
        Assert.Equal(addition + StartingBalance, portfolio.Balance);
    }

    [Fact]
    public void RemoveCash_ShouldSubtractFromBalance()
    {
        const int subtraction = 100;
        var portfolio = new Portfolio(StartingBalance);
        portfolio.RemoveCash(subtraction);
        Assert.Equal(StartingBalance - subtraction, portfolio.Balance);
    }

    [Fact]
    public void BuyingANewCoin_ShouldAddItToDictionary()
    {
        var portfolio = new Portfolio(StartingBalance);
        portfolio.Buy("BTC", 100, 10, DateTime.Now);
        Assert.True(portfolio.Coins.ContainsKey("BTC"));
    }

    [Fact]
    public void BuyingANewCoin_ShouldInsertATradeIntoTheCoinDictionary()
    {
        var portfolio = new Portfolio(StartingBalance);
        portfolio.Buy("BTC", 100, 10, DateTime.Now);
        Assert.Single(portfolio.Coins["BTC"].Trades);
    }
}