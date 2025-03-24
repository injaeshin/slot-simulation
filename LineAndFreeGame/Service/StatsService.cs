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
}
