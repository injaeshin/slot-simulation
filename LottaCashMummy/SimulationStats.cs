using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy;

public class SimulationStats
{
    private readonly StatsMatrix<int> baseWinStats = new(SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS);
    public IStatsMatrix<int> BaseWinPayStats => baseWinStats;

    // 딕셔너리 기반 통계 제거하고 배열 기반 통계만 사용
    // private List<ArraySlotStats> featureStats = new();
    // public List<ArraySlotStats> FeatureStats => featureStats;

    private double baseWinAmount = 0.0;
    public double BaseWinAmount => baseWinAmount;
    
    private double featureWinAmount = 0.0;
    public double FeatureWinAmount => featureWinAmount;

    public SimulationStats()
    {
        // featureStats = new List<ArraySlotStats>(BonusTypeConverter.CombiTypeOrder.Count);
        // for (int i = 0; i < BonusTypeConverter.CombiTypeOrder.Count; i++)
        // {
        //     featureStats.Add(new ArraySlotStats());
        // }
    }

    public void MergeFromBuffer(ThreadLocalStorage buffer)
    {
        // baseWinAmount += buffer.GetBaseWinAmount();
        // featureWinAmount += buffer.GetFeatureWinAmount();

        // // baseWinStats
        // var baseWinStats = buffer.SpinStats.BaseWinStats;
        // foreach (var (symbol, hits, count) in baseWinStats.GetItems())
        // {
        //     this.baseWinStats.Update(symbol, hits, count, (a, b) => a + b);
        // }

        // // 배열 기반 통계 병합 - 각 피처 타입별로 개별적으로 병합
        // // SpinStatistics의 MergeArrayStats 메서드를 직접 사용할 수 없으므로
        // // 각 ArraySlotStats 객체의 MergeFrom 메서드를 사용하여 병합합니다.
        // foreach (var (key, value) in BonusTypeConverter.CombiTypeOrder)
        // {
        //     var target = GetFeatureStats(key);
        //     var source = buffer.SpinStats.GetFeatureStats(key);

        //     // 배열 기반 통계 병합
        //     target.MergeFrom(source);
        // }

        // buffer.SpinStats.Clear();
    }

    // public ArraySlotStats GetFeatureStats(FeatureBonusType featureBonusType)
    // {
    //     var idx = BonusTypeConverter.GetCombiTypeOrder(featureBonusType);
    //     return featureStats[idx - 1];
    // }
}