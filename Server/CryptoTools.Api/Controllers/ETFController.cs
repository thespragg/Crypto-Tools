using System.Globalization;
using CryptoTools.Api.Models;
using CryptoTools.Api.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTools.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ETFController : ControllerBase
{
    private readonly ILogger<ETFController> _logger;
    private readonly TopCoinsETF _topCoins;
    public ETFController(ILogger<ETFController> logger, TopCoinsETF topCoins) => (_logger, _topCoins) = (logger, topCoins);

    [HttpGet]
    public async Task<IActionResult> Get(int amnt, int coins, string interval, string start, string end, string? ignored = null)
    {
        _logger.LogInformation($"REQUEST [{Request.HttpContext.Connection.RemoteIpAddress}]: ETF with params - amnt:{amnt},coins:{coins},interval:{interval},start:{start},end:{end},ignored:{ignored}");
        if (coins > 500) return BadRequest();
        if (!Enum.TryParse(typeof(DcaInterval), interval, out var parsedInterval)) return BadRequest();
        try
        {
            var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
            var ignoredCoins = ignored != null ? ignored.Split(",") : Array.Empty<string>();
            _logger.LogInformation($"Request from [{Request.HttpContext.Connection.RemoteIpAddress}] is valid, running etf portfolio.");
            var portfolio = await _topCoins.Run(amnt, coins, (DcaInterval)parsedInterval!, startDate, endDate, ignoredCoins.Select(x=>x.ToLower()).ToArray());
            return Ok(portfolio);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ex.Message);
        }

    }
}

