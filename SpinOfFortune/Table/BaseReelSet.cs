using Common;
using SpinOfFortune.Shared;
using System.ComponentModel;
using System.Text.Json;

namespace SpinOfFortune.Table;

public interface IBaseReelSet
{
    int ReelStrip { get; }
    (int[], SymbolType[][]) GetReelStrip(int idx);
}

public class BaseReelSet : IBaseReelSet
{
    public int ReelStrip { get; private set; } = 0;

    private readonly List<SymbolType[][]> baseReelStrips;
    private readonly List<int[]> baseReelStriptLengths;

    public BaseReelSet(GameDataLoader kv)
    {
        this.baseReelStrips = new List<SymbolType[][]>();
        this.baseReelStriptLengths = new List<int[]>();

        if (!kv.TryGetValue("BaseReelStrip", out var brw))
        {
            throw new Exception("BaseReelStrip not found in json object");
        }

        var baseReelStrip = JsonSerializer.Deserialize<Dictionary<string, string[]>>(brw.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid BaseReelStrip format");

        InitializeReelStrips(baseReelStrip);
    }

    private void InitializeReelStrips(Dictionary<string, string[]> rs)
    {
        var orderedReels = rs.OrderBy(kv => kv.Key).ToList();

        SymbolType[][] reelStrip = new SymbolType[rs.Count][];
        int[] reelLengths = new int[rs.Count];

        for (int i = 0; i < orderedReels.Count; i++)
        {
            reelStrip[i] = orderedReels[i].Value.Select(s => SlotConverter.ToSymbolType(s)).ToArray();
            reelLengths[i] = reelStrip[i].Length;
        }

        baseReelStrips.Add(reelStrip);
        baseReelStriptLengths.Add(reelLengths);
    }

    public (int[], SymbolType[][]) GetReelStrip(int idx)
    {
        return (baseReelStriptLengths[idx], baseReelStrips[idx]);
    }
}

