using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Globalization;
using System.Reflection;

namespace Crypto_Tools.Helpers;
public class CMCMarketCapGatherer
{
    private readonly string _startDate = "20130428";
    private readonly IMarketCapService _mcapService;
    public CMCMarketCapGatherer(IMarketCapService mcapService) => _mcapService = mcapService;
    internal async Task GetHistoricTopCoins(int qty = 500)
    {
        try
        {
            var date = DateTime.ParseExact(_startDate, "yyyyMMdd", CultureInfo.InvariantCulture);

            while (date < DateTime.Today)
            {
                var top = new TopMarketCap
                {
                    Date = date
                };

                try
                {
                    if (await _mcapService.FindByDate(date) != null)
                    {
                        date = date.AddDays(7);
                        continue; // Date already exists
                    }

                    var src = GetPageSource($"https://coinmarketcap.com/historical/{date:yyyyMMdd}");
                    if (src == null)
                    {
                        date = date.AddDays(7);
                        continue;
                    }
                    date = date.AddDays(7);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(src);

                    HtmlNode? table = null;
                    table = doc.DocumentNode.SelectNodes("//table").Last();

                    var rows = table.SelectNodes("//tbody//tr").Take(qty);
                    var res = new List<string>();
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

                        var id = await CoinGeckoStaticHelpers.ConvertToCoinGeckoId(name, symbol);
                        if (id == null) continue;
                        res.Add(id);
                    }
                    top.Coins = res.Where(x => !string.IsNullOrEmpty(x)).ToList();
                    await _mcapService.Create(top);
                }
                catch
                {

                }
            }
        }
        catch
        {

        }
    }

    private static string? GetPageSource(string url)
    {
        ChromeOptions options = new();
        var chromeDriverService = ChromeDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        chromeDriverService.HideCommandPromptWindow = true;
        chromeDriverService.SuppressInitialDiagnosticInformation = true;
        options.AddArgument("--silent");
        options.AddArgument("log-level=3");
        try
        {
            var driver = new ChromeDriver(chromeDriverService, options)
            {
                Url = url
            };
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            IJavaScriptExecutor js = driver;
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");

            //Click the load more button twice to get top 500
            try
            {
                var loadMore = driver.FindElement(By.XPath("//div[@class='cmc-table-listing__loadmore']"));
                var btn = loadMore.FindElement(By.CssSelector("button:nth-child(1)"));
                btn.Click();
                Thread.Sleep(500);
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                Thread.Sleep(1000);
                loadMore = driver.FindElement(By.XPath("//div[@class='cmc-table-listing__loadmore']"));
                btn = loadMore.FindElement(By.CssSelector("button:nth-child(1)"));
                btn.Click();
                Thread.Sleep(500);
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight)");
                Thread.Sleep(1000);
            }
            catch
            {
            }

            var src = driver.PageSource;
            driver.Quit();
            return src;
        }
        catch
        {
            return null;
        }
    }
}
