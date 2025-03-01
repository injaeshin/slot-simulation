using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class ThreadLocalStorage
{
    private int SEED = 0xABCDEF;

    private readonly Random random;
    public Random Random => random;

    private readonly BaseStorage baseStorage;
    public BaseStorage BaseStorage => baseStorage;

    private readonly FeatureStorage featureStorage;
    public FeatureStorage FeatureStorage => featureStorage;


    private readonly SpinStatistics spinStats;
    public SpinStatistics SpinStats => spinStats;

    public ThreadLocalStorage()
    {
        random = new Random(SEED);
        spinStats = new SpinStatistics();
        baseStorage = new BaseStorage(spinStats);
        featureStorage = new FeatureStorage(spinStats);
    }

    public void Clear()
    {
        baseStorage.Clear();
        FeatureStorage.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetBaseWinAmount() => SpinStats.GetBaseWinAmount();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetFeatureWinAmount() => SpinStats.GetFeatureWinAmount();
}

