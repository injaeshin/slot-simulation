using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevel(FeatureBonusType bonusType, int initGemCount, int level)
    {
        featureStats.AddLevel(bonusType, initGemCount, level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFeatureSpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        featureStats.AddSpinCount(bonusType, initGemCount, level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddObtainRespinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        //featureStats.AddSpinCount(bonusType, initGemCount, level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCreateGemCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        featureStats.AddGemCount(bonusType, initGemCount, level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddObtainGemValue(FeatureBonusType bonusType, int initGemCount, int level, double value)
    {
        featureStats.AddGemValue(bonusType, initGemCount, level, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCreateCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        if ((bonusType & FeatureBonusType.Collect) == FeatureBonusType.Collect)
        {
            featureStats.AddCoinWithRedCount(bonusType, initGemCount, level);
        }
        else
        {
            featureStats.AddCoinNoRedCount(bonusType, initGemCount, level);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddObtainCoinValue(FeatureBonusType bonusType, int initGemCount, int level, double value)
    {
        if ((bonusType & FeatureBonusType.Collect) == FeatureBonusType.Collect)
        {
            featureStats.AddCoinWithRedValue(bonusType, initGemCount, level, value);
        }
        else
        {
            featureStats.AddCoinNoRedValue(bonusType, initGemCount, level, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRedCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        //featureStats.RedCoinCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFreeSpinCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        //featureStats.FreeSpinCoinCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCreateCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        //featureStats.RespinCreateCoinCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinObtainCoinValue(FeatureBonusType bonusType, int initGemCount, int level, double value)
    {
        //featureStats.RespinObtainCoinValue += value;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseSpinCount() => baseStats.AddSpinCount();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBasePayWin(byte symbol, int hits, long amount)
    {
        baseStats.AddPayWin(symbol, hits, amount);
    }
}