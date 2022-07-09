using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CryptoTools.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuyTheDipController : ControllerBase
    {
        // private readonly BuyTheDip _strategy;
        // public BuyTheDipController(BuyTheDip strategy) => _strategy = strategy;
        //
        // [HttpGet]
        // public IActionResult Get(BuyTheDipOptions opts)
        // {
        //     var x = _strategy.Run(opts);
        //     return Ok(x);
        // }
        // private class Result
        // {
        //     public Result(float value, float spent, List<CoinProfit> portfolio) => (Value, Spent, Portfolio) = (value, spent, portfolio);
        //     public float Value { get; set; }
        //     public float Spent { get; set; }
        //     public List<CoinProfit> Portfolio { get; set; }
        // }
    }
}
