using MoMoMummy.Statistics.Model;

namespace MoMoMummy.Statistics;

public class StatsResult
{
    private BaseGameStatsModel baseGameStatsModel;
    public BaseGameStatsModel BaseGameStatsModel => baseGameStatsModel;
    
    private FeatureGameStatsModel featureGameStatsModel;
    public FeatureGameStatsModel FeatureGameStatsModel => featureGameStatsModel;

    public StatsResult()
    {
        baseGameStatsModel = new BaseGameStatsModel();
        featureGameStatsModel = new FeatureGameStatsModel();
    }
}