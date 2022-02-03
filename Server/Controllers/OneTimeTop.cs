using Crypto_Tools.DAL;
using Crypto_Tools.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Crypto_Tools.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OneTimeTop : ControllerBase
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    public OneTimeTop(ICoinPriceService coinPriceService, IMarketCapService mcapService) => (_priceService, _mcapService) = (coinPriceService, mcapService);

    [HttpGet]
    public async Task<IActionResult> Get(int amnt, int numCoins, string start, string end, string? ignored = null)
    {
        var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
        var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
        var ignoredCoins = ignored != null ? ignored.Split(",") : Array.Empty<string>();

        var portfolio = new OneTimeBuyTheTop(_mcapService, _priceService);
        var res = await portfolio.Run(amnt, numCoins, startDate, endDate, ignoredCoins);
        return Ok(res);
    }
}
