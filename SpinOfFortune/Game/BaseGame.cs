
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

using SpinOfFortune.Shared;
using SpinOfFortune.Table;
using SpinOfFortune.ThreadBuffer;

namespace SpinOfFortune.Game;

public class BaseGame(ReelSet reelSet)
{
    private readonly PayTable payTable = new();
    private readonly ReelSet reelSet = reelSet;

    public bool Spin(ThreadStorage ts)
    {
        var bs = ts.BaseStorage;
        var random = bs.Random;

        ReadOnlySpan<int> reelIndex =
        [
            random.Next(reelSet.ReelLengths[0]),
            random.Next(reelSet.ReelLengths[1]),
            random.Next(reelSet.ReelLengths[2]),
        ];

        bs.AddSpinCount();

        bs.Symbols[0] = GetMiddleSymbolInColumn(reelIndex[0], reelSet.ReelLengths[0], reelSet.ReelStrips[0]);
        bs.Symbols[1] = GetMiddleSymbolInColumn(reelIndex[1], reelSet.ReelLengths[1], reelSet.ReelStrips[1]);
        bs.Symbols[2] = GetMiddleSymbolInColumn(reelIndex[2], reelSet.ReelLengths[2], reelSet.ReelStrips[2]);

        var (pay, combiType) = payTable.GetPay(bs.Symbols);
        bs.Statistics.AddWinPay(combiType, pay);

        return pay > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SymbolType GetMiddleSymbolInColumn(int startIndex, int reelLength, ReadOnlySpan<SymbolType> strip)
    {
        var idx = startIndex + 1;
        var pos = idx >= reelLength ? idx - reelLength : idx;
        return strip[pos];
    }
}
