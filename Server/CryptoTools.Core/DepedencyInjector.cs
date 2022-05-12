using CryptoTools.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTools.Core;

public class DepedencyInjector
{
    public static void ConfigureIOC(IServiceCollection services)
    {
        services.AddHostedService<CmcDataGatherer>();
    }
}
