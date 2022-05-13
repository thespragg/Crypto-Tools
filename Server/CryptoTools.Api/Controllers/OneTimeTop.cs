using CryptoTools.Core.Models;
using CryptoTools.Core.PortfolioStrategies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CryptoTools.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OneTimeTop : ControllerBase
{
    private readonly OneTimeBuyTheTop _strategy;
    public OneTimeTop(OneTimeBuyTheTop strategy) => _strategy = strategy;
    [HttpGet]
    public IActionResult Get(StrategyOptions opts)
    {
        opts.IgnoredCoins ??= new List<string>();
        var res = _strategy.Run(opts);
        return Ok(res);
    }
}
