using System.Runtime.CompilerServices;
using LineAndFree.Shared;
using LineAndFree.Table;
using LineAndFree.ThreadStorage;

namespace LineAndFree.Game;

public class FreeGame
{
    private readonly PayTable payTable;
    private readonly ReelStrip reelStrip;

    public FreeGame(GameDataLoader kv, PayTable payTable)
    {
        this.payTable = payTable;
        this.reelStrip = new ReelStrip(kv, "FreeGameReel_FreeReelStrip");
    }

    public async Task ExecuteAsync(ThreadBuffer buf, int initScatterCount, int initFreeSpin)
    {
        var execSpinCount = 0;
        var remainSpinCount = initFreeSpin;
        while (remainSpinCount > 0)
        {
            if (execSpinCount > 500)
            {
                throw new Exception("Spin count is too high");
            }

            execSpinCount++;
            remainSpinCount--;

            // 프리 게임의 스핀 카운트 증가 
            buf.SpinStats.IncrementSpinCountByScatter(initScatterCount);

            Spin(buf);

            var scatterCount = buf.GetFreeScatterCount();
            if (scatterCount >= 3)
            {
                // 프리 게임의 스캐터 승리 횟수 기록
                buf.SpinStats.RecordFreeGameScatterWin(scatterCount);
                buf.SpinStats.RecordFreeGameRetrigger(initScatterCount, scatterCount);
                remainSpinCount += payTable.GetFreeSpinCount(scatterCount);
            }

            var (symbol, count, pay) = CalculateMiddleLinePay(buf);
            if (pay > 0)
            {
                // 프리 게임의 심볼 승리 금액 기록
                buf.SpinStats.RecordFreeGameSymbolWin(symbol, count, pay);
            }
        }

        await Task.CompletedTask;
    }

    //public async Task SimulateSingleSpin(ThreadBuffer buf, int initFreeSpin = 1)
    //{
    //    var execSpinCount = 0;
    //    var remainSpinCount = initFreeSpin;
    //    while (remainSpinCount > 0)
    //    {
    //        if (execSpinCount > 500)
    //        {
    //            throw new Exception("Spin count is too high");
    //        }
    //        execSpinCount++;
    //        remainSpinCount--;
    //        // 프리 게임의 스핀 카운트 증가 
    //        buf.SpinStats.IncrementSpinCountByScatter(1);
    //        Spin(buf);
    //        var scatterCount = buf.GetFreeScatterCount();
    //        if (scatterCount >= 3)
    //        {
    //            // 프리 게임의 스캐터 승리 횟수 기록
    //            buf.SpinStats.RecordFreeGameScatterWin(scatterCount);
    //            buf.SpinStats.RecordFreeGameRetrigger(1, scatterCount);
    //            remainSpinCount += payTable.GetFreeSpinCount(scatterCount);
    //        }
    //        var (symbol, count, pay) = CalculateMiddleLinePay(buf);
    //        if (pay > 0)
    //        {
    //            // 프리 게임의 심볼 승리 금액 기록
    //            buf.SpinStats.RecordFreeGameSymbolWin(symbol, count, pay);
    //        }
    //    }
    //    await Task.CompletedTask;
    //}

    private (SymbolType symbol, int count, int pay) CalculateMiddleLinePay(ThreadBuffer buf)
    {
        Span<SymbolType> middleSymbols = [
            buf.FreeGameSymbols[0 * 3 + 1], buf.FreeGameSymbols[1 * 3 + 1],
            buf.FreeGameSymbols[2 * 3 + 1], buf.FreeGameSymbols[3 * 3 + 1],
            buf.FreeGameSymbols[4 * 3 + 1]
        ];
        return payTable.CalculatePay(middleSymbols);
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
        buffer.FreeGameSymbols[baseIndex + row] = symbol;
    }

    public void PrintReelStrip()
    {
        reelStrip.OutputReelStrip();
    }

    public void PrintSymbolDistribution()
    {
        Console.WriteLine("Symbol distribution: Free Game");
        reelStrip.OutputSymbolDistribution();
    }
}

