using System.Globalization;
using Crypto_Tools.DAL;
using Crypto_Tools.Models;
using Crypto_Tools.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Tools.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ETFController : ControllerBase
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    private readonly ILogger<ETFController> _logger;
    private readonly TopCoinsETF _topCoins;
    public ETFController(IMarketCapService mcapService, ICoinPriceService coinPriceService, ILogger<ETFController> logger, TopCoinsETF topCoins) => (_mcapService, _priceService, _logger, _topCoins) = (mcapService, coinPriceService, logger, topCoins);

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

