namespace Crypto_Tools.DAL;

public class CryptoToolsDatabaseSettings : ICryptoToolsDatabaseSettings
{
    public string MarketCapCollectionName { get; set; } = string.Empty;
    public string CoinPricesCollectionName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public interface ICryptoToolsDatabaseSettings
{
    string MarketCapCollectionName { get; set; }
    string CoinPricesCollectionName { get; set; }
    string ConnectionString { get; set; }
    string DatabaseName { get; set; }
}
