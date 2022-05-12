using CryptoTools.Api.Models;
using CryptoTools.Api.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace CryptoTools.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DcaController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string allocation, int amnt, string interval, string start, string end)
        {
            var allocations = JsonSerializer.Deserialize<List<CoinAllocation>>(allocation, new JsonSerializerOptions {  PropertyNameCaseInsensitive = true });
            if (allocations == null) return BadRequest();
            if (!Enum.TryParse(typeof(DcaInterval), interval, out var parsedInterval)) return BadRequest();
            try
            {
                var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
                var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);
                var dca = new GenericDCA();
                var portfolio = dca.Run(allocations, amnt, (DcaInterval)parsedInterval!, startDate, endDate);
                return Ok(portfolio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
