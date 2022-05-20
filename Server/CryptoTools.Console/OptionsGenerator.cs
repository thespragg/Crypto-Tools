using CryptoTools.Core.Strategies.StrategyOptions;
using System.Reflection;

namespace CryptoTools.Console;

internal class OptionsGenerator
{
    internal static Dictionary<int, List<BuyTheDipOptions>> SingleOption() => new()
    {
        {
            0,
            new List<BuyTheDipOptions> {
            new BuyTheDipOptions
            {
            FiatPurchaseAmount = 20,
            StartDate = DateTime.Parse("01 Jan 2018"),
            EndDate = DateTime.Parse("19 May 2022"),
            TakeProfitPercent = 8,
            PurchaseThresholdPercent = 20,
            Timeframe = 9,
            StopLossPercent = 14,
            TrailingTakeProfit = true,
            MarketCapRankingMax = 200
            }
        }
        }
    };

    internal static Dictionary<int, List<BuyTheDipOptions>> FromFileAsSingle(string filename)
    {
        var opts = ReadOptions(filename);
        var dict = new Dictionary<int, List<BuyTheDipOptions>>();
        for (var i = 0; i < opts.Count; i++) dict.Add(i, new List<BuyTheDipOptions>() { opts[i] });
        return dict;
    }

    internal static Dictionary<int, List<BuyTheDipOptions>> FromFileWithRandomDates(string filename, int numGenerations)
    {
        var opts = ReadOptions(filename);
        var dict = new Dictionary<int, List<BuyTheDipOptions>>();
        for (var i = 0; i < opts.Count; i++) dict.Add(i, GenerateDatedOptions(numGenerations, opts[i])); 
        return dict;
    }

    internal static Dictionary<int, List<BuyTheDipOptions>> RandomOptions(int num)
    {
        var dict = new Dictionary<int, List<BuyTheDipOptions>>();
        for(var i = 0; i < num; i++)
        {
            dict.Add(i, new List<BuyTheDipOptions> { GenerateOptions() });
        }
        return dict;
    }

    internal static Dictionary<int, List<BuyTheDipOptions>> RandomOptionsWithRandomDates(int num, int generations)
    {
        var dict = new Dictionary<int, List<BuyTheDipOptions>>();
        for (var i = 0; i < num; i++)
        {
            var opt = GenerateOptions();
            dict.Add(i, GenerateDatedOptions(generations, opt));
        }
        return dict;
    }

    private static Random rnd = new Random();

    private static (DateTime, DateTime) GetDateRange(DateTime start, DateTime end)
    {
        var days = rnd.Next(200, (int)(end - start).TotalDays);
        var maxDate = end.AddDays(-days);
        var startDays = rnd.Next(0, (int)(maxDate - start).TotalDays);

        var startDate = start.AddDays(startDays);
        var endDate = startDate.AddDays(days);
        return (startDate, endDate);
    }

    private static List<BuyTheDipOptions> GenerateDatedOptions(int num, BuyTheDipOptions baseOpts)
    {
        var res = new List<BuyTheDipOptions>();
        for (var i = 0; i < num; i++)
        {
            var (start, end) = GetDateRange(DateTime.Parse("01 Jan 2017"), DateTime.Parse("01 May 2022"));
            res.Add(new BuyTheDipOptions
            {
                FiatPurchaseAmount = baseOpts.FiatPurchaseAmount,
                MarketCapRankingMax = baseOpts.MarketCapRankingMax,
                PurchaseThresholdPercent = baseOpts.PurchaseThresholdPercent,
                TakeProfitPercent = baseOpts.TakeProfitPercent,
                Timeframe = baseOpts.Timeframe,
                StopLossPercent = baseOpts.StopLossPercent,
                TrailingTakeProfit = baseOpts.TrailingTakeProfit,
                StartDate = start,
                EndDate = end,
            });
        }
        return res;
    }

    private static BuyTheDipOptions GenerateOptions()
    {
        var rnd = new Random();
        return new BuyTheDipOptions
        {
            StartDate = DateTime.Parse("01/01/2018"),
            EndDate = DateTime.Parse("01/01/2022"),
            FiatPurchaseAmount = 10,
            PurchaseThresholdPercent = rnd.Next(2, 10) * 10,
            Timeframe = rnd.Next(2, 10),
            MarketCapRankingMax = rnd.Next(1, 6) * 100,
            TakeProfitPercent = rnd.Next(1, 100),
            TrailingTakeProfit = true,
            StopLossPercent = rnd.Next(1, 100)
        };
    }

    private static List<BuyTheDipOptions> ReadOptions(string filename)
    {
        var data = File.ReadAllLines(Path.Combine(GetApplicationRoot(), filename));
        var options = new List<BuyTheDipOptions>();
        foreach (var line in data)
        {
            var split = line.Split(",");
            options.Add(new BuyTheDipOptions
            {
                StartDate = DateTime.Parse("01 Jan 2018"),
                EndDate = DateTime.Parse("01 May 2022"),
                FiatPurchaseAmount = 20,
                PurchaseThresholdPercent = float.Parse(split[1]),
                Timeframe = int.Parse(split[2]),
                MarketCapRankingMax = int.Parse(split[3]),
                TakeProfitPercent = float.Parse(split[4]),
                TrailingTakeProfit = true,
                StopLossPercent = float.Parse(split[5])
            });
        }
        return options;
    }

    private static string GetApplicationRoot()
    {
        var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (exePath!.Contains("file:")) exePath = exePath.Remove(0, 5);
        if (!exePath.StartsWith("/var")) exePath = AppDomain.CurrentDomain.BaseDirectory;
        return exePath;
    }
}