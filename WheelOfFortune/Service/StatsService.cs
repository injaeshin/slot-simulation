using WheelOfFortune.Shared;

namespace WheelOfFortune.Statistics;

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

    public int GetBaseGameTotalPayWinAmount(CombinationPayType combinationPayType)
    {
        return spinStats.Sum(model => model.TotalWinPays.GetValueOrDefault(combinationPayType, 0));
    }

    public double GetTotalBonusPay()
    {
        return spinStats.Sum(model => model.TotalBonusPay);
    }
}
