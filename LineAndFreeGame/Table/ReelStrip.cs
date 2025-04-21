
using LineAndFree.Shared;

namespace LineAndFree.Table;

public class ReelStrip : Base1DReelSet
{
    private readonly List<SymbolType[]> reelStrips = [];
    private readonly List<int> reelLengths = [];

    public List<SymbolType[]> ReelStrips => reelStrips;
    public List<int> ReelLengths => reelLengths;

    public ReelStrip(GameDataLoader kv, string reelSetName)
    {
        if (!base.ReadReelStrip(kv, reelSetName, 1))
        {
            throw new Exception("Failed to read reel strips");
        }

        var rawReelCount = base.RawReelLengths;
        for (int i = 0; i < rawReelCount; i++)
        {
            var (rawReelLength, rawReelStrip) = base.GetRawReelStrip(i);
            this.reelStrips.Add(rawReelStrip.Where(s => !string.IsNullOrEmpty(s)).Select(s => SlotConverter.ToSymbolType(s)).ToArray());
            this.reelLengths.Add(rawReelLength);
        }
    }
}

