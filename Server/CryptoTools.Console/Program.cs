using CryptoTools.Console;
using CryptoTools.Core;
using Serilog;
using Serilog.Core;
using System.Reflection;


Log.Logger = new LoggerConfiguration()
           .MinimumLevel.ControlledBy(new LoggingLevelSwitch(Serilog.Events.LogEventLevel.Information))
           .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
           .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
           .Enrich.FromLogContext()
           .WriteTo.Console()
           .CreateLogger();

var config = new ConfigurationBuilder()
        .SetBasePath(GetApplicationRoot())
        .AddEnvironmentVariables()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        services.BuildServiceProvider(); DepedencyInjector.ConfigureIOC(services, config);
        services.AddHostedService<Worker>();
    })
    .Build();


string GetApplicationRoot()
{
    var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    if (exePath!.Contains("file:")) exePath = exePath.Remove(0, 5);
    if (!exePath.StartsWith("/var")) exePath = AppDomain.CurrentDomain.BaseDirectory;
    return exePath;
}

await host.RunAsync();
