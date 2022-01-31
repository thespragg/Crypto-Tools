using CoinGecko.Clients;
using CoinGecko.Entities.Response.Coins;
using DCA_profitability.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DCA_profitability.Helpers
{
    internal class DCAProfits
    {
        private static DateTime Start = DateTime.Parse("1 Jan 2018");
        public static int DCAAmount = 150;

        private static List<CoinMarkets> MarketData = new List<CoinMarkets>();

        internal async Task Run()
        {
            var _client = CoinGeckoClient.Instance;
            var symbols = await _client.CoinsClient.GetCoinList();
            MarketData = (await _client.CoinsClient.GetCoinMarkets("usd")).Take(50).ToList(); // Get top 100 coins

            var coins = new List<CoinProfit>();
            var totPurchases = 0;

            for (var i = 0; i < MarketData.Count; i++) // Loop through the top 100
            {
                Console.Write($"\rProgress: {(i / 50f) * 100}%");
                var toAction = new List<TimestampedPrice>();
                var purchases = new List<CoinPurchase>();
                try
                {
                    var priceData = (await _client.CoinsClient.GetMarketChartsByCoinId(MarketData[i].Id, "usd", (DateTime.Today - Start).TotalDays.ToString())).Prices.Select(x => new TimestampedPrice((decimal)x[0], (decimal)x[1])).ToList();
                    var endingPrice = priceData[priceData.Count - 1].Price;

                    priceData.ForEachWithIndex((val, index) =>
                    {
                        if (index % 7 == 0) toAction.Add(val);
                    });

                    toAction.ForEach(x =>
                    {
                        purchases.Add(new CoinPurchase(x.Price, endingPrice));
                    });
                    totPurchases = purchases.Count;

                    coins.Add(new CoinProfit(MarketData[i].Id, purchases.Sum(x => x.Profit)));
                    Thread.Sleep(1250);
                }
                catch
                {
                    continue;
                }
            }

            Console.WriteLine();
            foreach (var coin in coins.Where(x => x.Id != "shiba-inu").OrderByDescending(x => x.Profit))
            {
                coin.PLPercentage = (int)Math.Round((coin.Profit / (DCAAmount * totPurchases)) * 100);
            }

            var newItems = new List<string>();
            newItems.Add("Symbol, Profit, P/L %");
            coins.OrderByDescending(x => x.PLPercentage).ToList().ForEach(x => newItems.Add($"{x.Id}, ${x.Profit}, {x.PLPercentage}"));
            coins = coins.OrderByDescending(x => x.PLPercentage).ToList();

            Console.WriteLine($"Spent: ${DCAAmount * totPurchases}");
            newItems.ForEach(x => Console.WriteLine(x));
        }
    }

    class CoinProfit
    {
        public string Id { get; set; }
        public decimal Profit { get; set; }
        public int PLPercentage { get; set; }

        public CoinProfit(string id, decimal profit) => (Id, Profit) = (id, decimal.Round(profit));
    }

    static class StaticHelpers
    {
        public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
        {
            int idx = 0;
            foreach (T item in enumerable)
                handler(item, idx++);
        }
    }

    class CoinPurchase
    {
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Profit { get; set; }

        public CoinPurchase(decimal coinPrice, decimal currentPrice)
        {
            Amount = DCAProfits.DCAAmount / coinPrice;
            Price = coinPrice;
            Profit = (currentPrice * Amount) - DCAProfits.DCAAmount;
        }
    }

}

