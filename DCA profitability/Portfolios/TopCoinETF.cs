using CoinGecko.Clients;
using DCA_profitability.Helpers;
using DCA_profitability.Models;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DCA_profitability
{
    internal class TopCoinETF
    {
        private readonly Dictionary<string, List<float>> _purchases = new();
        private readonly CoinGeckoClient _client = CoinGeckoClient.Instance;
        private int TotalCoins { get; set; }
        private CoinPriceWrapper PriceWrapper { get; set; }
        private List<float> PortfolioValue { get; set; } = new List<float>();

        internal async Task Run(string historicDataPath, int dcaAmnt, int numCoins = 100)
        {
            TotalCoins = numCoins;
            List<TopCoins> historicData = null;
            var carriedFunds = 0f;

            var table = new Table().LeftAligned();
            table.AddColumn(new TableColumn(new Markup("[bold yellow]Running algorithmic ETF[/]")).LeftAligned());
            await AnsiConsole.Live(table)
                .StartAsync(async ctx =>
                        {
                            table.AddRow(new Markup("[bold blue]Processing input data, please wait... 0%[/]").LeftAligned());
                            ctx.Refresh();

                            historicData = await ReadAndFindCoinIds(historicDataPath, table, ctx);
                            table.RemoveRow(0);
                            table.AddRow(new Markup($"[bold #c6c6c6]Running with the parameters: [/] \n [bold white] - {historicData.Count} months [/] \n [bold white] - ${dcaAmnt} per month [/] \n [bold white] - Top {numCoins} coins [/]").LeftAligned());
                            ctx.Refresh();

                            table.AddRow(new Markup("[bold blue]Processing price data, please wait... 0%[/]").LeftAligned());
                            ctx.Refresh();

                            PriceWrapper = await CoinPriceWrapper.LoadPrices(historicData.SelectMany(x => x.Coins.Select(x => x.Symbol)).Distinct().ToList(), ctx, table);
                        });

            AnsiConsole.WriteLine("");

            var secondTable = new Table().LeftAligned();
            secondTable.AddColumn(new TableColumn("Reallocated").LeftAligned());
            secondTable.AddColumn(new TableColumn("Month").LeftAligned());
            secondTable.AddColumn(new TableColumn("Progress").LeftAligned());
            secondTable.AddColumn(new TableColumn("Ending Balance").LeftAligned());
            secondTable.AddColumn(new TableColumn("P/L").LeftAligned());

            AnsiConsole.Live(secondTable).Start(ctx =>
                {
                    var monthCount = 0;
                    foreach (var month in historicData)
                    {
                        monthCount += 1;
                        secondTable.AddRow(new Markup("Selling dropped..."), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup("--%"), new Markup("$--"), new Markup("$--"));
                        ctx.Refresh();

                        var extraFunds = SellDropped(month.Coins.Select(x => x.Symbol).Take((int)(numCoins + (Math.Round(numCoins * 0.1f)))), month.Date); // Sell any coins that have left the selected total num coins + 10% and reallocate

                        secondTable.RemoveRow(secondTable.Rows.Count - 1);
                        secondTable.AddRow(new Markup($"${carriedFunds + extraFunds}"), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup($"--%"), new Markup("$--"), new Markup("$--"));
                        ctx.Refresh();

                        var totDcaAmnt = (extraFunds + dcaAmnt + carriedFunds) / numCoins;
                        carriedFunds = 0f;
                        var carryingFunds = 0f;
                        PortfolioValue.Add(0f);

                        for (var i = 0; i < month.Coins.Count; i++)
                        {
                            try
                            {
                                var coin = month.Coins[i];
                                var coinPrice = PriceWrapper.GetPriceOnDate(coin.Symbol, month.Date);
                                if (coinPrice == null)
                                {
                                    if (i < numCoins) carryingFunds += totDcaAmnt;
                                    continue;
                                }

                                if (!_purchases.ContainsKey(coin.Symbol)) _purchases.Add(coin.Symbol, new List<float>());
                                if (i < numCoins) _purchases[coin.Symbol].Add(totDcaAmnt / (float)coinPrice);
                                PortfolioValue[^1] += (float)coinPrice * _purchases[coin.Symbol].Sum();

                                secondTable.RemoveRow(secondTable.Rows.Count - 1);
                                var progress = Math.Round(((double)i / month.Coins.Count) * 100);
                                secondTable.AddRow(new Markup($"${carriedFunds + extraFunds}"), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup($"{progress}%"), new Markup("$--"), new Markup("$--"));
                                ctx.Refresh();
                            }
                            catch
                            {

                            }
                        }

                        secondTable.RemoveRow(secondTable.Rows.Count - 1);
                        var portfolioBalance = PortfolioValue[^1] + carryingFunds;
                        carriedFunds += carryingFunds;
                        secondTable.AddRow(new Markup($"${carriedFunds + extraFunds}"), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup($"100%"), new Markup($"${portfolioBalance}"), new Markup($"{GetProfitOrLossString(portfolioBalance - (monthCount * dcaAmnt))}"));
                        ctx.Refresh();
                    }
                });

            var endingSpend = dcaAmnt * historicData.Count;
            var endingProfit = PortfolioValue[^1];
            Console.WriteLine($"Ending P/L: ${endingProfit - endingSpend}");
        }

        private string GetProfitOrLossString(float value)
        {
            if (value < 0) return $"[bold red]${value}[/]";
            if (value > 0) return $"[bold green]${value}[/]";
            else return $"[bold white]${value}[/]";
        }

        private async Task<List<TopCoins>> ReadAndFindCoinIds(string path, Table table, LiveDisplayContext ctx)
        {
            var historicData = JsonSerializer.Deserialize<List<TopCoins>>(File.ReadAllText(path));
            var symbols = (await _client.CoinsClient.GetCoinList()).Where(x => !x.Name.ToLower().Contains("binance-peg") && !x.Name.ToLower().Contains("buff doge") && !x.Name.ToLower().Contains("next token") && !x.Name.ToLower().Contains("stonk league")).ToList();
            var total = historicData.SelectMany(x => x.Coins).Count();
            var currCount = 0;

            foreach (var month in historicData)
            {
                for (var i = 0; i < month.Coins.Count; i++)
                {
                    var coin = month.Coins[i];
                    if (coin.Symbol == "VEN") month.Coins[i].Symbol = "VET";
                    var id = symbols.FirstOrDefault(x => x.Name.ToLower() == coin.Name.ToLower() || x.Id.ToLower() == coin.Symbol.ToLower() || x.Name.ToLower() == coin.Symbol.ToLower() || x.Symbol.ToLower() == coin.Name.ToLower());
                    if (id != null) month.Coins[i].Symbol = id.Id;
                    else
                    {
                        id = symbols.FirstOrDefault(x => x.Symbol.ToLower() == coin.Symbol.ToLower() || x.Id.ToLower() == coin.Symbol.ToLower());
                        var chk = symbols.Where(x => x.Symbol.ToLower() == coin.Symbol.ToLower() || x.Id.ToLower() == coin.Symbol.ToLower()).ToList();
                        if (id != null) month.Coins[i].Symbol = id.Id;
                        else
                        {
                            month.Coins[i].Symbol = string.Empty;
                        }
                    }
                    currCount += 1;
                }

                var percent = Math.Round(((float)currCount / total) * 100);
                table.RemoveRow(table.Rows.Count - 1);
                table.AddRow($"[bold blue]Processing input data, please wait... {percent}%[/]");
                ctx.Refresh();
            }

            return historicData;
        }

        private float SellDropped(IEnumerable<String> monthCoins, DateTime month)
        {
            var missingCoins = _purchases.Keys.Where(x => !monthCoins.Contains(x)).ToList();
            var total = 0f;
            foreach (var coin in missingCoins)
            {
                var coinPrice = PriceWrapper.GetPriceOnDate(coin, month.Date);
                if (coinPrice == null) coinPrice = PriceWrapper.GetNearestDatePrice(coin, month.Date);
                if (coinPrice != null) total += _purchases[coin].Sum() * (float)coinPrice;
                _purchases.Remove(coin);
            }
            return total;
        }
    }
}
