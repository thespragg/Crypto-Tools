using CryptoTools.Core.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools.Core.DAL;

public class CryptoToolsDbContext : DbContext
{
    public CryptoToolsDbContext() { }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    public DbSet<CoinPrice> CoinPrices { get; set; }
    public DbSet<MarketCapRanking> MarketCapRankings { get; set; }
}
