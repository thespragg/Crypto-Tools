using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Crypto_Tools.DAL.Models;

public class CoinPrice
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [BsonElement("Prices")]
    public List<TimestampedPrice> Prices { get; set; } = new List<TimestampedPrice>();
}

public class PriceHolder
{
    public double? Price { get; set; }
    [JsonPropertyName("volume_24h")]
    public double? Volume24h { get; set; }
    [JsonPropertyName("percent_change_1h")]
    public double? PercentChange1h { get; set; }
    [JsonPropertyName("percent_change_24h")]
    public double? PercentChange24h { get; set; }
    [JsonPropertyName("percent_change_7d")]
    public double? PercentChange7d { get; set; }
    [JsonPropertyName("market_cap")]
    public double? MarketCap { get; set; }
}

public class Quote
{
    [JsonPropertyName("USD")]
    public PriceHolder? Price { get; set; }
}

public class TimestampedPrice
{
    public DateTime Date { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }

    [JsonPropertyName("num_market_pairs")]
    public int? NumMarketPairs { get; set; }
    [JsonPropertyName("date_added")]
    public DateTime? DateAdded { get; set; }
    public List<string?>? Tags { get; set; }
    [JsonPropertyName("max_supply")]
    public double? MaxSupply { get; set; }
    [JsonPropertyName("circulating_supply")]
    public double? CirculatingSupply { get; set; }
    [JsonPropertyName("total_supply")]
    public double? TotalSupply { get; set; }
    [JsonPropertyName("cmc_rank")]
    public int? CMCRank { get; set; }
    [JsonPropertyName("self_reported_circulating_supply")]
    public double? SelfReportedCirculatingSupply { get; set; }
    [JsonPropertyName("self_reported_market_cap")]
    public double? SelfReportedMarketCap { get; set; }
    [JsonPropertyName("Quote")]
    public Quote? Price { get; set; }
}

