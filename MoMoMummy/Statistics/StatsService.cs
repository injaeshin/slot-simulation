using MoMoMummy.Statistics;

public class StatsService
{
    private StatsResult[] statsResults = [];

    public void SetStatsResults(StatsResult[] statsResults)
    {
        this.statsResults = statsResults;
    }

    public bool SetStatsResults(long batchIndex, StatsResult statsResult)
    {
        if (batchIndex >= statsResults.Length)
        {
            return false;
        }

        this.statsResults[batchIndex] = statsResult;
        return true;
    }

    public long GetBaseGameTotalSpinCount()
    {
        long totalSpinCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalSpinCount += statsResult.BaseGameStatsModel.GetSpinCount();
        }
        return totalSpinCount;
    }

    public long GetBaseGameTotalPayWinAmount(int symbol, int hits)
    {
        long totalPayWinAmount = 0;
        foreach (var statsResult in statsResults)
        {
            totalPayWinAmount += statsResult.BaseGameStatsModel.GetWinPay(symbol, hits);
        }
        return totalPayWinAmount;
    }

    public long GetBonusGameTotalLevelCount(int bonusType, int gem, int level)
    {
        long totalLevelCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalLevelCount += statsResult.FeatureGameStatsModel.LevelCount.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalLevelCount;
    }

    public long GetBonusGameTotalGemSpinCount(int bonusType, int gem, int level)
    {
        long totalGemSpinCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalGemSpinCount += statsResult.FeatureGameStatsModel.GemSpinCount.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalGemSpinCount;
    }

    public long GetBonusGameTotalCoinSpinCount(int bonusType, int gem, int level)
    {
        long totalCoinSpinCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalCoinSpinCount += statsResult.FeatureGameStatsModel.CoinSpinCount.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalCoinSpinCount;
    }

    public long GetBonusGameTotalGemCount(int bonusType, int gem, int level)
    {
        long totalGemCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalGemCount += statsResult.FeatureGameStatsModel.GemCount.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalGemCount;
    }

    public double GetBonusGameTotalGemValue(int bonusType, int gem, int level)
    {
        double totalGemValue = 0;
        foreach (var statsResult in statsResults)
        {
            totalGemValue += statsResult.FeatureGameStatsModel.GemValue.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalGemValue;
    }

    public long GetBonusGameTotalGemSpinCount(int bonusType, int gem, byte level)
    {
        long totalGemSpinCount = 0;
        foreach (var statsResult in statsResults)
        {
            totalGemSpinCount += statsResult.FeatureGameStatsModel.GemSpinCount.GetValueOrDefault((bonusType, gem, level), 0);
        }
        return totalGemSpinCount;
    }
}
