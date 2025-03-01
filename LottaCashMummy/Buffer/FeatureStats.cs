using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class FeatureStats
{
    private readonly HashSet<(FeatureBonusType, int, int)> usedKeys = new();
    private readonly Dictionary<(FeatureBonusType, int, int), int> levelCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> LevelCount => levelCount;
    private readonly Dictionary<(FeatureBonusType, int, int), int> spinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> SpinCount => spinCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> gemCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> GemCount => gemCount;

    private readonly Dictionary<(FeatureBonusType, int, int), double> gemValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> GemValue => gemValue;

    private readonly Dictionary<(FeatureBonusType, int, int), int> coinWithRedCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> CoinWithRedCount => coinWithRedCount;

    private readonly Dictionary<(FeatureBonusType, int, int), double> coinWithRedValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> CoinWithRedValue => coinWithRedValue;

    private readonly Dictionary<(FeatureBonusType, int, int), int> coinNoRedCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> CoinNoRedCount => coinNoRedCount;

    private readonly Dictionary<(FeatureBonusType, int, int), double> coinNoRedValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> CoinNoRedValue => coinNoRedValue;

    public void Reset()
    {
        foreach (var key in usedKeys)
        {
            levelCount[key] = 0;
            spinCount[key] = 0;
            gemCount[key] = 0;
            gemValue[key] = 0;
            coinWithRedCount[key] = 0;
            coinWithRedValue[key] = 0;
            coinNoRedCount[key] = 0;
            coinNoRedValue[key] = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevel(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        if (usedKeys.Add(key))
        {
            levelCount[key] = 0;
            spinCount[key] = 0;
            gemCount[key] = 0;
            gemValue[key] = 0;
            coinWithRedCount[key] = 0;
            coinWithRedValue[key] = 0;
            coinNoRedCount[key] = 0;
            coinNoRedValue[key] = 0;
        }

        levelCount[key]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.spinCount[key] = this.spinCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddGemCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.gemCount[key] = this.gemCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddGemValue(FeatureBonusType bonusType, int initGemCount, int level, double gemValue)
    {
        var key = (bonusType, initGemCount, level);
        this.gemValue[key] = this.gemValue.GetValueOrDefault(key, 0) + gemValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCoinWithRedCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.coinWithRedCount[key] = this.coinWithRedCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCoinWithRedValue(FeatureBonusType bonusType, int initGemCount, int level, double coinValue)
    {
        var key = (bonusType, initGemCount, level);
        this.coinWithRedValue[key] = this.coinWithRedValue.GetValueOrDefault(key, 0) + coinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCoinNoRedCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.coinNoRedCount[key] = this.coinNoRedCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCoinNoRedValue(FeatureBonusType bonusType, int initGemCount, int level, double coinValue)
    {
        var key = (bonusType, initGemCount, level);
        this.coinNoRedValue[key] = this.coinNoRedValue.GetValueOrDefault(key, 0) + coinValue;
    }
}