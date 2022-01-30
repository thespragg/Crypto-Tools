using System;
using System.Collections.Generic;

namespace DCA_profitability.Models
{
    public class TopCoins
    {
        public DateTime Date { get; set; }
        public List<Coin> Coins { get; set; }
        public int Amount { get => Coins.Count; }
        public TopCoins(DateTime date) => (Date, Coins) = (date, new List<Coin>());
    }

    public class Coin
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
