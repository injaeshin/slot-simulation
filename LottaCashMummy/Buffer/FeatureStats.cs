using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class Level
{
    public int LevelCount { get; set; }
    public int SpinCount { get; set; }
}

public class FeatureStats
{
    private readonly HashSet<(FeatureBonusType, int, int)> usedKeys = new();

    private readonly Dictionary<(FeatureBonusType, int, int), Level> enterCount = new();
    public Dictionary<(FeatureBonusType, int, int), Level> EnterCount => enterCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> levelCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> LevelCount => levelCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> spinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> SpinCount => spinCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> gemCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> GemCount => gemCount;

    private readonly Dictionary<(FeatureBonusType, int, int), double> gemValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> GemValue => gemValue;

    private readonly Dictionary<(FeatureBonusType, int, int), int> coinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> CoinCount => coinCount;

    private readonly Dictionary<(FeatureBonusType, int, int), double> coinValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> CoinValue => coinValue;

    private readonly Dictionary<(FeatureBonusType, int, int), int> splitCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> SplitCount => splitCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> spinAdd1SpinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> SpinAdd1SpinCount => spinAdd1SpinCount;

    #region respin
    private readonly Dictionary<(FeatureBonusType, int, int), int> redcoinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> RedCoinCount => redcoinCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> respinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> RespinCount => respinCount;

    private readonly Dictionary<(FeatureBonusType, int, int), int> respinCoinCount = new();
    public Dictionary<(FeatureBonusType, int, int), int> RespinCoinCount => respinCoinCount;
    
    private readonly Dictionary<(FeatureBonusType, int, int), double> respinCoinValue = new();
    public Dictionary<(FeatureBonusType, int, int), double> RespinCoinValue => respinCoinValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRedCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.redcoinCount[key] = this.redcoinCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.respinCount[key] = this.respinCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.respinCoinCount[key] = this.respinCoinCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCoinValue(FeatureBonusType bonusType, int initGemCount, int level, double coinValue)
    {
        var key = (bonusType, initGemCount, level);
        this.respinCoinValue[key] = this.respinCoinValue.GetValueOrDefault(key, 0) + coinValue;
    }

    #endregion}

    public void Reset()
    {
        foreach (var key in usedKeys)
        {
            enterCount[key].EnterCount = 0;
            enterCount[key].SpinCount = 0;

            levelCount[key] = 0;
            spinCount[key] = 0;
            gemCount[key] = 0;
            gemValue[key] = 0;
            coinCount[key] = 0;
            coinValue[key] = 0;
            splitCount[key] = 0;
            spinAdd1SpinCount[key] = 0;

            redcoinCount[key] = 0;
            respinCount[key] = 0;
            respinCoinCount[key] = 0;
            respinCoinValue[key] = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddEnter(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        if (usedKeys.Add(key))
        {
            enterCount[key] = new Enter();

            levelCount[key] = 0;
            spinCount[key] = 0;
            gemCount[key] = 0;
            gemValue[key] = 0;
            coinCount[key] = 0;
            coinValue[key] = 0;
            splitCount[key] = 0;
            spinAdd1SpinCount[key] = 0;

            redcoinCount[key] = 0;
            respinCount[key] = 0;
            respinCoinCount[key] = 0;
            respinCoinValue[key] = 0;
        }

        levelCount[key] = levelCount.GetValueOrDefault(key, 0) + 1;

        var enter = enterCount.GetValueOrDefault(key, new Enter());
        enter.EnterCount++;
        enterCount[key] = enter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevelUp(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        if (usedKeys.Add(key))
        {
            enterCount[key] = new Enter();

            levelCount[key] = 0;
            spinCount[key] = 0;
            gemCount[key] = 0;
            gemValue[key] = 0;
            coinCount[key] = 0;
            coinValue[key] = 0;
            splitCount[key] = 0;
            spinAdd1SpinCount[key] = 0;

            redcoinCount[key] = 0;
            respinCount[key] = 0;
            respinCoinCount[key] = 0;
            respinCoinValue[key] = 0;
        }

        levelCount[key] = levelCount.GetValueOrDefault(key, 0) + 1;

        //var enter = enterCount.GetValueOrDefault(key, new Enter());
        //enter.EnterCount++;
        //enterCount[key] = enter;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.spinCount[key] = this.spinCount.GetValueOrDefault(key, 0) + 1;

        //enterCount[key].SpinCount = enterCount.GetValueOrDefault(key, new Enter()).SpinCount + 1;
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
    public void AddCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.coinCount[key] = this.coinCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCoinValue(FeatureBonusType bonusType, int initGemCount, int level, double coinValue)
    {
        var key = (bonusType, initGemCount, level);
        this.coinValue[key] = this.coinValue.GetValueOrDefault(key, 0) + coinValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSplitCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.splitCount[key] = this.splitCount.GetValueOrDefault(key, 0) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSpinAdd1SpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        var key = (bonusType, initGemCount, level);
        this.spinAdd1SpinCount[key] = this.spinAdd1SpinCount.GetValueOrDefault(key, 0) + 1;
    }
}