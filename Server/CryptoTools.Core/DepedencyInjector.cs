using CryptoTools.Core.DAL;
using CryptoTools.Core.Helpers;
using CryptoTools.Core.Interfaces;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using CryptoTools.Core.Strategies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTools.Core;

public class DepedencyInjector
{
    public static void ConfigureIOC(IServiceCollection services, IConfiguration config)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<CryptoToolsDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("CryptoToolsDbConnection")));

        services.AddHostedService<CmcDataGatherer>();
        services.AddTransient<IPortfolio, Portfolio>();

        /* Strategies */
        services.AddTransient<ITradeStrategy<BuyTheDip>, BuyTheDip>();
        services.AddScoped<ITradeStrategy<GenericDCA>, GenericDCA>();
        services.AddTransient<ITradeStrategy<OneTimeBuyTheTop>, OneTimeBuyTheTop>();
        services.AddTransient<ITradeStrategy<BuyTheDip>, BuyTheDip>();
    }
}
