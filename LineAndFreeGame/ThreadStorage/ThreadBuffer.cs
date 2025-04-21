using LineAndFree.Shared;
using LineAndFree.Statistics;

namespace LineAndFree.ThreadStorage;

public class ThreadBuffer
{
    private readonly Random random = new();
    public Random Random => random;

    private SpinStatistics spinStats = new();
    public SpinStatistics SpinStats => spinStats;

    private readonly SymbolType[] baseGameSymbols = new SymbolType[5 * 3];
    public SymbolType[] LineGameSymbols => baseGameSymbols;

    private readonly SymbolType[] freeGameSymbols = new SymbolType[5 * 3];
    public SymbolType[] FreeGameSymbols => freeGameSymbols;

    public int GetScatterCount()
    {
        var scatterCount = 0;
        for (int i = 0; i < baseGameSymbols.Length; i++)
        {
            if (baseGameSymbols[i] == SymbolType.SS)
            {
                scatterCount++;
            }
        }

        return scatterCount;
    }

    public int GetFreeScatterCount()
    {
        var scatterCount = 0;
        for (int i = 0; i < freeGameSymbols.Length; i++)
        {
            if (freeGameSymbols[i] == SymbolType.SS)
            {
                scatterCount++;
            }
        }
        return scatterCount;
    }

    public void BaseGameClear()
    {
        Array.Clear(baseGameSymbols);
    }

    public void FreeGameClear()
    {
        Array.Clear(freeGameSymbols);
    }

    public void StatsClear() => spinStats = new();
}