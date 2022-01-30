using System.Threading.Tasks;

namespace DCA_profitability
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var etf = new TopCoinETF();
            await etf.Run("TopCoinsByMonth.json", 100);
        }
    }
}
