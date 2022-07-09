using System.Globalization;
using CryptoTools.Core.Enums;
using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTools.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ETFController : ControllerBase
{
    // private readonly TopCoinsETF _topCoins;
    // public ETFController(TopCoinsETF topCoins) => (_topCoins) = (topCoins);
    //
    // [HttpGet]
    // public IActionResult Get(StrategyOptions opts)
    // {
    //     opts.IgnoredCoins ??= new List<string>();
    //     var portfolio = _topCoins.Run(opts);
    //     return Ok(portfolio);
    // }
}

