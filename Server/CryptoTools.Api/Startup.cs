using CryptoTools.Api.PortfolioStrategies;
using CryptoTools.Api.Services;
using CryptoTools.Core;
using CryptoTools.Core.DAL;
using Microsoft.EntityFrameworkCore;

namespace CryptoTools.Api;

public class Startup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration) => (Configuration) = (configuration);
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddCors(o => o.AddPolicy("AllowAnyCorsPolicy", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        }));

        services.AddControllers();

        DepedencyInjector.ConfigureIOC(services);

        services.AddTransient<TopCoinsETF>();

        services.AddHostedService<CoinPriceTimerService>();
        services.AddHostedService<MarketCapCollectionService>();

        services.AddEntityFrameworkNpgsql().AddDbContext<CryptoToolsDbContext>(opt =>
            opt.UseNpgsql(Configuration.GetConnectionString("CryptoToolsDbConnection")));

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

