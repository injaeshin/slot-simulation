using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class SlotStats
{
    private readonly BaseStats baseStats;
    public BaseStats BaseStats => baseStats;

    private readonly FeatureStats featureStats;
    public FeatureStats FeatureStats => featureStats;

    public SlotStats()
    {
        baseStats = new BaseStats();
        featureStats = new FeatureStats();
    }

    public void Reset()
    {
        baseStats.Reset();
        featureStats.Reset();
    }
}