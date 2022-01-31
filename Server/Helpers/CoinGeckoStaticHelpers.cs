using CoinGecko.Clients;
using CoinGecko.Entities.Response.Coins;
using Crypto_Tools.DAL.Models;

namespace Crypto_Tools.Helpers;

public class CoinGeckoStaticHelpers
{
    private static List<CoinList>? Symbols { get; set; } = null;
    private static readonly CoinGeckoClient _client = CoinGeckoClient.Instance;

    public static async Task<string?> ConvertToCoinGeckoId(string name, string symbol)
    {
        if(Symbols == null) Symbols = (await _client.CoinsClient.GetCoinList()).Where(x => !x.Name.ToLower().Contains("binance-peg") && !x.Name.ToLower().Contains("buff doge") && !x.Name.ToLower().Contains("next token") && !x.Name.ToLower().Contains("stonk league")).ToList();
        
        if (symbol == "VEN") symbol = "VET";
        var id = Symbols.FirstOrDefault(x => x.Name.ToLower() == name.ToLower() || x.Id.ToLower() == symbol.ToLower() || x.Name.ToLower() == symbol.ToLower() || x.Symbol.ToLower() == name.ToLower());
        if (id != null) symbol = id.Id;
        else
        {
            id = Symbols.FirstOrDefault(x => x.Symbol.ToLower() == symbol.ToLower() || x.Id.ToLower() == symbol.ToLower());
            var chk = Symbols.Where(x => x.Symbol.ToLower() == symbol.ToLower() || x.Id.ToLower() == symbol.ToLower()).ToList();
            if (id != null) symbol = id.Id;
            else symbol = string.Empty;
        }

        return symbol;
    }

    public static async Task<CoinPrice> GetPrice(string coin)
    {
        var priceData = (await _client.CoinsClient.GetMarketChartsByCoinId(coin, "usd", "max"));
        Thread.Sleep(1200);
        var prices = priceData.Prices.GroupBy(p => UnixTimeStampToDateTime((long)p[0]!)).Select(x=>new TimestampedPrice { Date = x.Key, Price = (float)x.First()[1]!});
        return new CoinPrice
        {
            Name =coin,
            Prices = prices.ToList(),
            LastChecked = DateTime.Now
        };
    }

    private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        DateTimeOffset dateTimeOffSet = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp);
        DateTime dateTime = dateTimeOffSet.DateTime;
        return dateTime;
    }
}

