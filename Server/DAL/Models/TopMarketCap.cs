using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Crypto_Tools.DAL.Models;

public class TopMarketCap
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("coins")]
    public List<string> Coins { get; set; } = new List<string>();
}

