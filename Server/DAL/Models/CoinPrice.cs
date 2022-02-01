using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crypto_Tools.DAL.Models;

public class CoinPrice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("lastChecked")]
    public DateTime LastChecked { get; set; }

    [BsonElement("Prices")]
    public List<TimestampedPrice> Prices { get; set; } = new List<TimestampedPrice>();
}

public class TimestampedPrice
{
    public DateTime Date { get; set; }
    public float Price { get; set; }
}

