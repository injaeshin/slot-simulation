using LineAndFreeGame.Common;
using LineAndFreeGame.Statistics;

namespace LineAndFreeGame.ThreadStorage;

public class ThreadBuffer
{
    private readonly Random random = new();
    public Random Random => random;

    private SpinStatistics spinStats = new();
    public SpinStatistics SpinStats => spinStats;

    private readonly SymbolType[] symbols = new SymbolType[5 * 3];
    public SymbolType[] Symbols => symbols;

    public int ScatterCount { get; internal set; }
    public bool HasBonus() => ScatterCount >= 3;

    public void Clear()
    {
        Array.Clear(symbols);
        ScatterCount = 0;
    }

    public void StatsClear() => spinStats = new();
}