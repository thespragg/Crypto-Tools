using CryptoTools.Core.Models;

namespace CryptoTools.Tests.Models;

public class TradeTests
{
    [Fact]
    public void AddingTradeQuantityAndPrice_ShouldUpdateValue()
    {
        var trade = new Trade
        {
            Quantity = 0.5f,
            Price = 100
        };
        Assert.Equal(50, trade.Value);
    }
}