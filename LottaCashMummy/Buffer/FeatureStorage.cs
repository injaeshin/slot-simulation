using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class FeatureStorage
{
    private readonly SpinStatistics spinStats;

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

    private readonly int[] mummyAreaIndices;
    public ReadOnlySpan<int> MummyAreaIndices => mummyAreaIndices;

    public int GemCount { get; private set; }
    public int CoinCount { get; private set; }
    public int MummyAreaCount { get; private set; }

    public FeatureStorage(SpinStatistics spinStats)
    {
        this.spinStats = spinStats;
        mummy = new MummyState();

        screenArea = new SymbolPair[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        for (int i = 0; i < screenArea.Length; i++)
        {
            screenArea[i] = new SymbolPair();
        }

        mummyArea = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        gemIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        coinIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
        mummyAreaIndices = new int[SlotConst.FEATURE_ROWS * SlotConst.FEATURE_COLS];
    }

    public void Clear()
    {
        ClearSymbolInScreenArea();

        mummy.Clear();
        Array.Clear(mummyAreaIndices, 0, mummyAreaIndices.Length);

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
        Array.Clear(mummyAreaIndices, 0, mummyAreaIndices.Length);

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
        Array.Clear(mummyAreaIndices, 0, mummyAreaIndices.Length);
        MummyAreaCount = 0;
    }

    private void AddBlankSymbol(int idx)
    {
        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
    }

    private void AddGemSymbol(int idx, FeatureBonusValueType bonusType, double value, bool isCreate)
    {
        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Gem, bonusType, value);
        gemIndices[GemCount++] = idx;
    }

    private void AddCoinSymbol(int idx, FeatureBonusValueType bonusType, double value)
    {
        if (mummyArea[idx] == 0)
        {
            return;
        }

        var symbol = screenArea[idx];
        symbol.AddSymbol(FeatureSymbolType.Coin, bonusType, value);
        coinIndices[CoinCount++] = idx;
    }

    public void AddSymbol(int idx, FeatureSymbolType symbolType, FeatureBonusValueType bonusType, double value, bool isCreate = true)
    {
        var symbol = GetSymbol(idx);
        if (symbol.IsFull())
        {
            throw new Exception("Symbol is full");
        }

        if (symbolType == FeatureSymbolType.Blank)
        {
            AddBlankSymbol(idx);
            return;
        }

        switch (symbolType)
        {
            case FeatureSymbolType.Coin:
                AddCoinSymbol(idx, bonusType, value);
                //spinStats.GetFeatureStats(FeatureBonusType).IncrementCreateCoinCount(FeatureBonusType, mummy.Level, initGemCount);
                break;
            case FeatureSymbolType.Gem:
                AddGemSymbol(idx, bonusType, value, isCreate);
                if (isCreate)
                {
                    //spinStats.GetFeatureStats(FeatureBonusType).IncrementCreateGemCount(mummy.Level, initGemCount);
                }
                break;
            default:
                throw new ArgumentException($"Invalid feature symbol type: {symbolType}");
        }
    }

    public void SetBonusType(byte featureBonusType) => this.featureBonusType = (FeatureBonusType)featureBonusType;

    public void SetInitGemCount(int cnt) => this.initGemCount = cnt;

    public void AddBonusSpinCount(int spinCount) => remainSpinCount += spinCount;

    public void AddMummyArea(int idx) => mummyAreaIndices[MummyAreaCount++] = idx;

    public SymbolPair GetSymbol(int index) => screenArea[index];

    public bool IsActiveMummyArea(int index) => mummyArea[index] > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetMummyAreaCount() => MummyAreaCount;

    public bool UseSpinCount()
    {
        if (remainSpinCount <= 0)
        {
            return false;
        }

        remainSpinCount--;
        //spinStats.GetFeatureStats(FeatureBonusType).IncrementSpinCount(mummy.Level, initGemCount);

        return true;
    }

    public void FeatureEnter()
    {
        remainSpinCount = SlotConst.FEATURE_SPIN_COUNT;

        //spinStats.GetFeatureStats(FeatureBonusType).IncrementLevelUpCount(mummy.Level, initGemCount);
    }

    //public void AddWinAmount(double amount) => TotalWinAmount += amount;

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

        //spinStats.GetFeatureStats(FeatureBonusType).IncrementLevelUpCount(mummy.Level, initGemCount);

        return true;
    }

    public bool CollectSymbolValue(int idx, FeatureSymbol symbol)
    {
        bool hasRedCoin = false;
        switch (symbol.BonusType)
        {
            case FeatureBonusValueType.RedCoin:
                hasRedCoin = true;
                //spinStats.GetFeatureStats(FeatureBonusType).IncrementRedCoinCount(mummy.Level, initGemCount);
                break;
            case FeatureBonusValueType.Spin:
                AddBonusSpinCount(1);
                //spinStats.GetFeatureStats(FeatureBonusType).IncrementFreeSpinCoinCount(mummy.Level, initGemCount);
                break;
            default: // Coin or Jackpot
                //AddWinAmount(symbol.Value);
                spinStats.AddFeatureWinAmount(symbol.Value);
                //if (symbol.Type == FeatureSymbolType.Gem)
                    //spinStats.GetFeatureStats(FeatureBonusType).IncrementObtainGemValue(mummy.Level, initGemCount, symbol.Value);
                //else
                    //spinStats.GetFeatureStats(FeatureBonusType).IncrementObtainCoinValue(FeatureBonusType, mummy.Level, initGemCount, symbol.Value);
                break;
        }

        return hasRedCoin;
    }

    public void CollectSymbol(int idx, FeatureSymbol symbol)
    {
        if (symbol.Type == FeatureSymbolType.Coin)
        {
            CoinCount--;
        }
        else if (symbol.Type == FeatureSymbolType.Gem)
        {
            mummy.CollectGem();
            GemCount--;
        }

        symbol.Clear();
    }

    public void AddRespinCount()
    {
        //spinStats.GetFeatureStats(FeatureBonusType).IncrementRespinCount(mummy.Level, initGemCount);
    }
}