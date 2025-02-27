using System.Collections.Concurrent;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy;

public class SimulationState
{
    public int[,] WinPayTable { get; } = new int[SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS];

    public ConcurrentDictionary<(FeatureBonusType Type, int Level, int initGemCount), long> FeatureEnterCount { get; } = new();
    public ConcurrentDictionary<(FeatureBonusType Type, int Level, int initGemCount), long> FeatureSpinCount { get; } = new();
    public ConcurrentDictionary<(FeatureBonusType Type, int Level, int initGemCount), long> FeatureRedCoinCount { get; } = new();
    public ConcurrentDictionary<(FeatureBonusType Type, int Level, int initGemCount), long> FeatureRespinCount { get; } = new();
    public ConcurrentDictionary<(FeatureBonusType Type, int FromLevel, int initGemCount), long> FeatureLevelUpCount { get; } = new();
    public ConcurrentDictionary<(FeatureBonusType Type, int Level, int initGemCount), long> FeatureCreateGemCount { get; } = new();


    public void UpdateResults(ThreadLocalStorage buffer)
    {
        var payWin = buffer.SpinStats.WinPayTable;
        for (int symbol = 0; symbol < SlotConst.PAYTABLE_SYMBOL; symbol++)
        {
            for (int hits = 0; hits < SlotConst.MAX_HITS; hits++)
            {
                Interlocked.Add(ref WinPayTable[symbol, hits], payWin[symbol, hits]);
            }
        }

        var featureEnterCount = buffer.SpinStats.Feature.EnterCount;
        foreach (var entry in featureEnterCount)
        {
            FeatureEnterCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        var featureSpinCount = buffer.SpinStats.Feature.SpinCount;
        foreach (var entry in featureSpinCount)
        {
            FeatureSpinCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        var featureRedCoinCount = buffer.SpinStats.Feature.RedCoinCount;
        foreach (var entry in featureRedCoinCount)
        {
            FeatureRedCoinCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        var featureRespinCount = buffer.SpinStats.Feature.RespinCount;
        foreach (var entry in featureRespinCount)
        {
            FeatureRespinCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        var featureLevelupCount = buffer.SpinStats.Feature.LevelUpCount;
        foreach (var entry in featureLevelupCount)
        {
            FeatureLevelUpCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        var featureCreateGemCount = buffer.SpinStats.Feature.CreateGemCount;
        foreach (var entry in featureCreateGemCount)
        {
            FeatureCreateGemCount.AddOrUpdate(entry.Key, entry.Value, (_, count) => count + entry.Value);
        }

        buffer.SpinStats.Reset();
    }
}