using System.Runtime.CompilerServices;
using LottaCashMummy.Common;
using LottaCashMummy.Database;

namespace LottaCashMummy.Buffer;

public class ThreadLocalStorage
{
    private const int SEED = 0xABCDEF;

    private readonly Random random;
    public Random Random => random;

    private readonly BaseStorage baseStorage;
    public BaseStorage BaseStorage => baseStorage;

    private readonly FeatureStorage featureStorage;
    public FeatureStorage FeatureStorage => featureStorage;


    private readonly SlotStats spinStats;
    public SlotStats SpinStats => spinStats;

    public ThreadLocalStorage(IDbRepository dbRepository)
    {
        random = new Random(SEED);
        spinStats = new SlotStats();
        baseStorage = new BaseStorage(spinStats.BaseStats, dbRepository);
        featureStorage = new FeatureStorage(spinStats.FeatureStats);
    }

    public void Clear()
    {
        baseStorage.Clear();
        featureStorage.Clear();
    }


    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public double GetBaseWinAmount() => SpinStats.GetBaseWinAmount();

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public double GetFeatureWinAmount() => SpinStats.GetFeatureWinAmount();
}

