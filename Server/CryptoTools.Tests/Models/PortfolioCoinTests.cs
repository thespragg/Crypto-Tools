using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;

namespace CryptoTools.Tests.Models;

public class PortfolioCoinTests
{
    [Fact]
    public void AddingBuyTrade_ShouldIncreaseQuantity()
    {
        var coin = new PortfolioCoin();
        coin.Trades.Add(new Trade
        {
            Date = DateTime.Now,
            Direction = TradeDirection.Buy,
            Price = 100,
            Quantity = 1,
            Symbol = "Test"
        });
        Assert.Equal(1, coin.Quantity);
    }
    
    [Fact]
    public void AddingSellTrade_ShouldDecreaseQuantity()
    {
        var coin = new PortfolioCoin();
        coin.Trades.Add(new Trade
        {
            Date = DateTime.Now,
            Direction = TradeDirection.Buy,
            Price = 100,
            Quantity = 1,
            Symbol = "Test"
        });
        coin.Trades.Add(new Trade
        {
            Date = DateTime.Now,
            Direction = TradeDirection.Sell,
            Price = 100,
            Quantity = 0.5f,
            Symbol = "Test"
        });
        Assert.Equal(0.5, coin.Quantity);
    }
}