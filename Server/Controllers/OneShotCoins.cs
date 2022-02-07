using Crypto_Tools.DAL;
using Microsoft.AspNetCore.Mvc;

namespace Crypto_Tools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OneShotCoins : ControllerBase
    {
        private readonly IMarketCapService _mcapService;
        public OneShotCoins(IMarketCapService mcapService) => (_mcapService) = (mcapService);

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var topMcaps = (await _mcapService.Get()).ToList();
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
            return Ok(Tuple.Create<List<string>,LowestCoin>(singlecoins, lowestCoin));
        }

        private class LowestCoin
        {
            public string? Coin { get; set; }
            public int Position { get; set; }
            public string? Date { get; set; }
        }
    }
}
