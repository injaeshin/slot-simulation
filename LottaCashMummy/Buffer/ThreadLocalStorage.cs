using LottaCashMummy.Statistics;

namespace LottaCashMummy.Buffer;

public class ThreadLocalStorage
{
    private const int SEED = 0xa1b2c3;

    private readonly Random random;
    public Random Random => random;

    private StatsResult statsResult;
    public StatsResult StatsResult => statsResult;

    private BaseStorage baseStorage;
    public BaseStorage BaseStorage => baseStorage;

    private FeatureStorage featureStorage;
    public FeatureStorage FeatureStorage => featureStorage;

    public ThreadLocalStorage()
    {
        random = new Random(SEED);
        statsResult = new StatsResult();
        baseStorage = new BaseStorage(statsResult.BaseGameStatsModel);
        featureStorage = new FeatureStorage(statsResult.FeatureGameStatsModel, random);
    }

    public void StatsClear()
    {
        statsResult = new StatsResult();
        baseStorage.StatsClear(statsResult.BaseGameStatsModel);
        featureStorage.StatsClear(statsResult.FeatureGameStatsModel);
    }
}

