using CryptoTools.Core.DAL;
using CryptoTools.Core.Interfaces;

namespace CryptoTools.Core.Models;

public class TradeBasedPortfolio : IPortfolio
{
    private readonly CryptoToolsDbContext _db;
    public TradeBasedPortfolio(CryptoToolsDbContext db) => (_db, CashBalance) = (db, StartingBalance);
    public int TotalTrades { get => Trades.Count; }
    public Dictionary<string, List<Trade>> Trades { get; set; } = new Dictionary<string, List<Trade>>();
    public List<Trade> TradeList { get => Trades.SelectMany(x => x.Value).ToList(); }
    public decimal StartingBalance { get; private set; } = 500;
    public Dictionary<string, PortfolioCoin> Coins { get; private set; } = new Dictionary<string, PortfolioCoin>();
    public Dictionary<string, PortfolioCoin> CoinList { get => Coins; }
    public decimal CashBalance { get; private set; }
    public bool Buy(string symbol, decimal price, decimal spent, DateTime day)
    {
        if (CashBalance < spent) return false;
        CashBalance -= spent;
        var trade = new Trade
        {
            Symbol = symbol,
            Quantity = (float)(spent / price),
            Price = price,
            Date = day,
            Direction = TradeDirection.BUY
        };
        if (!Coins.ContainsKey(symbol)) Coins.Add(symbol, new PortfolioCoin(symbol));
        if (Trades.ContainsKey(symbol)) Trades[symbol].Add(trade);
        else Trades.Add(symbol, new List<Trade> { trade });
        return true;
    }

    public void Sell(string symbol, decimal price, DateTime day, float? quantity = null)
    {
        if (quantity == null)
        {
            var bought = Trades[symbol].Where(x => x.Direction == TradeDirection.BUY).Sum(x => x.Quantity);
            var sold = Trades[symbol].Where(x => x.Direction == TradeDirection.SELL).Sum(x => x.Quantity);
            quantity = bought - sold;
        }
        if (quantity.Value == 0) return;
        CashBalance += (decimal)quantity * price;
        Trades[symbol].Add(new Trade
        {
            Symbol = symbol,
            Quantity = quantity.Value,
            Price = price,
            Date = day,
            Direction = TradeDirection.SELL
        });
    }

    public PortfolioCoin? GetCoin(string symbol)
    {
        if (!Coins.ContainsKey(symbol)) return null;
        return Coins[symbol];
    }

    private List<PortfolioSnapshot> Snapshots { get; set; } = new List<PortfolioSnapshot>();
    public List<PortfolioSnapshot> GetSnapshots() => Snapshots;

    public void TakeSnapshot(DateTime date)
    {
        throw new NotImplementedException();
    }

    public void TakeSnapshot(DateTime date, Dictionary<string, decimal> prices)
    {
        var snapshot = new PortfolioSnapshot()
        {
            Date = date,
        };
        var sales = new List<decimal>();
        foreach (var (coin, trades) in Trades)
        {
            var spent = 0M;
            var sold = 0M;

            var soldQty = 0f;
            var boughtQty = 0f;

            var currentSpend = 0m;
            foreach (var trade in trades)
            {
                if (trade.Direction == TradeDirection.BUY)
                {
                    spent += trade.Value;
                    boughtQty += trade.Quantity;

                    currentSpend += trade.Value;
                }
                else
                {
                    sold += trade.Value;
                    soldQty += trade.Quantity;

                    if (trade.Value > currentSpend) snapshot.ProfitableSales += 1;
                    sales.Add(trade.Value - currentSpend);
                    currentSpend = 0;
                    snapshot.TotalSales += 1;
                }
            }
            var remainder = boughtQty - soldQty;
            var hasPrice = prices.TryGetValue(coin, out var price);

            snapshot.Spent += spent;
            snapshot.CashValue += sold;

            if (!hasPrice || remainder == 0) continue;
            snapshot.CoinValue += (decimal)remainder * price;
        }
        if (sales.Count > 0)
        {
            snapshot.AverageProfit = sales.Where(x => x >= 0).Sum() / (decimal)sales.Count;
            snapshot.AverageLoss = sales.Where(x => x < 0).Select(x=>Math.Abs(x)).Sum() / (decimal)sales.Count;
        }
        Snapshots.Add(snapshot);
    }
}
