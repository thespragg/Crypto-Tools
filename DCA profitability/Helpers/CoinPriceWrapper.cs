using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CoinGecko.Clients;
using Spectre.Console;

namespace DCA_profitability.Helpers
{
    public class CoinPriceWrapper
    {
        private Dictionary<string, Dictionary<string, float>> Prices { get; set; }
        private CoinPriceWrapper() { }
        public static async Task<CoinPriceWrapper> LoadPrices(List<string> coins, LiveDisplayContext ctx, Table table, string startDateStr = "01/01/2018")
        {
            var _client = CoinGeckoClient.Instance;
            var startDate = DateTime.Parse(startDateStr);

            var wrapper = new CoinPriceWrapper()
            {
                Prices = new Dictionary<string, Dictionary<string, float>>()
            };

            wrapper.LoadLocalPrices(startDateStr);

            var coinCount = 0;
            var errs = new List<string>();

            foreach (var coin in coins)
            {
                if (wrapper.Prices.ContainsKey(coin))
                {
                    var earliestDate = wrapper.Prices[coin].Keys.Select(x => DateTime.ParseExact(x, "dd/MM/yyyy", CultureInfo.InvariantCulture)).OrderBy(x => x).First();
                    if (earliestDate <= startDate)
                    {
                        coinCount += 1;
                        if (table != null && ctx != null)
                        {
                            table.RemoveRow(table.Rows.Count - 1);
                            table.AddRow(new Markup($"[bold blue]Processing price data, please wait... {Math.Round(((float)coinCount / coins.Count) * 100)}%[/]"));
                            ctx.Refresh();
                        }
                        continue;
                    }
                } 

                try
                {
                    var priceData = (await _client.CoinsClient.GetMarketChartsByCoinId(coin, "usd", "max"));
                    var priceDict = priceData.Prices.GroupBy(p => UnixTimeStampToDateTime((long)p[0]).ToString("dd/MM/yyyy"), StringComparer.OrdinalIgnoreCase).ToDictionary(z => z.Key, z => (float)z.First()[1]);
                    wrapper.Prices.Add(coin, priceDict);
                    var earliestDate = priceDict.Keys.OrderBy(x => DateTime.ParseExact(x, "dd/MM/yyyy", CultureInfo.InvariantCulture)).FirstOrDefault();
                    File.WriteAllText($"Prices/{coin};{startDate:yyyyMMdd}", JsonSerializer.Serialize(priceDict));

                    coinCount += 1;
                    if (table != null && ctx != null)
                    {
                        table.RemoveRow(table.Rows.Count - 1);
                        table.AddRow(new Markup($"[bold blue]Processing price data, please wait... {Math.Round(((float)coinCount / coins.Count) * 100)}%[/]"));
                        ctx.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    errs.Add(coin + ":" + ex.Message);
                }
                Thread.Sleep(1300);
            }


            if (table != null && ctx != null)
            {
                foreach (var err in errs)
                {
                    table.AddRow(new Markup($"[bold red]{err}[/]"));
                }
                ctx.Refresh();
            }

            foreach (var coin in wrapper.Prices)
            {
                foreach (var item in coin.Value.Where(kvp => kvp.Value.Equals(-1)).ToList()) coin.Value.Remove(item.Key);
            }

            return wrapper;
        }

        public static async Task<CoinPriceWrapper> LoadPrices(List<string> coins, string startDateStr = "01/01/2018") => await LoadPrices(coins, null, null, startDateStr);

        public float? GetNearestDatePrice(string coin, DateTime date)
        {
            if (!Prices.ContainsKey(coin)) return null;
            var closestDate = Prices[coin].Keys.OrderBy(t => Math.Abs((DateTime.ParseExact(t, "dd/MM/yyyy", CultureInfo.InvariantCulture) - date).Ticks)).FirstOrDefault();
            if (closestDate == null) return null;
            return Prices[coin][closestDate];
        }

        public float? GetPriceOnDate(string coin, DateTime date)
        {
            if (!Prices.ContainsKey(coin)) return null;
            var dict = Prices[coin];
            if (!Prices[coin].ContainsKey(date.ToString("dd/MM/yyyy"))) return null;
            return Prices[coin][date.ToString("dd/MM/yyyy")];
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp);
            DateTime dateTime = dateTimeOffSet.DateTime;
            return dateTime;
        }

        private void LoadLocalPrices(string startDateStr)
        {
            var files = Directory.GetFiles("Prices");

            foreach (var file in files)
            {
                var contents = File.ReadAllText(file);
                var coin = Path.GetFileName(file);
                var parts = coin.Split(";");
                var date = DateTime.ParseExact(parts[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                Prices.Add(parts[0], JsonSerializer.Deserialize<Dictionary<string, float>>(contents));
                if (!Prices[parts[0]].ContainsKey(date.ToString("dd/MM/yyyy"))) Prices[parts[0]].Add(date.ToString("dd/MM/yyyy"), -1);
            }
        }
    }
}