using LineAndFreeGame.Common;
using LineAndFreeGame.Statistics;

namespace LineAndFreeGame.Service;

public class StatsService
{
    private readonly List<SpinStatistics> spinStats = [];

    public void AddSpinStats(SpinStatistics spinStats)
    {
        this.spinStats.Add(spinStats);
    }

    public long GetTotalSpinCount()
    {
        return spinStats.Sum(model => model.TotalSpinCount);
    }

    public long GetTotalWinPay()
    {
        return spinStats.Sum(model => model.TotalWinPays.Values.Sum());
    }

    public int GetBaseGameTotalPayWinAmount(SymbolType symbolType, int count)
    {
        return spinStats.Sum(model => model.TotalWinPays.GetValueOrDefault((symbolType, count), 0));
    }

    public double GetTotalBonusPay()
    {
        return spinStats.Sum(model => model.TotalBonusPay);
    }

    // bbsymbol write to file with csv format
    public void WriteBBSymbolToFile()
    {
        var filePath = "bbsymbol.csv";
        var csvContent = string.Join("\n", spinStats.SelectMany(model => model.BBSymbols.Select(symbol => $"{symbol.Item1},{symbol.Item2},{symbol.Item3},{symbol.Item4},{symbol.Item5}")));
        File.WriteAllText(filePath, csvContent);
    }
}
