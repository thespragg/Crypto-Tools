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
        private int RateLimit { get => (60 / TotalCoins + 2) * 1000; }
        private List<float> PortfolioValue { get; set; } = new List<float>();

        internal async Task Run(string historicDataPath, int dcaAmnt, int numCoins = 100)
        {
            var table = new Table().LeftAligned();
            table.AddColumn(new TableColumn(new Markup("[bold yellow]Running algorithmic ETF[/]")).LeftAligned());
            await AnsiConsole.Live(table)
                .StartAsync(async ctx =>
                        {
                            table.AddRow(new Markup("[bold blue]Processing input data, please wait...[/]").LeftAligned());
                            ctx.Refresh();

                            var historicData = await ReadAndFindCoinIds(historicDataPath);
                            table.RemoveRow(0);
                            table.AddRow(new Markup($"[bold #c6c6c6]Running with the parameters: [/] \n [bold white] - {historicData.Count} months [/] \n [bold white] - ${dcaAmnt} per month [/] \n [bold white] - Top {numCoins} coins [/]").LeftAligned());
                            ctx.Refresh();
                        });

            TotalCoins = numCoins;
            var historicData = await ReadAndFindCoinIds(historicDataPath);
            var x = historicData.SelectMany(x => x.Coins.Select(x=>x.Name)).Distinct().ToList().Count;
            var carriedFunds = 0f;

            AnsiConsole.WriteLine("");

            var secondTable = new Table().LeftAligned();
            secondTable.AddColumn(new TableColumn("Reallocated").LeftAligned());
            secondTable.AddColumn(new TableColumn("Month").LeftAligned());
            secondTable.AddColumn(new TableColumn("Progress").LeftAligned());
            secondTable.AddColumn(new TableColumn("Ending Balance").LeftAligned());
            secondTable.AddColumn(new TableColumn("P/L").LeftAligned());

            await AnsiConsole.Live(secondTable).StartAsync(async ctx =>
                {
                    var monthCount = 0;
                    foreach (var month in historicData)
                    {
                        monthCount += 1;
                        secondTable.AddRow(new Markup("Selling dropped..."), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup("--%"), new Markup("$--"), new Markup("$--"));
                        ctx.Refresh();

                        var extraFunds = await SellDropped(month.Coins.Select(x => x.Symbol), month.Date); // Sell any coins that have left the selected total num coins + 10% and reallocate

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
                                if (string.IsNullOrEmpty(coin.Symbol))
                                {
                                    if(i < numCoins) carryingFunds += totDcaAmnt;
                                    continue;
                                }

                                var coinData = await _client.CoinsClient.GetHistoryByCoinId(coin.Symbol, month.Date.ToString("dd-MM-yyyy"), "false");
                                if (coinData.MarketData != null)
                                {
                                    if (!_purchases.ContainsKey(coin.Symbol)) _purchases.Add(coin.Symbol, new List<float>());
                                    var price = (float)coinData.MarketData.CurrentPrice["usd"].Value;
                                    if (i < numCoins) _purchases[coin.Symbol].Add(totDcaAmnt / price);
                                    PortfolioValue[^1] += price * _purchases[coin.Symbol].Sum();
                                }
                                else
                                {
                                    if (i < numCoins) carryingFunds += totDcaAmnt; // If a coin had no price, add it to next months funding
                                }

                                secondTable.RemoveRow(secondTable.Rows.Count - 1);
                                var progress = Math.Round(((double)i / month.Coins.Count)*100);
                                secondTable.AddRow(new Markup($"${carriedFunds + extraFunds}"), new Markup($"{month.Date:dd/MM/yyyy}"), new Markup($"{progress}%"), new Markup("$--"), new Markup("$--"));
                                ctx.Refresh();
                                Thread.Sleep(RateLimit);
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        secondTable.RemoveRow(secondTable.Rows.Count - 1);
                        var portfolioBalance = PortfolioValue[^1] + carryingFunds;
                        carriedFunds += carryingFunds;
                        secondTable.AddRow(new Markup($"{month.Date:dd/MM/yyyy}"), new Markup($"100%"), new Markup($"${portfolioBalance}"), new Markup($"${portfolioBalance}"), new Markup($"{GetProfitOrLossString(portfolioBalance - (monthCount * dcaAmnt))}"));
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

        private async Task<List<TopCoins>> ReadAndFindCoinIds(string path)
        {
            var historicData = JsonSerializer.Deserialize<List<TopCoins>>(File.ReadAllText(path));
            var symbols = (await _client.CoinsClient.GetCoinList()).Where(x => !x.Name.ToLower().Contains("binance-peg") && !x.Name.ToLower().Contains("buff doge") && !x.Name.ToLower().Contains("next token") && !x.Name.ToLower().Contains("stonk league")).ToList();
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
                }
            }
            return historicData;
        }

        private async Task<float> SellDropped(IEnumerable<String> monthCoins, DateTime month)
        {
            var missingCoins = _purchases.Keys.Where(x => !monthCoins.Contains(x)).ToList();
            var total = 0f;
            foreach (var coin in missingCoins)
            {
                var coinData = await _client.CoinsClient.GetHistoryByCoinId(coin, month.ToString("dd-MM-yyyy"), "false");
                var price = (float)coinData.MarketData.CurrentPrice["usd"].Value;
                total += _purchases[coin].Sum() * price;
                _purchases.Remove(coin);
                Thread.Sleep(RateLimit);
            }
            return total;
        }

        private void GetPriceData(List<string> coins)
        {

        }

        private void GetPriceOnDate(string coin)
        {

        }
    }
}
