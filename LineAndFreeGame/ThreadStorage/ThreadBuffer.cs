using LineAndFreeGame.Common;
using LineAndFreeGame.Statistics;

namespace LineAndFreeGame.ThreadStorage;

public class ThreadBuffer
{
    private readonly Random random = new();
    public Random Random => random;

    private SpinStatistics spinStats = new();
    public SpinStatistics SpinStats => spinStats;

    private readonly SymbolType[] lineGameSymbols = new SymbolType[5 * 3];
    public SymbolType[] LineGameSymbols => lineGameSymbols;

    private readonly SymbolType[] freeGameSymbols = new SymbolType[5 * 3];
    public SymbolType[] FreeGameSymbols => freeGameSymbols;

    public int GetLineScatterCount()
    {
        var scatterCount = 0;
        for (int i = 0; i < lineGameSymbols.Length; i++)
        {
            if (lineGameSymbols[i] == SymbolType.SS)
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

    public void LineGameClear()
    {
        Array.Clear(lineGameSymbols);
    }
    
    public void FreeClear()
    {
        Array.Clear(freeGameSymbols);
    }

    public void StatsClear() => spinStats = new();
}