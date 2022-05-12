using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CryptoTools.Core.Helpers;
public class CmcDataGatherer : IHostedService
{
    private readonly string _startDate = "20130428";
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

    public CmcDataGatherer() => _httpClient = GenerateNewClient();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Get();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private HttpClient GenerateNewClient()
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

        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://web-api.coinmarketcap.com/v1/cryptocurrency/listings/")
        };
        client.DefaultRequestHeaders.Add("accept", "application/json, text/plain, */*");
        client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.Add("accept-language", "en-GB,en-US;q=0.9,en;q=0.8");
        client.DefaultRequestHeaders.Add("origin", "https://coinmarketcap.com");
        client.DefaultRequestHeaders.Add("referer", "https://coinmarketcap.com/");
        client.DefaultRequestHeaders.Add("sec-ch-ua", "' Not A;Brand'; v = '99', 'Chromium'; v = '98', 'Google Chrome'; v = '98'");
        client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "'Windows'");
        client.DefaultRequestHeaders.Add("sec-fetch-dest", "empty");
        client.DefaultRequestHeaders.Add("sec-fetch-mode", "cors");
        client.DefaultRequestHeaders.Add("sec-fetch-site", "same-site");
        client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.81 Safari/537.36");
        return client;
    }

    public void Get()
    {
        var _db = new CryptoToolsDbContext();
        new Thread(async () =>
        {
            var date = DateTime.ParseExact(_startDate, "yyyyMMdd", CultureInfo.InvariantCulture);

        retry:
            while (date < DateTime.Today)
            {
                try
                {
                    var top = new MarketCapRanking
                    {
                        Date = date
                    };

                    var existing = _db.MarketCapRankings.FirstOrDefault(x => x.Date == date);
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
                        _httpClient = GenerateNewClient();
                        goto retry;
                    }
                    top.Coins = res!.Data!.Where(x => string.IsNullOrEmpty(x.Name)).Select(x => x.Name!).ToList();
                    _db.MarketCapRankings.Add(top);

                    foreach (var coin in res.Data!)
                    {
                        var storedCoin = _db.CoinPrices.FirstOrDefault(x => x.CoinSymbol == coin.Symbol && x.Date == date);
                        if (storedCoin == null)
                        {
                            storedCoin = _db.CoinPrices.Add(new CryptoTools.Core.DAL.Models.CoinPrice
                            {
                                Date = date,
                                CoinSymbol = coin.Symbol!,
                                Price = (decimal)coin.Price,
                                MarketCapRank = coin.CMCRank ?? 0
                            }).Entity;
                        }
                    }

                    _db.SaveChanges();
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

    class TimestampedPrice
    {
        public DateTime Date { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public int? CMCRank { get; set; }
        public Quote? PriceHolder { get; set; }
        public float Price
        {
            get
            {
                if (PriceHolder == null) return 0f;
                if (PriceHolder.Price == null) return 0f;
                if (PriceHolder.Price.Price == null) return 0f;
                return (float)PriceHolder.Price.Price;
            }
        }
    }

    class PriceHolder
    {
        public double? Price { get; set; }
    }

    class Quote
    {
        public PriceHolder? Price { get; set; }
    }
}
