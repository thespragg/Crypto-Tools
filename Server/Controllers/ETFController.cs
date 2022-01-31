using Microsoft.AspNetCore.Mvc;

namespace Crypto_Tools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ETFController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {

        }
    }
}
