using Common;
using Common.Table;
using Microsoft.Extensions.Configuration;
using SpinOfFortune.Shared;

namespace SpinOfFortune.Table;

public interface IReelSet
{
    public List<SymbolType[]> ReelStrips { get; }
    public List<int> ReelLengths { get; }
}

public class ReelSet : Base1DReelSet, IReelSet
{
    private readonly List<SymbolType[]> reelStrips = [];
    private readonly List<int> reelLengths = [];

    public List<SymbolType[]> ReelStrips => reelStrips;
    public List<int> ReelLengths => reelLengths;

    public ReelSet(GameDataLoader kv) : base()
    {
        if (!base.ReadReelStrip(1, kv))
        {
            throw new Exception("Failed to read reel strips");
        }

        var rawReelCount = base.RawReelLengths;
        for (int i = 0; i < rawReelCount; i++)
        {
            var (rawReelLength, rawReelStrip) = base.GetRawReelStrip(i);
            this.reelStrips.Add(rawReelStrip.Select(s => SlotConverter.ToSymbolType(s)).ToArray());
            this.reelLengths.Add(rawReelLength);
        }
    }
}