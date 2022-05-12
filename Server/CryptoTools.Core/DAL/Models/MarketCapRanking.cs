using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CryptoTools.Core.DAL.Models;

[Table("MarketCapRankings")]
public class MarketCapRanking
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public DateTime Date { get; set; }
    public List<string> Coins { get; set; } = new List<string>();
}
