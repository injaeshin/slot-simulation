using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class BaseStats
{
    public Dictionary<(byte, int), long> PayData { get; } = new();
    private readonly HashSet<(byte, int)> usedKeys = new();
    public int SpinCount { get; set; }

    public BaseStats()
    {
        SpinCount = 0;
    }

    public void Reset()
    {
        foreach (var key in usedKeys)
        {
            PayData[key] = 0;
        }
        SpinCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSpinCount() => SpinCount++;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddPayWin(byte symbol, int hits, long amount)
    {
        var key = (symbol, hits);
        if (usedKeys.Add(key))
        {
            PayData[key] = 0;
        }
        
        PayData[key] += amount;
    }
}