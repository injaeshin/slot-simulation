using MoMoMummy.ThreadStorage;
using MoMoMummy.Shared;
using MoMoMummy.Table;
using System.Runtime.CompilerServices;

namespace MoMoMummy.Game;

public class BaseSpin
{
    private readonly IBaseData baseData;

    public BaseSpin(IBaseData baseData)
    {
        this.baseData = baseData;
    }

    public bool Spin(BaseStorage bs, Random random, FeatureBonusTrigger featureTrigger)
    {
        var reelNo = featureTrigger.Type > FeatureBonusType.None ? baseData.BaseReelSet.BonusReelStrip : baseData.BaseReelSet.NormalReelStrip;
        (var reelLengths, var reelstrips) = baseData.BaseReelSet.GetReelStrip(reelNo);
        Span<int> reelIndex =
        [
            random.Next(reelLengths[0]),
            random.Next(reelLengths[1]),
            random.Next(reelLengths[2]),
            random.Next(reelLengths[3]),
            random.Next(reelLengths[4]),
        ];

        // ProcessReelColumn(bs, 0, reelIndex[0], reelLengths[0], reelstrips[0]);
        // ProcessReelColumn(bs, 1, reelIndex[1], reelLengths[1], reelstrips[1]);
        // ProcessReelColumn(bs, 2, reelIndex[2], reelLengths[2], reelstrips[2]);
        // ProcessReelColumn(bs, 3, reelIndex[3], reelLengths[3], reelstrips[3]);
        // ProcessReelColumn(bs, 4, reelIndex[4], reelLengths[4], reelstrips[4]);

        return bs.NormalCount > 0 || bs.GemCount > 0;
    }

    private static void ProcessReelColumn(BaseStorage bs, int col, int startIndex, int reelLength, byte[] strip)
    {
        ProcessReelRow(bs, col, 0, startIndex, reelLength, strip);
        ProcessReelRow(bs, col, 1, startIndex + 1, reelLength, strip);
        ProcessReelRow(bs, col, 2, startIndex + 2, reelLength, strip);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ProcessReelRow(BaseStorage bs, int col, int row, int pos, int reelLength, byte[] strip)
    {
        pos = pos >= reelLength ? pos - reelLength : pos;
        bs.AddSymbol(row, col, strip[pos]);
    }
}