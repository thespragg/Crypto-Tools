using Crypto_Tools.DAL;
using Crypto_Tools.Services;
using Microsoft.Extensions.Options;

namespace Crypto_Tools;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration) => (Configuration) = (configuration);
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CryptoToolsDatabaseSettings>(Configuration.GetSection(nameof(CryptoToolsDatabaseSettings)));

        services.AddSingleton<ICryptoToolsDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<CryptoToolsDatabaseSettings>>().Value);

        services.AddCors(o => o.AddPolicy("AllowAnyCorsPolicy", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));

        services.AddControllers();

        services.AddSingleton<IMarketCapService, MarketCapService>();
        services.AddSingleton<ICoinPriceService, CoinPriceService>();

        services.AddHostedService<CoinPriceTimerService>();
        services.AddHostedService<MarketCapCollectionService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CryptoTools"));

        app.UseCors("AllowAnyCorsPolicy");

        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}

