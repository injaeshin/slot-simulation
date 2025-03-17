using SpinOfFortune.Shared;
using SpinOfFortune.Statistics;

namespace SpinOfFortune.ThreadBuffer;

public class ThreadStorage
{
    private readonly Random random = new();
    public Random Random => random;

    private SpinStatistics spinStats = new();
    public SpinStatistics SpinStats => spinStats;

    private readonly SymbolType[] symbols = new SymbolType[3 * 3];
    public SymbolType[] Symbols => symbols;

    public int BonusCount { get; internal set; }

    public bool HasBonus() => BonusCount >= 3;

    public void Clear()
    {
        Array.Clear(symbols);
        BonusCount = 0;
    }

    public void StatsClear() => spinStats = new();
}

