using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

using StatsDict = LottaCashMummy.Common.StatsDictionary<(int, int), double>;

namespace LottaCashMummy.Buffer;

public class SlotStats<TDictImpl> where TDictImpl : IStatsDictionary<(int, int), double>, new()
{
    private readonly TDictImpl spinCounts = new();
    public TDictImpl SpinCounts => spinCounts;

    private readonly TDictImpl respinCounts = new();
    public TDictImpl RespinCounts => respinCounts;

    private readonly TDictImpl levelUpCounts = new();
    public TDictImpl LevelUpCounts => levelUpCounts;

    private readonly TDictImpl createGemCount = new();
    public TDictImpl CreateGemCount => createGemCount;

    private readonly TDictImpl obtainGemValue = new();
    public TDictImpl ObtainGemValue => obtainGemValue;

    private readonly TDictImpl createCoinCountA = new();
    public TDictImpl CreateCoinCountA => createCoinCountA;

    private readonly TDictImpl createCoinCountB = new();
    public TDictImpl CreateCoinCountB => createCoinCountB;

    private readonly TDictImpl obtainCoinValueA = new();
    public TDictImpl ObtainCoinValueA => obtainCoinValueA;

    private readonly TDictImpl obtainCoinValueB = new();
    public TDictImpl ObtainCoinValueB => obtainCoinValueB;

    private readonly TDictImpl redCoinCount = new();
    public TDictImpl RedCoinCount => redCoinCount;

    private readonly TDictImpl freeSpinCoinCount = new();
    public TDictImpl FreeSpinCoinCount => freeSpinCoinCount;

    public void Clear()
    {
        spinCounts.Clear();
        respinCounts.Clear();
        levelUpCounts.Clear();
        createCoinCountA.Clear();
        createCoinCountB.Clear();
        obtainCoinValueA.Clear();
        obtainCoinValueB.Clear();
        createGemCount.Clear();
        obtainGemValue.Clear();
        redCoinCount.Clear();
        freeSpinCoinCount.Clear();
    }

    public void IncrementSpinCount(int level, int gems)
    {
        spinCounts.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }

    public void IncrementRespinCount(int level, int gems)
    {
        respinCounts.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }

    public void IncrementLevelUpCount(int level, int gems)
    {
        levelUpCounts.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }

    public void IncrementCreateCoinCount(FeatureBonusType type, int level, int gems)
    {
        var withRedCoin = (type & FeatureBonusType.Collect) == FeatureBonusType.Collect;
        if (withRedCoin)
        {
            createCoinCountA.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
        }
        else
        {
            createCoinCountB.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
        }
    }

    public void IncrementObtainCoinValue(FeatureBonusType type, int level, int gems, double value)
    {
        var withRedCoin = (type & FeatureBonusType.Collect) == FeatureBonusType.Collect;
        if (withRedCoin)
        {
            obtainCoinValueA.AddOrUpdate((level, gems), value, (k, v) => v + value);
        }
        else
        {
            obtainCoinValueB.AddOrUpdate((level, gems), value, (k, v) => v + value);
        }
    }

    public void IncrementCreateGemCount(int level, int gems)
    {
        createGemCount.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }

    public void IncrementObtainGemValue(int level, int gems, double value)
    {
        obtainGemValue.AddOrUpdate((level, gems), value, (k, v) => v + value);
    }

    public void IncrementRedCoinCount(int level, int gems)
    {
        redCoinCount.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }

    public void IncrementFreeSpinCoinCount(int level, int gems)
    {
        freeSpinCoinCount.AddOrUpdate((level, gems), 1, (k, v) => v + 1);
    }
}

public class SpinStatistics
{
    //private FeatureBonusCombiType featureBonusCombiType;
    //public FeatureBonusCombiType FeatureBonusCombiType => featureBonusCombiType;

    private readonly StatsMatrix<int> baseWinStats;
    public IStatsMatrix<int> BaseWinStats => baseWinStats;

    private readonly List<SlotStats<StatsDict>> featureCounts;
    public List<SlotStats<StatsDict>> FeatureCounts => featureCounts;

    private long baseWinAmount;
    public long BaseWinAmount => baseWinAmount;

    public SpinStatistics()
    {
        baseWinStats = new StatsMatrix<int>(SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS);

        featureCounts = new List<SlotStats<StatsDict>>();
        for (int i = 0; i < BonusTypeConverter.CombiTypeOrder.Count; i++)
        {
            featureCounts.Add(new SlotStats<StatsDict>());
        }

        baseWinAmount = 0;
    }

    public SlotStats<StatsDict> GetFeatureStats(FeatureBonusType featureBonusType)
    {
        var idx = BonusTypeConverter.GetCombiTypeOrder(featureBonusType);
        return featureCounts[idx - 1];
    }

    public void Reset()
    {
        baseWinStats.Clear();
        foreach (var featureCount in featureCounts)
        {
            featureCount.Clear();
        }
        baseWinAmount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseWinCount(byte symbol, int hits)
    {
        baseWinStats.Update(symbol, hits, 1, (a, b) => a + b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseWinAmount(long amount)
    {
        baseWinAmount += amount;
    }
}

