using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class FeatureStorage
{
    private const int SEED = 0x10203040;
    private readonly Random symbolRng = new Random(SEED);
    public Random SymbolRng => symbolRng;
    private readonly Random valueRng = new Random(SEED);
    public Random ValueRng => valueRng;

    private readonly FeatureStats featureStats;
    public FeatureStats FeatureStats => featureStats;

    private readonly MummyState mummy;
    public MummyState Mummy => mummy;

    private FeatureBonusType featureBonusType;
    public FeatureBonusType FeatureBonusType => featureBonusType;

    private int initGemCount;
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

    public FeatureStorage(FeatureStats featureStats)
    {
        this.featureStats = featureStats;

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
        ClearSymbolInScreenArea();
        ClearMummyAreaIndices();
        mummy.Clear();

        initGemCount = 0;
        remainSpinCount = 0;
        MummyAreaCount = 0;
        featureBonusType = FeatureBonusType.None;
    }

    public void ClearSymbolInScreenArea()
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

    public void ClearSymbolInMummyArea()
    {
        for (int i = 0; i < mummyArea.Length; i++)
        {
            if (mummyArea[i] > 0)
            {
                screenArea[i].Clear();
            }
        }
    }

    public void ClearMummyAreaIndices()
    {
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
                featureStats.AddCoinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.Gem:
                CreateGemSymbol(idx, bonusType, value);
                featureStats.AddGemCount(featureBonusType, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.RedCoin:
                CreateRedCoinSymbol(idx, bonusType, value);
                featureStats.AddRedCoinCount(featureBonusType, initGemCount, mummy.Level);
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
                featureStats.AddRespinCoinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            case FeatureSymbolType.Gem:
                throw new Exception("Gem is not allowed to be respin");
            case FeatureSymbolType.RedCoin:
                CreateRedCoinSymbol(idx, bonusType, value);
                featureStats.AddRedCoinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            default:
                throw new ArgumentException($"Invalid feature symbol type: {symbolType}");
        }
    }

    #endregion

    public void CollectGemSymbolFromBaseGame(FeatureSymbolType symbolType, FeatureBonusValueType bonusType, double value)
    {
        if (symbolType != FeatureSymbolType.Gem)
        {
            throw new Exception("Invalid symbol type");
        }

        // 젬 획득!
    }

    public void SetBonusType(FeatureBonusType featureBonusType) => this.featureBonusType = featureBonusType;

    public void SetInitGemCount(int cnt) => this.initGemCount = cnt;

    public void AddBonusSpinCount(int spinCount) => remainSpinCount += spinCount;

    public void AddMummyActiveArea(int idx) => mummyActiveIndices[MummyAreaCount++] = idx;

    public SymbolPair GetSymbol(int index) => screenArea[index];

    public bool IsActiveMummyArea(int index) => mummyArea[index] == 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMummyAreaCount() => MummyAreaCount;

    public bool UseSpinCount()
    {
        if (remainSpinCount <= 0)
        {
            return false;
        }

        remainSpinCount--;
        featureStats.AddSpinCount(featureBonusType, initGemCount, mummy.Level);

        return true;
    }

    public void Enter()
    {
        remainSpinCount = SlotConst.FEATURE_SPIN_COUNT;
        featureStats.AddLevel(featureBonusType, initGemCount, mummy.Level);
    }

    public void InitMummy(int centerIndex, int area, int level, int reqGem)
    {
        mummy.Init(centerIndex, area, level, reqGem);
    }

    public bool MummyLevelUp(int newArea, int nextGemsToLevel)
    {
        if (!mummy.LevelUp(newArea, nextGemsToLevel, out int remainGemCount))
        {
            throw new Exception("Level Up failed");
        }

        featureStats.AddLevel(featureBonusType, initGemCount, mummy.Level);

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
                mummy.ObtainGem();
                RemoveGem(symbol);
                break;
            case FeatureSymbolType.RedCoin:
                featureStats.AddRedCoinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            default:
                throw new Exception("Invalid symbol type");
        }

        symbol.Clear();
    }

    private void CollectSymbolValue(int idx, FeatureSymbol symbol)
    {
        switch (symbol.BonusType)
        {
            case FeatureBonusValueType.PlusSpin:
                AddBonusSpinCount(1);
                featureStats.AddSpinAdd1SpinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            case FeatureBonusValueType.Pay:
                featureStats.AddCoinValue(featureBonusType, initGemCount, mummy.Level, symbol.Value);
                break;
            default:
                break;
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
                featureStats.AddSpinAdd1SpinCount(featureBonusType, initGemCount, mummy.Level);
                break;
            case FeatureBonusValueType.Pay:
                featureStats.AddRespinCoinValue(featureBonusType, initGemCount, mummy.Level, symbol.Value);
                break;
            default:
                break;
        }
    }

    private void RemoveGem(FeatureSymbol symbol)
    {
        if (symbol.Type != FeatureSymbolType.Gem)
        {
            throw new Exception("Invalid symbol type");
        }

        GemCount--;
    }

    private void RemoveCoin(FeatureSymbol symbol)
    {
        if (symbol.Type != FeatureSymbolType.Coin)
        {
            throw new Exception("Invalid symbol type");
        }

        CoinCount--;
    }

    public void CollectGemFromBaseGame(double value)
    {
        mummy.ObtainGem();

        // didn't add value to stats.
    }

    public void UseRespin()
    {
        featureStats.AddRespinCount(featureBonusType, initGemCount, mummy.Level);
    }
}