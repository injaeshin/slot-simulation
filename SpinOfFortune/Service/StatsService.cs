using SpinOfFortune.Shared;

namespace SpinOfFortune.Statistics;

public class StatsService
{
    private List<BaseStatsModel> baseStatsModels = [];

    public void AddBaseStatsModel(BaseStatsModel baseStatsModel)
    {
        baseStatsModels.Add(baseStatsModel);
    }

    public long GetTotalSpinCount()
    {
        return baseStatsModels.Sum(model => model.TotalSpinCount);
    }

    public long GetTotalWinPay()
    {
        return baseStatsModels.Sum(model => model.TotalWinPays.Values.Sum());
    }

    public int GetBaseGameTotalPayWinAmount(CombinationPayType combinationPayType)
    {
        return baseStatsModels.Sum(model => model.TotalWinPays.GetValueOrDefault(combinationPayType, 0));
    }
}
