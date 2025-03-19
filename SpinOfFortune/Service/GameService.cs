using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

using Common;
using SpinOfFortune.Shared;
using SpinOfFortune.Table;
using SpinOfFortune.ThreadBuffer;

namespace SpinOfFortune.Service;

public class GameService
{
    private readonly PayTable payTable = new();
    private readonly ReelSet reelSet;

    private readonly BonusValue[] bonusValues;
    private readonly int totalWeights;

    public GameService(IConfiguration conf)
    {
        var filePath = conf.GetSection("file").Value ?? throw new Exception("Reel strip path not found in configuration");
        var kv = GameDataLoader.Read(filePath) ?? throw new Exception("Failed to load reel strip");

        this.reelSet = new ReelSet(kv);
        (this.bonusValues, this.totalWeights) = BonusValueModelParser.ReadSymbolValues("BonusValue", kv);
    }

    public List<SymbolType[]> GetRawReelStrip()
    {
        return reelSet.ReelStrips;
    }

    public void GetPayTableCombination()
    {
        payTable.OutputRules();
    }

    public void SimulateSingleSpin(ThreadStorage ts)
    {
        ts.SpinStats.AddSpinCount();

        Spin(ts);

        if (ts.HasBonus())
        {
            var random = ts.Random;
            var bonusValue = bonusValues[random.Next(totalWeights)];
            ts.SpinStats.AddBonusPay(bonusValue.Value);
        }

        Span<SymbolType> middleSymbols = [ts.Symbols[0 * 3 + 1], ts.Symbols[1 * 3 + 1], ts.Symbols[2 * 3 + 1]];
        var (pay, combiType) = payTable.GetPay(middleSymbols);
        ts.SpinStats.AddWinPay(combiType, pay);
    }

    private void Spin(ThreadStorage ts)
    {
        var random = ts.Random;
        ReadOnlySpan<int> reelIndex =
        [
            random.Next(reelSet.ReelLengths[0]),
            random.Next(reelSet.ReelLengths[1]),
            random.Next(reelSet.ReelLengths[2]),
        ];

        ProcessReelColumn(ts, 0, reelIndex[0], reelSet.ReelLengths[0], reelSet.ReelStrips[0]);
        ProcessReelColumn(ts, 1, reelIndex[1], reelSet.ReelLengths[1], reelSet.ReelStrips[1]);
        ProcessReelColumn(ts, 2, reelIndex[2], reelSet.ReelLengths[2], reelSet.ReelStrips[2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessReelColumn(ThreadStorage ts, int col, int startIndex, int reelLength, SymbolType[] strip)
    {
        ProcessReelRow(ts, col, 0, startIndex, reelLength, strip);
        ProcessReelRow(ts, col, 1, startIndex + 1, reelLength, strip);
        ProcessReelRow(ts, col, 2, startIndex + 2, reelLength, strip);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessReelRow(ThreadStorage ts, int col, int row, int pos, int reelLength, SymbolType[] strip)
    {
        pos = pos >= reelLength ? pos - reelLength : pos;
        var symbol = strip[pos];
        ts.Symbols[col * 3 + row] = symbol;

        if (symbol == SymbolType.Bonus)
        {
            ts.BonusCount++;
        }
    }
}
