using CryptoTools.Core.DAL;
using Microsoft.AspNetCore.Mvc;

namespace CryptoTools.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OneShotCoins : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var db = new CryptoToolsDbContext();
            var topMcaps = db.MarketCapRankings.ToList();
            if (topMcaps == null) return NotFound();
            var singlecoins = topMcaps.SelectMany(x => x.Coins).GroupBy(x => x).Where(x => x.Count() == 1).SelectMany(x => x).Where(x=>x!= "sushi").ToList();
            LowestCoin? lowestCoin = null;

            foreach (var singlecoin in singlecoins)
            {
                foreach (var mcap in topMcaps)
                {
                    var index = mcap.Coins.IndexOf(singlecoin);
                    if (index != -1 && (lowestCoin == null || index < lowestCoin.Position)){
                        lowestCoin = new LowestCoin
                        {
                            Coin = singlecoin,
                            Position = index,
                            Date = mcap.Date.ToString("dd/MM/yyyy")
                        };
                    }
                }
            }
            return Ok(Tuple.Create(singlecoins, lowestCoin!));
        }

        private class LowestCoin
        {
            public string? Coin { get; set; }
            public int Position { get; set; }
            public string? Date { get; set; }
        }
    }
}
