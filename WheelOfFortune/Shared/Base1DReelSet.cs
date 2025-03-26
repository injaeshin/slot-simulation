using System.Text.Json;

namespace WheelOfFortune.Shared;

public class Base1DReelSet
{
    private readonly List<string[]> rawReelStrips;
    private readonly List<int> rawReelLengths;

    public List<string[]> RawReelStrips => rawReelStrips;
    public int RawReelLengths => rawReelStrips.Count;

    public Base1DReelSet()
    {
        this.rawReelStrips = [];
        this.rawReelLengths = [];
    }

    protected bool ReadReelStrip(GameDataLoader kv, string reelSetName, int reelCount)
    {
        for (int i = 0; i < reelCount; i++)
        {
            if (!kv.TryGetValue($"{reelSetName}{i + 1}", out var rs))
            {
                return false;
            }

            var reelStrip = JsonSerializer.Deserialize<Dictionary<string, string[]>>(rs.ToString()!, JsonOptions.Opt)
                ?? throw new Exception("Invalid BaseReelStrip format");

            InitializeReelStrips(reelStrip);
        }

        return true;
    }

    private void InitializeReelStrips(Dictionary<string, string[]> raw)
    {
        // 릴 순서대로 정렬 (Reel1, Reel2, Reel3...)
        var orderedReels = raw.OrderBy(kv => kv.Key).ToList();

        for (int i = 0; i < orderedReels.Count; i++)
        {
            var reelStrip = orderedReels[i].Value.Where(s => !string.IsNullOrEmpty(s)).Select(s => s).ToArray();
            this.rawReelStrips.Add(reelStrip);
            this.rawReelLengths.Add(reelStrip.Length);
        }
    }

    public (int, string[]) GetRawReelStrip(int idx)
    {
        return (rawReelLengths[idx], rawReelStrips[idx]);
    }
}

public class Base2DReelSet
{
    private readonly List<string[][]> rawReelStrips;
    private readonly List<int[]> rawReelLengths;

    public int RawReelCount => rawReelStrips.Count;
    public int RawReelStripLength(int idx) => rawReelLengths[idx].Length;

    public Base2DReelSet()
    {
        this.rawReelStrips = [];
        this.rawReelLengths = [];
    }

    protected bool ReadReelStrip(int reelCount, GameDataLoader kv)
    {
        for (int i = 0; i < reelCount; i++)
        {
            if (!kv.TryGetValue($"BaseReelStrip{i + 1}", out var brw))
            {
                return false;
            }

            var reelStrip = JsonSerializer.Deserialize<Dictionary<string, string[]>>(brw.ToString()!, JsonOptions.Opt)
                ?? throw new Exception("Invalid BaseReelStrip format");

            InitializeReelStrips(reelStrip);
        }

        return true;
    }

    private void InitializeReelStrips(Dictionary<string, string[]> raw)
    {
        // 릴 순서대로 정렬 (Reel1, Reel2, Reel3...)
        var orderedReels = raw.OrderBy(kv => kv.Key).ToList();

        var reelStrips = new string[orderedReels.Count][];
        var reelLengths = new int[orderedReels.Count];

        for (int i = 0; i < orderedReels.Count; i++)
        {
            reelStrips[i] = orderedReels[i].Value.Select(s => s).ToArray();
            reelLengths[i] = orderedReels[i].Value.Length;
        }

        this.rawReelStrips.Add(reelStrips);
        this.rawReelLengths.Add(reelLengths);
    }

    public (int[], string[][]) GetRawReelStrip(int idx)
    {
        return (rawReelLengths[idx], rawReelStrips[idx]);
    }
}

