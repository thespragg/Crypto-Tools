using Crypto_Tools.DAL;
using Crypto_Tools.Models;
using Crypto_Tools.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Crypto_Tools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyTheDipController : ControllerBase
    {
        private readonly ICoinPriceService _coins;
        public BuyTheDipController(ICoinPriceService coins) => (_coins) = (coins);

        [HttpGet]
        public async Task<IActionResult> Get(int amnt, string start, string end, int top, int dropPercent, int returnPercent)
        {
            var startDate = DateTime.ParseExact(start, "yyyyMMdd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(end, "yyyyMMdd", CultureInfo.InvariantCulture);

            var btd = new BuyTheDip(_coins);
            var x = await btd.Run(amnt, startDate, endDate, top, dropPercent, returnPercent);
            return Ok(new Result(x.Item1,x.Item2,x.Item3));
        }
        private class Result
        {
            public Result(float value, float spent, List<CoinProfit> portfolio) => (Value, Spent, Portfolio) = (value, spent, portfolio);

            public float Value { get; set; }
            public float Spent { get; set; }
            public List<CoinProfit> Portfolio { get; set; }
        }
    }
}
