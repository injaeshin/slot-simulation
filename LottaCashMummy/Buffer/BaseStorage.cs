using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Buffer;

public class BaseStorage
{
    private static readonly byte[,] symbolIndexLookup;
    private readonly SpinStatistics spinStats;

    private readonly byte[] symbols;        // 심볼 타입
    private readonly byte[] jackpots;       // 잭팟 정보
    private readonly byte[] attributes;     // Gem 속성
    private readonly double[] values;       // Gem 값

    private readonly Symbol[] winGems;
    private readonly byte[] gemIndices;
    private readonly byte[] normalIndices;
    private readonly byte[] mummyPosition;

    private byte normalSymbolCount;
    private byte gemSymbolCount;
    private byte winGemCount;
    private byte mummyCount;
    private byte bonusType;

    static BaseStorage()
    {
        symbolIndexLookup = new byte[SlotConst.BASE_ROWS, SlotConst.BASE_COLS];
        for (int row = 0; row < SlotConst.BASE_ROWS; row++)
        {
            for (int col = 0; col < SlotConst.BASE_COLS; col++)
            {
                symbolIndexLookup[row, col] = (byte)(row * SlotConst.BASE_COLS + col);
            }
        }
    }

    public BaseStorage(SpinStatistics spinStats)
    {
        this.spinStats = spinStats;

        symbols = new byte[SlotConst.TOTAL_POSITIONS];
        normalIndices = new byte[SlotConst.TOTAL_POSITIONS];
        mummyPosition = new byte[SlotConst.MAX_MUMMY_SYMBOL];
        gemIndices = new byte[SlotConst.TOTAL_POSITIONS];
        jackpots = new byte[SlotConst.TOTAL_POSITIONS];
        attributes = new byte[SlotConst.TOTAL_POSITIONS];
        values = new double[SlotConst.TOTAL_POSITIONS];
        
        winGems = new Symbol[SlotConst.MAX_WIN_GEM_SYMBOL];
        for (int i = 0; i < SlotConst.MAX_WIN_GEM_SYMBOL; i++)
        {
            winGems[i] = new Symbol();
        }

        Clear();
    }
    public void Clear()
    {
        Array.Clear(symbols, 0, symbols.Length);
        Array.Clear(jackpots, 0, jackpots.Length);
        Array.Clear(attributes, 0, attributes.Length);
        Array.Clear(values, 0, values.Length);
        Array.Clear(normalIndices, 0, normalIndices.Length);
        Array.Clear(gemIndices, 0, gemIndices.Length);
        Array.Clear(mummyPosition, 0, mummyPosition.Length);
        
        for (int i = 0; i < winGemCount; i++)
        {
            winGems[i].Clear();
        }

        normalSymbolCount = 0;
        gemSymbolCount = 0;
        winGemCount = 0;
        mummyCount = 0;
        bonusType = 0;
    }

    public byte MummyIndex => mummyCount > 0 ? mummyPosition[0] : (byte)0;
    public int NormalCount => normalSymbolCount;
    public int GemCount => gemSymbolCount;
    public bool HasMummySymbol => mummyCount > 0;
    public byte FeatureBonusType => bonusType;
    public int WinGemCount => winGemCount;

    // Getters for external access

    public byte GetSymbol(byte index) => symbols[index];
    public byte GetAttribute(byte index) => attributes[index];
    public double GetValue(byte index) => values[index];
    public byte GetJackpot(byte index) => jackpots[index];
    public byte GetNormalIndex(byte index) => normalIndices[index];
    public byte GetGemIndex(byte index) => gemIndices[index];
    public void SetFeatureBonusType(byte type) => bonusType = type;
    public Symbol GetWinGemSymbol(byte i) => winGems[i];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNormalSymbol(int row, int col, byte sym)
    {
        var index = symbolIndexLookup[row, col];
        symbols[index] = sym;
        normalIndices[normalSymbolCount++] = index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddGemSymbol(int row, int col)
    {
        var index = symbolIndexLookup[row, col];
        symbols[index] = SlotConst.GEM_SYMBOL;
        gemIndices[gemSymbolCount++] = index;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddMummySymbol(int row, int col)
    {
        mummyPosition[0] = symbolIndexLookup[row, col];
        symbols[mummyPosition[0]] = SlotConst.MUMMY_SYMBOL;
        mummyCount = 1;
    }

    public void AddSymbol(int row, int col, byte symbol)
    {
        if (symbol < SlotConst.WILD_SYMBOL)
        {
            AddNormalSymbol(row, col, symbol);
            return;
        }

        switch (symbol)
        {
            case SlotConst.MUMMY_SYMBOL:
                AddMummySymbol(row, col);
                break;
            case SlotConst.GEM_SYMBOL:
                AddGemSymbol(row, col);
                break;
            default:
                AddNormalSymbol(row, col, symbol);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SwapNormalToGemSymbol(byte normalIdx)
    {
        var symbolIndex = normalIndices[normalIdx];
        normalIndices[normalIdx] = normalIndices[--normalSymbolCount];
        symbols[symbolIndex] = SlotConst.GEM_SYMBOL;
        gemIndices[gemSymbolCount++] = symbolIndex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGemValue(byte index, double gemValue, JackpotType jackpotType)
    {
        values[index] = gemValue;
        jackpots[index] = (byte)jackpotType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetGemAttribute(byte index, GemBonusType bonusType) => attributes[index] = (byte)bonusType;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinGemSymbol(byte index)
    {
        var symbolType = (SymbolType)symbols[index];
        var gemBonusType = (GemBonusType)attributes[index];
        var gemValue = values[index];

        winGems[winGemCount].SetSymbol(
            index,
            symbolType,
            gemBonusType,
            gemValue
        );
        
        winGemCount++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddWinAmount(byte symbol, int hits, int amount)
    {
        spinStats.AddBaseWinCount(symbol, hits);
        spinStats.AddBaseWinAmount(amount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetRollNormalSymbol(Random random) => normalIndices[random.Next(normalSymbolCount)];
}