using LineAndFreeGame.Common;

namespace LineAndFreeGame.Statistics;

public class SpinStatistics
{
    private long totalLineSpinCount;
    public long TotalBaseSpinCount => totalLineSpinCount;

    private readonly Dictionary<(SymbolType, int), int> lineGameWinPays = [];
    public Dictionary<(SymbolType, int), int> LineGameWinPays => lineGameWinPays;

    private byte lineScatter3Count;
    private byte lineScatter4Count;
    private byte lineScatter5Count;

    public byte LineScatter3Count => lineScatter3Count;
    public byte LineScatter4Count => lineScatter4Count;
    public byte LineScatter5Count => lineScatter5Count;

    private long totalFreeSpinTriggerCount;
    public long TotalFreeSpinTriggerCount => totalFreeSpinTriggerCount;

    private readonly Dictionary<(SymbolType, int), int> totalFreeGameWinPays = [];
    public Dictionary<(SymbolType, int), int> TotalFreeGameWinPays => totalFreeGameWinPays;

    private readonly List<(byte, int)> totalFreeSpinExecutions = [];
    public List<(byte, int)> TotalFreeSpinExecutions => totalFreeSpinExecutions;

    private long totalFreeSpinCount;
    public long TotalFreeSpinCount => totalFreeSpinCount;

    public void AddLineSpinCount()
    {
        totalLineSpinCount++;
    }

    public void AddLineGameWinPay(SymbolType symbol, int count, int winPay)
    {
        lineGameWinPays[(symbol, count)] = lineGameWinPays.GetValueOrDefault((symbol, count), 0) + winPay;
    }

    public void AddLineScatterCount(int count)
    {
        switch (count)
        {
            case 3:
                lineScatter3Count++;
                break;
            case 4:
                lineScatter4Count++;
                break;
            case 5:
                lineScatter5Count++;
                break;
        }
    }

    public void AddFreeSpinTriggerCount()
    {
        totalFreeSpinTriggerCount++;
    }

    public void AddFreeSpinCount()
    {
        totalFreeSpinCount++;
    }

    public void AddFreeGameWinPay(SymbolType symbol, int count, int winPay)
    {
        totalFreeGameWinPays[(symbol, count)] = totalFreeGameWinPays.GetValueOrDefault((symbol, count), 0) + winPay;
    }

    public void AddExecFreeSpinCount(int initFreeSpin, int execSpinCount)
    {
        totalFreeSpinExecutions.Add(((byte)initFreeSpin, execSpinCount));
    }
}