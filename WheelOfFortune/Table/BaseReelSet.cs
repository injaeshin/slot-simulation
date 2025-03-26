using WheelOfFortune.Shared;

namespace WheelOfFortune.Table;

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

    public ReelSet(GameDataLoader kv, string reelSetName, int reelCount) : base()
    {
        if (!base.ReadReelStrip(kv, reelSetName, reelCount))
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