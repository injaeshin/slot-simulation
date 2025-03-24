using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

using LineAndFreeGame.Common;
using LineAndFreeGame.Table;
using LineAndFreeGame.ThreadStorage;

namespace LineAndFreeGame.Game;

public class LineGame(ReelStrip reelStrip, PayTable payTable, ILogger<LineGame> log)
{
    private readonly ILogger<LineGame> logger = log;
    private readonly ReelStrip reelStrip = reelStrip;
    private readonly PayTable payTable = payTable;

    public void SimulateSingleSpin(ThreadBuffer buf)
    {
        buf.SpinStats.AddSpinCount();

        Spin(buf);

        if (buf.HasBonus())
        {
            //logger.LogInformation("Bonus");
        }

        Span<SymbolType> middleSymbols = [buf.Symbols[0 * 3 + 1], buf.Symbols[1 * 3 + 1], buf.Symbols[2 * 3 + 1], buf.Symbols[3 * 3 + 1], buf.Symbols[4 * 3 + 1]];
        (var symbol, var count, var pay) = payTable.CalculatePay(middleSymbols);

        //if (symbol == SymbolType.BB && count == 3)
        //{
        //    buf.SpinStats.AddBBSymbol(middleSymbols, pay);
        //}


        buf.SpinStats.AddWinPay(symbol, count, pay);
    }

    private void Spin(ThreadBuffer buffer)
    {
        var random = buffer.Random;
        ReadOnlySpan<int> reelIndex =
        [
            random.Next(reelStrip.ReelLengths[0]),
            random.Next(reelStrip.ReelLengths[1]),
            random.Next(reelStrip.ReelLengths[2]),
            random.Next(reelStrip.ReelLengths[3]),
            random.Next(reelStrip.ReelLengths[4]),
        ];

        ProcessReelColumn(buffer, 0, reelIndex[0], reelStrip.ReelLengths[0], reelStrip.ReelStrips[0]);
        ProcessReelColumn(buffer, 1, reelIndex[1], reelStrip.ReelLengths[1], reelStrip.ReelStrips[1]);
        ProcessReelColumn(buffer, 2, reelIndex[2], reelStrip.ReelLengths[2], reelStrip.ReelStrips[2]);
        ProcessReelColumn(buffer, 3, reelIndex[3], reelStrip.ReelLengths[3], reelStrip.ReelStrips[3]);
        ProcessReelColumn(buffer, 4, reelIndex[4], reelStrip.ReelLengths[4], reelStrip.ReelStrips[4]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessReelColumn(ThreadBuffer buffer, int col, int startIndex, int reelLength, SymbolType[] strip)
    {
        var baseIndex = col * 3;
        ProcessReelRow(buffer, baseIndex, 0, startIndex, reelLength, strip);
        ProcessReelRow(buffer, baseIndex, 1, startIndex + 1, reelLength, strip);
        ProcessReelRow(buffer, baseIndex, 2, startIndex + 2, reelLength, strip);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessReelRow(ThreadBuffer buffer, int baseIndex, int row, int pos, int reelLength, SymbolType[] strip)
    {
        pos = pos >= reelLength ? pos - reelLength : pos;
        var symbol = strip[pos];
        buffer.Symbols[baseIndex + row] = symbol;
        buffer.ScatterCount += symbol == SymbolType.SS ? 1 : 0;
    }

    public void PrintSymbolDistribution() => reelStrip.OutputSymbolDistribution();
}

