using LottaCashMummy.Common;
using LottaCashMummy.Statistics.Model;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class FeatureStorage
{
    private const int SEED = 0x10203040;
    private readonly Random symbolRng = new Random(SEED);
    public Random SymbolRng => symbolRng;
    private readonly Random valueRng = new Random(SEED);
    public Random ValueRng => valueRng;

    private FeatureGameStatsModel featureGameStats;
    public FeatureGameStatsModel FeatureGameStats => featureGameStats;

    private readonly MummyState mummy;
    public MummyState Mummy => mummy;

    private int bonusTypeOrder;
    private FeatureBonusType featureBonusType;
    public FeatureBonusType FeatureBonusType => featureBonusType;

    private int initGemCount;
    public int InitGemCount => initGemCount;

    private int remainSpinCount;

    private readonly SymbolPair[] screenArea;
    public ReadOnlySpan<SymbolPair> ScreenArea => screenArea;

    private readonly int[] mummyArea;
    public Span<int> MummyArea => mummyArea;

    private readonly int[] gemIndices;
    public ReadOnlySpan<int> GemIndices => gemIndices;

    private readonly int[] coinIndices;
    public ReadOnlySpan<int> CoinIndices => coinIndices;

    private readonly int[] mummyActiveIndices;
    public Span<int> MummyActiveIndices => mummyActiveIndices;

    public int GemCount { get; private set; }
    public int CoinCount { get; private set; }
    public int MummyAreaCount { get; private set; }

    public FeatureStorage(FeatureGameStatsModel featureGameStats)
    {
        this.featureGameStats = featureGameStats;

        mummy = new MummyState();
        screenArea = new SymbolPair[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        for (int i = 0; i < screenArea.Length; i++)
        {
            screenArea[i] = new SymbolPair();
        }

        mummyArea = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        gemIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        coinIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        mummyActiveIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
    }

    public void Clear()
    {
        ClearAllSymbols();
        ClearMummyArea();
        mummy.Clear();

        initGemCount = 0;
        remainSpinCount = 0;
        MummyAreaCount = 0;

        bonusTypeOrder = 0;
        featureBonusType = FeatureBonusType.None;
    }

    public void StatsClear(FeatureGameStatsModel featureGameStatsModel)
    {
        featureGameStats = featureGameStatsModel;
    }

    public void ClearAllSymbols()
    {
        for (int i = 0; i < screenArea.Length; i++)
        {
            screenArea[i].Clear();
        }

        Array.Clear(gemIndices, 0, gemIndices.Length);
        Array.Clear(coinIndices, 0, coinIndices.Length);
        Array.Clear(mummyActiveIndices, 0, mummyActiveIndices.Length);

        GemCount = 0;
        CoinCount = 0;
    }

    public void ClearSymbolsInMummyArea()
    {
        for (int i = 0; i < mummyArea.Length; i++)
        {
            if (mummyArea[i] > 0)
            {
                screenArea[i].Clear();
            }
        }
    }

    public void ClearMummyArea()
    {
        Array.Clear(mummyArea, 0, mummyArea.Length);
        Array.Clear(mummyActiveIndices, 0, mummyActiveIndices.Length);
        MummyAreaCount = 0;
    }

    #region Create Symbol

    private void CreateBlankSymbol(int idx)
    {
        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
    }

    private void CreateGemSymbol(int idx, FeatureBonusValueType bonusType, double value)
    {
        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Gem, bonusType, value);
        gemIndices[GemCount++] = idx;
    }

    private void CreateRedCoinSymbol(int idx, FeatureBonusValueType bonusType, double value)
    {
        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.RedCoin, bonusType, value);
        coinIndices[CoinCount++] = idx;
    }

    private void CreateCoinSymbol(int idx, FeatureBonusValueType bonusType, double value)
    {
        if (mummyArea[idx] == 0)
        {
            throw new Exception("Coin is not allowed to be created in mummy area");
        }

        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Coin, bonusType, value);
        coinIndices[CoinCount++] = idx;
    }

    #endregion

    #region Add Symbol



    public void AddSymbol(int idx, FeatureSymbolType symbolType, FeatureBonusValueType bonusType, double value)
    {
        var symbol = GetSymbol(idx);
        if (symbol.IsFull())
        {
            throw new Exception("Symbol is full");
        }

        if (symbolType == FeatureSymbolType.Blank)
        {
            CreateBlankSymbol(idx);
            return;
        }

        switch (symbolType)
        {
            case FeatureSymbolType.Coin:
                CreateCoinSymbol(idx, bonusType, value);
                featureGameStats.AddCoinCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.Gem:
                CreateGemSymbol(idx, bonusType, value);
                featureGameStats.AddGemCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.RedCoin:
                CreateRedCoinSymbol(idx, bonusType, value);
                //featureGameLog.AddRedCoinCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            default:
                throw new ArgumentException($"Invalid feature symbol type: {symbolType}");
        }
    }

    public void AddSymbolWithRespin(int idx, FeatureSymbolType symbolType, FeatureBonusValueType bonusType, double value)
    {
        if (!IsActiveMummyArea(idx))
        {
            throw new Exception("Do not add symbol with respin in non-mummy area");
        }

        var symbol = GetSymbol(idx);
        if (symbol.IsFull())
        {
            throw new Exception("Symbol is full with respin");
        }

        if (symbolType == FeatureSymbolType.Blank)
        {
            CreateBlankSymbol(idx);
            return;
        }

        switch (symbolType)
        {
            case FeatureSymbolType.Coin:
                CreateCoinSymbol(idx, bonusType, value);
                //featureGameLog.AddRespinCoinCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.Gem:
                throw new Exception("Gem is not allowed to be respin");
            case FeatureSymbolType.RedCoin:
                throw new Exception("RedCoin is not allowed to be respin");
            default:
                throw new ArgumentException($"Invalid feature symbol type: {symbolType}");
        }
    }

    #endregion

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
    public SymbolPair GetSymbol(int index) => screenArea[index];

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
        featureGameStats.AddSpin(bonusTypeOrder, initGemCount, mummy.Level);

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

    public void CollectSymbol(int idx, FeatureSymbol symbol)
    {
        switch (symbol.Type)
        {
            case FeatureSymbolType.Blank:
                throw new Exception("Blank symbol is not allowed to be collected");
            case FeatureSymbolType.Coin:
                CollectSymbolValue(idx, symbol);
                RemoveCoin(symbol);
                break;
            case FeatureSymbolType.Gem:
                CollectSymbolValue(idx, symbol);
                RemoveGem(symbol);
                break;
            case FeatureSymbolType.RedCoin:
                //featureGameLog.AddRedCoinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            default:
                throw new Exception("Invalid symbol type");
        }

        symbol.Clear();
    }

    private void CollectSymbolValue(int idx, FeatureSymbol symbol)
    {
        if (symbol.Type == FeatureSymbolType.Gem)
        {
            mummy.ObtainGem();
            featureGameStats.AddGemValue(bonusTypeOrder, initGemCount, mummy.Level, symbol.Value);
            return;
        }

        switch (symbol.BonusType)
        {
            case FeatureBonusValueType.PlusSpin:
                AddBonusSpinCount(1);
                //featureGameLog.AddSpinAdd1SpinCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            case FeatureBonusValueType.Pay:
            case FeatureBonusValueType.Grand:
            case FeatureBonusValueType.Mega:
            case FeatureBonusValueType.Major:
            case FeatureBonusValueType.Minor:
            case FeatureBonusValueType.Mini:
                featureGameStats.AddCoinValue(bonusTypeOrder, initGemCount, mummy.Level, symbol.Value);
                break;
            default:
                throw new Exception("Invalid symbol bonus type");
        }
    }

    public void CollectSymbolRespin(int idx, FeatureSymbol symbol)
    {
        switch (symbol.Type)
        {
            case FeatureSymbolType.Coin:
                CollectSymbolValueRespin(idx, symbol);
                RemoveCoin(symbol);
                break;
            default:
                throw new Exception("Invalid symbol type");
        }
    }

    private void CollectSymbolValueRespin(int idx, FeatureSymbol symbol)
    {
        switch (symbol.BonusType)
        {
            case FeatureBonusValueType.PlusSpin:
                AddBonusSpinCount(1);
                //featureGameLog.AddSpinAdd1SpinCount(bonusTypeOrder, initGemCount, mummy.Level);
                break;
            case FeatureBonusValueType.Pay:
            case FeatureBonusValueType.Grand:
            case FeatureBonusValueType.Mega:
            case FeatureBonusValueType.Major:
            case FeatureBonusValueType.Minor:
            case FeatureBonusValueType.Mini:
                //featureGameLog.AddRespinCoinValue(bonusTypeOrder, initGemCount, mummy.Level, symbol.Value);
                break;
            default:
                throw new Exception("Invalid symbol bonus type");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveGem(FeatureSymbol symbol)
    {
        if (symbol.Type != FeatureSymbolType.Gem)
        {
            throw new Exception("Invalid symbol type");
        }

        GemCount--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveCoin(FeatureSymbol symbol)
    {
        if (symbol.Type != FeatureSymbolType.Coin)
        {
            throw new Exception("Invalid symbol type");
        }

        CoinCount--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CollectGemFromBaseGame(double value)
    {
        mummy.ObtainGem();
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

    public void AddGemValueLog(double value)
    {
        featureGameStats.AddGemValue(bonusTypeOrder, initGemCount, mummy.Level, value);
    }
}