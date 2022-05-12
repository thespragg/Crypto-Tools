using System.Reflection;

namespace CryptoTools.Api;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(hostConfig =>
            {
                hostConfig.SetBasePath(GetApplicationRoot());
                hostConfig.AddEnvironmentVariables();
                hostConfig.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

    public static string GetApplicationRoot()
    {
        var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (exePath!.Contains("file:")) exePath = exePath.Remove(0, 5);
        if (!exePath.StartsWith("/var")) exePath = AppDomain.CurrentDomain.BaseDirectory;
        return exePath;
    }
}

