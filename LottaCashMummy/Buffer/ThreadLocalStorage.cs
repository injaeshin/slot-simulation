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

    public int BaseWinAmount { get; private set; }
    public int FeatureWinAmount { get; private set; }

    public SpinStatistics SpinStats { get; private set; }

    public ThreadLocalStorage()
    {
        SpinStats = new SpinStatistics();

        random = new Random(SEED);
        baseStorage = new BaseStorage(SpinStats);
        featureStorage = new FeatureStorage(SpinStats);
    }

    public void Clear()
    {
        baseStorage.Clear();
        FeatureStorage.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseWinAmount(int amount) => BaseWinAmount += amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFeatureWinAmount(int amount) => FeatureWinAmount += amount;
}

