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
    private readonly HttpClient _httpClient;
    public CMCMarketCapGatherer(IMarketCapService mcapService, ICoinPriceService priceService, ILogger<MarketCapCollectionService> logger)
    {
        (_mcapService, _priceService, _logger) = (mcapService, priceService, logger);
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
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

                    var test = await _httpClient.GetAsync($"historical?convert=USD,USD,BTC&date={date:yyyy-MM-dd}&limit=500&start=1");
                    var buffer = await test.Content.ReadAsByteArrayAsync();
                    var byteArray = buffer.ToArray();
                    var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    var res = JsonSerializer.Deserialize<CMCMarketCapData>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (res.Data == null) continue;
                    top.Coins = res!.Data!.Where(x=>string.IsNullOrEmpty(x.Name)).Select(x => x.Name!).ToList();
                    await _mcapService.Create(top);

                    foreach(var coin in res.Data!)
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
