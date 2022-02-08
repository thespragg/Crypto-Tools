using Crypto_Tools.DAL.Models;
using MongoDB.Driver;

namespace Crypto_Tools.DAL;

public interface IMarketCapService
{
    Task<TopMarketCap> Create(TopMarketCap mcap);
    Task<List<TopMarketCap>> Get();
    Task<TopMarketCap> Get(string id);
    Task<TopMarketCap> FindByDate(DateTime date);
    Task<List<TopMarketCap>> GetBetweenDates(DateTime start, DateTime end);
    Task<List<TopMarketCap>> GetWithCappedCoinsBetweenDates(DateTime start, DateTime end, int numCoins);
    Task<DeleteResult> Remove(string id);
    Task<ReplaceOneResult> Update(TopMarketCap mcapIn);
}

public class MarketCapService : IMarketCapService
{
    private readonly IMongoCollection<TopMarketCap> _topMarketCaps;
    public MarketCapService(ICryptoToolsDatabaseSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _topMarketCaps = database.GetCollection<TopMarketCap>(settings.MarketCapCollectionName);
    }

    public async Task<List<TopMarketCap>> Get() =>
        (await _topMarketCaps.FindAsync(mcap => true)).ToList();

    public async Task<TopMarketCap> Get(string id) =>
        (await _topMarketCaps.FindAsync(mcap => mcap.Id == id)).FirstOrDefault();

    public async Task<List<TopMarketCap>> GetBetweenDates(DateTime start, DateTime end) =>
       (await _topMarketCaps.FindAsync(mcap => mcap.Date >= start && mcap.Date <= end)).ToList();

    public async Task<List<TopMarketCap>> GetWithCappedCoinsBetweenDates(DateTime start, DateTime end, int numCoins) => (await GetBetweenDates(start,end)).Select(x=>new TopMarketCap(x,numCoins)).ToList();

    public async Task<TopMarketCap> FindByDate(DateTime date) =>
       (await _topMarketCaps.FindAsync(mcap => mcap.Date == date)).FirstOrDefault();

    public async Task<TopMarketCap> Create(TopMarketCap mcap)
    {
        await _topMarketCaps.InsertOneAsync(mcap);
        return mcap;
    }

    public async Task<ReplaceOneResult> Update(TopMarketCap mcapIn) =>
        (await _topMarketCaps.ReplaceOneAsync(mcap => mcap.Id == mcapIn.Id, mcapIn));

    public async Task<DeleteResult> Remove(string id) =>
        await _topMarketCaps.DeleteOneAsync(mcap => mcap.Id == id);
}

