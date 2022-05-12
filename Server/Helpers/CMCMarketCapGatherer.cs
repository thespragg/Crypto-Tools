using Crypto_Tools.DAL;
using Crypto_Tools.DAL.Models;
using Crypto_Tools.Services;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Crypto_Tools.Helpers;
public class CMCMarketCapGatherer
{
    private readonly string _startDate = "20130428";
    private readonly IMarketCapService _mcapService;
    private readonly ICoinPriceService _priceService;
    private readonly ILogger<MarketCapCollectionService> _logger;
    private HttpClient _httpClient;

    private readonly List<string> Proxies = new List<string>
    {
        "45.140.13.119:9132",
        "45.142.28.83:8094",
        "45.136.231.85:7141",
        "45.136.231.43:7099",
        "45.142.28.20:8031",
        "45.140.13.112:9125",
        "45.142.28.187:8198",
        "45.140.13.124:9137",
        "45.142.28.145:8156",
        "45.137.60.112:6640"
    };

    private readonly List<string> UsedProxies = new List<string>();

    public CMCMarketCapGatherer(IMarketCapService mcapService, ICoinPriceService priceService, ILogger<MarketCapCollectionService> logger)
    {
        (_mcapService, _priceService, _logger) = (mcapService, priceService, logger);
        GenerateNewClient();
    }

    private void GenerateNewClient()
    {
        if (Proxies.Count == 0)
        {
            Proxies.AddRange(UsedProxies);
            UsedProxies.Clear();
        }
        var proxy = Proxies.First();
        Proxies.Remove(proxy);
        UsedProxies.Add(proxy);

        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseProxy = true,
            Proxy = new WebProxy(proxy),
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential("owckvfpv", "qckoq5ozdjti"),
            DefaultProxyCredentials = new NetworkCredential("owckvfpv", "qckoq5ozdjti")
        };

        _httpClient = new(handler)
        {
            BaseAddress = new Uri("https://web-api.coinmarketcap.com/v1/cryptocurrency/listings/")
        };
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
        _httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
        _httpClient.DefaultRequestHeaders.Add("accept-language", "en-GB,en-US;q=0.9,en;q=0.8");
        _httpClient.DefaultRequestHeaders.Add("origin", "https://coinmarketcap.com");
        _httpClient.DefaultRequestHeaders.Add("referer", "https://coinmarketcap.com/");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "' Not A;Brand'; v = '99', 'Chromium'; v = '98', 'Google Chrome'; v = '98'");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "'Windows'");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
        _httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.81 Safari/537.36");
    }

    internal void GetHistoricTopCoins(int qty = 500)
    {
        new Thread(async () =>
        {
            var date = DateTime.ParseExact(_startDate, "yyyyMMdd", CultureInfo.InvariantCulture);

        retry:
            while (date < DateTime.Today)
            {
                try
                {
                    var top = new TopMarketCap
                    {
                        Date = date
                    };
                    var existing = await _mcapService.FindByDate(date);
                    if (existing != null)
                    {
                        date = date.AddDays(1);
                        continue;
                    }

                    var test = await _httpClient.GetAsync($"historical?convert=USD&date={date:yyyy-MM-dd}&limit=500&start=1");
                    var buffer = await test.Content.ReadAsByteArrayAsync();
                    var byteArray = buffer.ToArray();
                    var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    var res = JsonSerializer.Deserialize<CMCMarketCapData>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (res?.Data == null)
                    {
                        GenerateNewClient();
                        goto retry;
                    }
                    top.Coins = res!.Data!.Where(x => string.IsNullOrEmpty(x.Name)).Select(x => x.Name!).ToList();
                    await _mcapService.Create(top);

                    foreach (var coin in res.Data!)
                    {
                        var storedCoin = await _priceService.Find(coin.Name!);
                        if (storedCoin == null)
                        {
                            storedCoin = await _priceService.Create(new CoinPrice
                            {
                                Name = coin.Name!,
                                Symbol = coin.Symbol!,
                                Prices = new List<TimestampedPrice>()
                            });
                        }
                        coin.Date = date;
                        storedCoin!.Prices.Add(coin);
                        await _priceService.Update(storedCoin);
                    }

                    Thread.Sleep(2000);
                    date = date.AddDays(1);
                }
                catch
                {

                }
            }

        }).Start();
    }

    class CMCMarketCapData
    {
        public List<TimestampedPrice>? Data { get; set; }
    }
}
