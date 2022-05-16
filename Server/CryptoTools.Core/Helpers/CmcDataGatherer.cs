using CryptoTools.Core.DAL;
using CryptoTools.Core.DAL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    private CryptoToolsDbContext _db;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CmcDataGatherer> _logger;
    public CmcDataGatherer(IServiceProvider serviceProvider, ILogger<CmcDataGatherer> logger) => (_httpClient, _serviceProvider, _logger) = (GenerateNewClient(), serviceProvider, logger);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        _db = scope.ServiceProvider.GetService<CryptoToolsDbContext>()!;
        await Get();
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

    public async Task Get()
    {
        var date = DateTime.ParseExact(_startDate, "yyyyMMdd", CultureInfo.InvariantCulture);

    retry:
        while (date < DateTime.Today)
        {
            _logger.LogInformation("Processing date: {date}", date.ToString("dd/MM/yyyy"));
            var days = (DateTime.Today - date).TotalDays;

            try
            {
                var top = new MarketCapRanking
                {
                    Date = date
                };

                var existing = _db.MarketCapRankings.FirstOrDefault(x => x.Date == date);

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
                top.Coins = res!.Data!.Where(x => !string.IsNullOrEmpty(x.Name)).Select(x => x.Symbol!).ToList();
                if (existing != null)
                {
                    _db.MarketCapRankings.Add(top);
                    _db.SaveChanges();
                }

                var dict = res.Data!.GroupBy(x => x.Symbol!).ToDictionary(x => x.Key, x => x.OrderBy(x => x.CMC_Rank).First());
                var allPrices = _db.CoinPrices.Where(x => x.Date == date).ToList().GroupBy(x => x.CoinSymbol).ToDictionary(x => x.Key, x => x.OrderBy(x => x.MarketCapRank).First());

                if (allPrices.Count == res.Data!.Count)
                {
                    date = date.AddDays(1);
                    continue;
                }

                var coins = res.Data!.Where(x => dict.ContainsKey(x.Name!) && !allPrices.ContainsKey(x.Name)).ToList().Select(x => new CoinPrice
                {
                    Date = date,
                    CoinSymbol = x.Symbol!,
                    Price = (decimal)x.Price,
                    MarketCapRank = x.CMC_Rank ?? 0
                });
                if (!coins.Any())
                {
                    date = date.AddDays(1);
                    continue;
                }

                _db.CoinPrices.AddRange(coins);
                _db.SaveChanges();
                _db.ChangeTracker.Clear();

                Thread.Sleep(500);
                date = date.AddDays(1);
            }
            catch (Exception ex)
            {
                _httpClient = GenerateNewClient();
            }
        }
    }

    public class CMCMarketCapData
    {
        public List<TimestampedPrice>? Data { get; set; }
    }

    public class TimestampedPrice
    {
        public DateTime Date { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public int? CMC_Rank { get; set; }
        public Quote? Quote { get; set; }
        public float Price
        {
            get
            {
                if (Quote == null) return 0f;
                if (Quote.USD == null) return 0f;
                if (Quote.USD.Price == null) return 0f;
                return (float)Quote.USD.Price;
            }
        }
    }

    public class Quote
    {
        public PriceHolder USD { get; set; }
    }

    public class PriceHolder
    {
        public double? Price { get; set; }
    }
}
