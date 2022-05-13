using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace CryptoTools.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DcaController : ControllerBase
    {
        private readonly GenericDCA _strategy;
        public DcaController(GenericDCA strategy) => _strategy = strategy;

        [HttpGet]
        public IActionResult Get([FromBody] StrategyOptions opts)
        {
            var portfolio = _strategy.Run(opts);
            return Ok(portfolio);
        }
    }
}
