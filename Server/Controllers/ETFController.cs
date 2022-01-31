using System.Globalization;
using Crypto_Tools.DAL;
using Crypto_Tools.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using static Crypto_Tools.PortfolioStrategies.TopCoinsETF;

namespace Crypto_Tools.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ETFController : ControllerBase
{
    private readonly ICoinPriceService _priceService;
    private readonly IMarketCapService _mcapService;
    public ETFController(IMarketCapService mcapService, ICoinPriceService coinPriceService) => (_mcapService, _priceService) = (mcapService, coinPriceService);


    [HttpGet]
    public async Task<IActionResult> Get(int amnt, int coins, string interval, string start, string end)
    {
        if (coins > 500) return BadRequest();
        if (!Enum.TryParse(typeof(DcaInterval), interval, out var parsedInterval)) return BadRequest();
        try
        {
            var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
            var etf = new TopCoinsETF(_mcapService, _priceService);
            return Ok(await etf.Run(amnt, coins, (DcaInterval)parsedInterval!, startDate, endDate));
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }
}

