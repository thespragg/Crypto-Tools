using Crypto_Tools.DAL.Models;
using MongoDB.Driver;

namespace Crypto_Tools.DAL;

public interface ICoinPriceService
{
    Task<CoinPrice> Create(CoinPrice coin);
    Task<List<CoinPrice>> Get();
    Task<CoinPrice> Get(string id);
    Task<CoinPrice> Find(string symbol);
    Task<Dictionary<string, Dictionary<DateTime, TimestampedPrice>>> GetPriceDictionary(IEnumerable<string> coins, DateTime start, DateTime end);
    Task<DeleteResult> Remove(string id);
    Task<ReplaceOneResult> Update(CoinPrice coinIn);
}

public class CoinPriceService : ICoinPriceService
{
    private readonly IMongoCollection<CoinPrice> _coinPrices;
    public CoinPriceService(ICryptoToolsDatabaseSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _coinPrices = database.GetCollection<CoinPrice>(settings.CoinPricesCollectionName);
    }

    public async Task<List<CoinPrice>> Get() =>
        (await _coinPrices.FindAsync(coin => true)).ToList();

    public async Task<CoinPrice> Get(string id) =>
        (await _coinPrices.FindAsync(coin => coin.Id == id)).FirstOrDefault();

    public async Task<CoinPrice> Find(string symbol) =>
        (await _coinPrices.FindAsync(coin => coin.Name == symbol)).FirstOrDefault();

    public async Task<Dictionary<string, Dictionary<DateTime, TimestampedPrice>>> GetPriceDictionary(IEnumerable<string> coins, DateTime start, DateTime end)
    {
        var prices = new Dictionary<string, Dictionary<DateTime, TimestampedPrice>>();
        foreach (var coin in coins)
        {
            if(prices.ContainsKey(coin)) continue;
            var storedCoin = await Find(coin);
            if (storedCoin == null) continue;
            var storedPrices = storedCoin!.Prices.Where(x => x.Date >= start && x.Date <= end).ToList();
            if (storedPrices.Count == 0) continue;
            prices.Add(coin, storedPrices.ToDictionary(x=>x.Date,x=>x));
        }
        return prices;
    }

    public async Task<CoinPrice> Create(CoinPrice coin)
    {
        await _coinPrices.InsertOneAsync(coin);
        return coin;
    }

    public async Task<ReplaceOneResult> Update(CoinPrice coinIn) =>
        (await _coinPrices.ReplaceOneAsync(coin => coin.Id == coinIn.Id, coinIn));

    public async Task<DeleteResult> Remove(string id) =>
        await _coinPrices.DeleteOneAsync(coin => coin.Id == id);
}
