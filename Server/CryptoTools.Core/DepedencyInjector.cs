using CryptoTools.Core.DAL;
using CryptoTools.Core.Helpers;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
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
        services.AddTransient<Portfolio>();

        /* Strategies */
        services.AddTransient<BuyTheDip>();
        services.AddTransient<GenericDCA>();
        services.AddTransient<OneTimeBuyTheTop>();
        services.AddTransient<BuyTheDip>();
    }
}
