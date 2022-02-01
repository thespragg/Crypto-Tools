using Crypto_Tools.DAL;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Tools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinController : ControllerBase
    {
        private readonly ICoinPriceService _coins;
        public CoinController(ICoinPriceService coins) => (_coins) = (coins);

        [HttpGet]
        public async Task<IActionResult> Get() => Ok((await _coins.Get()).Select(x => x.Name));
    }
}
