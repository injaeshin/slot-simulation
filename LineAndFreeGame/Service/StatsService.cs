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

    public long GetTotalBaseSpinCount()
    {
        return spinStats.Sum(model => model.TotalBaseSpinCount);
    }

    public long GetTotalFreeGameTriggerCount()
    {
        return spinStats.Sum(model => model.TotalFreeSpinTriggerCount);
    }

    public long GetTotalBaseGameWinPay()
    {
        return spinStats.Sum(model => model.LineGameWinPays.Values.Sum());
    }

    public long GetTotalFreeGameWinPay()
    {
        return spinStats.Sum(model => model.TotalFreeGameWinPays.Values.Sum());
    }

    public int GetLineGameTotalPayWinAmount(SymbolType symbolType, int count)
    {
        return spinStats.Sum(model => model.LineGameWinPays.GetValueOrDefault((symbolType, count), 0));
    }

    public int GetFreeGameTotalPayWinAmount(SymbolType symbolType, int count)
    {
        return spinStats.Sum(model => model.TotalFreeGameWinPays.GetValueOrDefault((symbolType, count), 0));
    }

    public double GetAvgFreeSpinExecutions(int initSpin)
    {
        // 전체 스핀 실행 수
        var totalExecutions = spinStats.Sum(model => model.TotalFreeSpinCount);

        // 프리스핀 실행 수
        var totalFreeSpinExecutions = spinStats.Sum(model => model.TotalFreeSpinExecutions.Where(v => v.Item1 == initSpin).Sum(v => v.Item2));

        return totalFreeSpinExecutions / totalExecutions;
    }

    public long GetTotalFreeSpinCount()
    {
        return spinStats.Sum(model => model.TotalFreeSpinCount);
    }

    public long GetLineScatterCount(int count)
    {
        return spinStats.Sum(model => count switch
        {
            3 => model.LineScatter3Count,
            4 => model.LineScatter4Count,
            5 => model.LineScatter5Count,
            _ => 0
        });
    }
}
