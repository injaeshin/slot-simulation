using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class BaseStats
{
    public int SpinCount { get; set; }
    public byte SymbolType { get; set; }
    public int Hit { get; set; }
    public long Amount { get; set; }

    public BaseStats()
    {
        SpinCount = 0;
        SymbolType = 0;
        Hit = 0;
        Amount = 0;
    }

    public void Clear()
    {
        SpinCount = 0;
        SymbolType = 0;
        Hit = 0;
        Amount = 0;
    }

    public void AddSpinCount() => SpinCount++;

    public void AddPayWin(byte symbol, int hits, long amount)
    {
        SymbolType = symbol;
        Hit = (byte)hits;
        Amount += amount;
    }
}

public class FeatureStats
{
    public int Level { get; set; }
    public int SpinCounts { get; set; }
    public int CreateGemCount { get; set; }
    public double ObtainGemValue { get; set; }
    public int CreateCoinCountA { get; set; }
    public int CreateCoinCountB { get; set; }
    public double ObtainCoinValueA { get; set; }
    public double ObtainCoinValueB { get; set; }
    public int RedCoinCount { get; set; }
    public int FreeSpinCoinCount { get; set; }


    public int RespinCounts { get; set; }
    public int RespinCreateCoinCount { get; set; }
    public double RespinObtainCoinValue { get; set; }


    public void Clear()
    {
        Level = 0;
        SpinCounts = 0;
        CreateGemCount = 0;
        ObtainGemValue = 0;
        CreateCoinCountA = 0;
        CreateCoinCountB = 0;
        ObtainCoinValueA = 0;
        ObtainCoinValueB = 0;
        RedCoinCount = 0;
        FreeSpinCoinCount = 0;

        RespinCounts = 0;
        RespinCreateCoinCount = 0;
        RespinObtainCoinValue = 0;
    }
}
public class SpinStatistics
{
    private readonly BaseStats baseStats;
    public BaseStats BaseStats => baseStats;

    private readonly FeatureStats featureStats;
    public FeatureStats FeatureStats => featureStats;

    public SpinStatistics()
    {
        baseStats = new BaseStats();
        featureStats = new FeatureStats();
    }

    public void Clear()
    {
        baseStats.Clear();
        featureStats.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevel() => featureStats.Level++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFeatureSpinCount() => featureStats.SpinCounts++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCount() => featureStats.RespinCounts++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCreateGemCount() => featureStats.CreateGemCount++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddObtainGemValue(double value) => featureStats.ObtainGemValue += value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCreateCoinCount(FeatureBonusType type)
    {
        if ((type & FeatureBonusType.Collect) == FeatureBonusType.Collect)
        {
            featureStats.CreateCoinCountA++;
        }
        else
        {
            featureStats.CreateCoinCountB++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddObtainCoinValue(FeatureBonusType type, double value)
    {
        if ((type & FeatureBonusType.Collect) == FeatureBonusType.Collect)
        {
            featureStats.ObtainCoinValueA += value;
        }
        else
        {
            featureStats.ObtainCoinValueB += value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRedCoinCount() => featureStats.RedCoinCount++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFreeSpinCoinCount() => featureStats.FreeSpinCoinCount++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCreateCoinCount() => featureStats.RespinCreateCoinCount++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinObtainCoinValue(double value) => featureStats.RespinObtainCoinValue += value;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseSpinCount() => baseStats.AddSpinCount();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBasePayWin(byte symbol, int hits, long amount)
    {
        baseStats.AddPayWin(symbol, hits, amount);
    }
}