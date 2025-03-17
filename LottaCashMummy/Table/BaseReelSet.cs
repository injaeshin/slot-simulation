using Common;
using Common.Table;
using LottaCashMummy.Shared;

namespace LottaCashMummy.Table;

public interface IReelSet
{
    int BonusReelStrip { get; }
    int NormalReelStrip { get; }

    (int[], SymbolType[][]) GetReelStrip(int idx);
}

public class ReelSet : Base1DReelSet, IReelSet
{
    public int BonusReelStrip { get; private set; } = 1;
    public int NormalReelStrip { get; private set; } = 0;

    private List<SymbolType[][]> reelStrips = [];
    private List<int[]> reelLengths = [];

    public ReelSet(GameDataLoader kv) : base()
    {
        var reelCount = 2;
        if (!base.ReadReelStrip(reelCount, kv))
        {
            throw new Exception("Failed to read reel strips");
        }

        //AddReelStrip();
    }

    // private void AddReelStrip()
    // {
    //     var rawReelCount = base.RawReelCount;
    //     this.reelStrips = new List<SymbolType[][]>(rawReelCount);
    //     this.reelLengths = new List<int[]>(rawReelCount);

    //     for (int i = 0; i < rawReelCount; i++)
    //     {
    //         var (rawReelLength, rawReelStrip) = base.GetRawReelStrip(i);
    //         var rs = new SymbolType[rawReelLength.Length][];
    //         var rl = new int[rawReelLength.Length];

    //         for (int j = 0; j < rawReelLength.Length; j++)
    //         {
    //             rl[j] = rawReelLength[j];
    //             //rs[j] = rawReelStrip[j].Select(s => SlotConverter.ToSymbolType(s)).ToArray();
    //         }

    //         this.reelLengths[i] = rl;
    //         this.reelStrips[i] = rs;
    //     }
    // }

    public (int[], SymbolType[][]) GetReelStrip(int idx)
    {
        return (reelLengths[idx], reelStrips[idx]);
    }
}

