using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

using StatsDict = LottaCashMummy.Common.StatsDictionary<(int, int), double>;
using ConcurrentStatsDict = LottaCashMummy.Common.ConcurrentStatsDictionary<(int, int), double>;

namespace LottaCashMummy;

public class SimulationStats
{
    private readonly StatsMatrix<int> baseWinStats = new(SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS);
    public IStatsMatrix<int> BaseWinPayStats => baseWinStats;

    private List<SlotStats<ConcurrentStatsDict>> featureStats = new();
    public List<SlotStats<ConcurrentStatsDict>> FeatureStats => featureStats;

    public SimulationStats()
    {
        featureStats = new List<SlotStats<ConcurrentStatsDict>>();
        for (int i = 0; i < BonusTypeConverter.CombiTypeOrder.Count; i++)
        {
            featureStats.Add(new SlotStats<ConcurrentStatsDict>());
        }
    }

    public void MergeFromBuffer(ThreadLocalStorage buffer)
    {
        // baseWinStats
        var baseWinStats = buffer.SpinStats.BaseWinStats;
        foreach (var (symbol, hits, count) in baseWinStats.GetItems())
        {
            this.baseWinStats.Update(symbol, hits, count, (a, b) => a + b);
        }

        // featureStats
        foreach (var (key, value) in BonusTypeConverter.CombiTypeOrder)
        {
            var target = GetFeatureStats(key);
            var source = buffer.SpinStats.GetFeatureStats(key);

            MergeDict(target.SpinCounts, source.SpinCounts);
            MergeDict(target.RespinCounts, source.RespinCounts);
            MergeDict(target.LevelUpCounts, source.LevelUpCounts);
            MergeDict(target.CreateGemCount, source.CreateGemCount);
            MergeDict(target.ObtainGemValue, source.ObtainGemValue);
            MergeDict(target.CreateCoinCountA, source.CreateCoinCountA);
            MergeDict(target.CreateCoinCountB, source.CreateCoinCountB);
            MergeDict(target.ObtainCoinValueA, source.ObtainCoinValueA);
            MergeDict(target.ObtainCoinValueB, source.ObtainCoinValueB);
            MergeDict(target.RedCoinCount, source.RedCoinCount);
            MergeDict(target.FreeSpinCoinCount, source.FreeSpinCoinCount);
        }
    }

    private static void MergeDict(ConcurrentStatsDict target, StatsDict source)
    {
        foreach (var item in source.GetItems())
        {
            target.AddOrUpdate(item.Key, item.Value, (k, v) => v + item.Value);
        }
    }

    public SlotStats<ConcurrentStatsDict> GetFeatureStats(FeatureBonusType featureBonusType)
    {
        var idx = BonusTypeConverter.GetCombiTypeOrder(featureBonusType);
        return featureStats[idx - 1];
    }
}