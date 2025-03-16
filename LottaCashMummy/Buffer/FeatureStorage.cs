using LottaCashMummy.Common;
using LottaCashMummy.Statistics.Model;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class FeatureStorage
{
    private readonly Random random;
    public Random Random => random;

    private FeatureGameStatsModel featureGameStats;

    private FeatureSingleSpinResult spinResult;
    public FeatureSingleSpinResult SpinResult => spinResult;

    private readonly MummyState mummy;
    public MummyState Mummy => mummy;

    private int bonusTypeOrder;
    private FeatureBonusType featureBonusType;
    public FeatureBonusType FeatureBonusType => featureBonusType;

    private int initGemCount;

    private double totalValue;
    private int remainSpinCount;

    private readonly int[] mummyArea;
    public Span<int> MummyArea => mummyArea;

    private readonly int[] mummyActiveIndices;
    public Span<int> MummyActiveIndices => mummyActiveIndices;

    public int MummyAreaCount { get; private set; }

    public FeatureStorage(FeatureGameStatsModel featureGameStats, Random random)
    {
        this.random = random;
        this.featureGameStats = featureGameStats;
        this.spinResult = new FeatureSingleSpinResult();

        mummy = new MummyState();
        mummyArea = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        mummyActiveIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
    }

    public void Clear()
    {
        ClearMummyArea();

        spinResult.Clear();
        mummy.Clear();

        totalValue = 0;
        initGemCount = 0;
        remainSpinCount = 0;
        bonusTypeOrder = 0;
        featureBonusType = FeatureBonusType.None;
    }

    public void StatsClear(FeatureGameStatsModel featureGameStatsModel)
    {
        featureGameStats = featureGameStatsModel;
    }

    public void ClearMummyArea()
    {
        Array.Clear(mummyArea, 0, mummyArea.Length);
        Array.Clear(mummyActiveIndices, 0, mummyActiveIndices.Length);
        MummyAreaCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBonusType(FeatureBonusType featureBonusType)
    {
        this.featureBonusType = featureBonusType;
        this.bonusTypeOrder = BonusTypeConverter.GetBonusTypeOrder(featureBonusType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetInitGemCount(int cnt) => this.initGemCount = cnt;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBonusSpinCount(int spinCount) => remainSpinCount += spinCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsActiveMummyArea(int index) => mummyArea[index] == 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool UseSpinCount()
    {
        if (remainSpinCount <= 0)
        {
            return false;
        }

        remainSpinCount--;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enter()
    {
        remainSpinCount = SlotConst.FEATURE_SPIN_COUNT;
        featureGameStats.AddLevel(bonusTypeOrder, initGemCount, mummy.Level);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InitMummy(int centerIndex, int area, int level, int reqGem)
    {
        mummy.Init(centerIndex, area, level, reqGem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MummyLevelUp(int newArea, int nextGemsToLevel, int spin)
    {
        if (!mummy.LevelUp(newArea, nextGemsToLevel, out int remainGemCount))
        {
            throw new Exception("Level Up failed");
        }

        AddBonusSpinCount(spin);
        featureGameStats.AddLevel(bonusTypeOrder, initGemCount, mummy.Level);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CollectGemValue(double value)
    {
        mummy.ObtainGem(1);
        totalValue += value;
        // didn't add value to stats.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UseRespin()
    {
        //featureGameLog.AddRespinCount(bonusTypeOrder, initGemCount, mummy.Level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSplit()
    {
        //featureGameLog.AddSplitCount(bonusTypeOrder, initGemCount, mummy.Level);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddMummyActiveArea(int index)
    {
        if (mummyArea[index] == 1)
        {
            throw new Exception("Mummy active area already exists");
        }

        mummyArea[index] = 1;
        mummyActiveIndices[MummyAreaCount++] = index;
    }

    public void AddStatsAddGemCountAndValue()
    {
        featureGameStats.AddGemSpinCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.GemSpinCount);
        featureGameStats.AddGemCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.GemCount);
        featureGameStats.AddGemValue(bonusTypeOrder, initGemCount, mummy.Level, spinResult.GemValue);
    }

    public void AddStatsAddCoinCountAndValue()
    {
        featureGameStats.AddCoinSpinCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.CoinSpinCount);
        featureGameStats.AddCoinCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.CoinCount);
        featureGameStats.AddCoinValue(bonusTypeOrder, initGemCount, mummy.Level, spinResult.CoinValue);
    }

    public void AddStatsAddRedCoinCount()
    {
        if (spinResult.HasRedCoin)
            featureGameStats.AddRedCoinCount(bonusTypeOrder, initGemCount, mummy.Level);
    }

    public void AddGemSpinCount()
    {
        featureGameStats.AddGemSpinCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.GemSpinCount);
    }

    public void AddCoinSpinCount()
    {
        featureGameStats.AddCoinSpinCount(bonusTypeOrder, initGemCount, mummy.Level, spinResult.CoinSpinCount);
    }
}