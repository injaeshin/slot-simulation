using LottaCashMummy.Common;

using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class Feature
{
    private static readonly IEnumerable<FeatureBonusType> BonusTypesRange = Enum.GetValues(typeof(FeatureBonusType))
        .Cast<FeatureBonusType>()
        .Where(x => x != FeatureBonusType.None);
    private static readonly int[] LevelsRange = [1, 2, 3, 4];
    private static readonly int[] InitGemCountsRange = [0, 1, 2, 3, 4, 5];

    // 입장 수
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> enterCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> EnterCount => enterCount;

    // 스핀 카운트
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> spinCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> SpinCount => spinCount;

    // 레드 코인 카운트
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> redCoinCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> RedCoinCount => redCoinCount;

    // 리스핀 카운트
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> respinCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> RespinCount => respinCount;

    // 레벨 업 카운트
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> levelUpCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> LevelUpCount => levelUpCount;

    // 젬 생성 카운트
    private readonly Dictionary<(FeatureBonusType type, int level, int initGemCount), long> createGemCount;
    public Dictionary<(FeatureBonusType type, int level, int initGemCount), long> CreateGemCount => createGemCount;

    public Feature()
    {
        enterCount = new();
        spinCount = new();
        redCoinCount = new();
        respinCount = new();
        levelUpCount = new();
        createGemCount = new();

        foreach (FeatureBonusType bonusType in BonusTypesRange)
        {
            foreach (int level in LevelsRange)
            {
                foreach (int initGemCount in InitGemCountsRange)
                {
                    enterCount[(bonusType, level, initGemCount)] = 0;
                    spinCount[(bonusType, level, initGemCount)] = 0;
                    redCoinCount[(bonusType, level, initGemCount)] = 0;
                    respinCount[(bonusType, level, initGemCount)] = 0;
                    levelUpCount[(bonusType, level, initGemCount)] = 0;
                    createGemCount[(bonusType, level, initGemCount)] = 0;
                }
            }
        }
    }

    public void Clear()
    {
        foreach (var key in enterCount.Keys) enterCount[key] = 0;
        foreach (var key in spinCount.Keys) spinCount[key] = 0;
        foreach (var key in redCoinCount.Keys) redCoinCount[key] = 0;
        foreach (var key in respinCount.Keys) respinCount[key] = 0;
        foreach (var key in levelUpCount.Keys) levelUpCount[key] = 0;
        foreach (var key in createGemCount.Keys) createGemCount[key] = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddEnterCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        enterCount[(bonusType, level, initGemCount)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevelSpinCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        spinCount[(bonusType, level, initGemCount)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRedCoinCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        redCoinCount[(bonusType, level, initGemCount)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRespinCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        respinCount[(bonusType, level, initGemCount)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLevelUpCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        levelUpCount[(bonusType, level, initGemCount)]++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCreateGemCount(FeatureBonusType bonusType, int level, int initGemCount)
    {
        createGemCount[(bonusType, level, initGemCount)]++;
    }
}

// statistics
public class SpinStatistics
{
    private readonly Feature feature;
    public Feature Feature => feature;

    private readonly int[,] winPayTable;
    public int[,] WinPayTable => winPayTable;

    public SpinStatistics()
    {
        winPayTable = new int[SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS];

        feature = new Feature();
    }

    public void Reset()
    {
        Array.Clear(winPayTable, 0, winPayTable.Length);
        feature.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWin(byte symbol, int hits, int amount)
    {
        winPayTable[symbol, hits]++;
    }
}

