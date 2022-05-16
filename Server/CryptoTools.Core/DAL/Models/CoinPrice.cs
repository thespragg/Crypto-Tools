using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTools.Core.DAL.Models;

[Table("CoinPrices")]
public class CoinPrice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string CoinSymbol { get; set; } = string.Empty;
    [Required]
    public string CoinName { get; set; } = string.Empty;
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public decimal Price { get; set; }
    public int MarketCapRank { get; set; }
}
