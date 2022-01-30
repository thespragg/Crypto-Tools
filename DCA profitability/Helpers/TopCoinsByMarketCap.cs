using DCA_profitability.Models;
using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace DCA_profitability.Helpers
{
    internal class TopCoinsByMarketCap
    {
        private readonly List<string> Dates = new List<string>
        {
            "20180107",
            "20180204",
            "20180304",
            "20180401",
            "20180506",
            "20180603",
            "20180701",
            "20180805",
            "20180902",
            "20181007",
            "20181104",
            "20181202",
            "20190106",
            "20190203",
            "20190303",
            "20190407",
            "20190505",
            "20190602",
            "20190707",
            "20190804",
            "20190901",
            "20191006",
            "20191103",
            "20191201",
            "20200105",
            "20200202",
            "20200301",
            "20200405",
            "20200503",
            "20200607",
            "20200705",
            "20200802",
            "20200906",
            "20201004",
            "20201101",
            "20201206",
            "20210103",
            "20210207",
            "20210307",
            "20210404",
            "20210502",
            "20210606",
            "20210704",
            "20210801",
            "20210905",
            "20211003",
            "20211107",
            "20211205",
            "20220102"
        };

        private static string GetPageSource(string url)
        {
            var driver = new ChromeDriver()
            {
                Url = url
            };
            var src = driver.PageSource;
            driver.Close();
            return src;
        }

        internal List<TopCoins> GetHistoricTopCoins(int qty)
        {
            try
            {
                var res = new List<TopCoins>();

                foreach (var date in Dates)
                {
                DoWork:
                    var dateParsed = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                    Console.WriteLine($"Processing: {dateParsed.ToString("dd-MM-yyyy")}");
                    var top = new TopCoins(dateParsed);
                    var creds = CredentialCache.DefaultNetworkCredentials;

                    var src = GetPageSource($"https://coinmarketcap.com/historical/{date}");
                    var doc = new HtmlDocument();
                    doc.LoadHtml(src);

                    HtmlNode table = null;
                    try { 
                        table = doc.DocumentNode.SelectNodes("//table").Last();
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                        goto DoWork;
                    }
                    var rows = table.SelectNodes("//tbody//tr").Take(qty);
                    foreach (var row in rows)
                    {
                        var name = "";
                        try
                        {
                            name = row.ChildNodes[1].FirstChild.LastChild.InnerText;
                        }
                        catch
                        {
                            name = row.ChildNodes[1].InnerText;
                        }

                        var symbol = "";
                        try
                        {
                            symbol = row.ChildNodes[2].FirstChild.InnerText;
                        }
                        catch { }

                        var coin = new Coin()
                        {
                            Name = name,
                            Symbol = symbol
                        };
                        top.Coins.Add(coin);
                    }
                    res.Add(top);
                    Thread.Sleep(1000);
                }
                return res;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
