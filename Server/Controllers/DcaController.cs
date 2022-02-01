using Crypto_Tools.DAL;
using Crypto_Tools.Models;
using Crypto_Tools.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace Crypto_Tools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DcaController : ControllerBase
    {
        private readonly ICoinPriceService _priceService;
        public DcaController(ICoinPriceService coinPriceService) => (_priceService) = (coinPriceService);

        [HttpGet]
        public async Task<IActionResult> Get(string allocation, int amnt, string interval, string start, string end)
        {
            var allocations = JsonSerializer.Deserialize<List<CoinAllocation>>(allocation, new JsonSerializerOptions {  PropertyNameCaseInsensitive = true });
            if (allocations == null) return BadRequest();
            if (!Enum.TryParse(typeof(DcaInterval), interval, out var parsedInterval)) return BadRequest();
            try
            {
                var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
                var dca = new GenericDCA(_priceService);
                var portfolio = await dca.Run(allocations, amnt, (DcaInterval)parsedInterval!, startDate, endDate);
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
