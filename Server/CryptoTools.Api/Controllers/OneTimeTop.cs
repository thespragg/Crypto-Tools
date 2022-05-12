using CryptoTools.Api.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CryptoTools.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OneTimeTop : ControllerBase
{
    [HttpGet]
    public IActionResult Get(int amnt, int numCoins, string start, string end, string? ignored = null)
    {
        var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
        var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
        var ignoredCoins = ignored != null ? ignored.Split(",") : Array.Empty<string>();

        var portfolio = new OneTimeBuyTheTop();
        var res = portfolio.Run(amnt, numCoins, startDate, endDate, ignoredCoins);
        return Ok(res);
    }
}
