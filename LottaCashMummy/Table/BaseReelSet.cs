using Common;
using LottaCashMummy.Shared;
using System.Text.Json;

namespace LottaCashMummy.Table;

public interface IBaseReelSet
{
    int BonusReelStrip { get; }
    int NormalReelStrip { get; }
    (byte[], byte[][]) GetReelStrip(int idx);
}

public class BaseReelSet : IBaseReelSet
{
    public int BonusReelStrip { get; private set; } = 1;
    public int NormalReelStrip { get; private set; } = 0;

    private readonly List<byte[][]> baseReelStrips;
    private readonly List<byte[]> baseReelStriptLengths;

    public BaseReelSet(GameDataLoader kv)
    {
        this.baseReelStrips = new List<byte[][]>();
        this.baseReelStriptLengths = new List<byte[]>();

        if (!kv.TryGetValue("BaseReelStrip1", out var brw))
        {
            throw new Exception("BaseReelStrip1 not found in json object");
        }

        var baseReelStrip1 = JsonSerializer.Deserialize<Dictionary<string, string[]>>(brw.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid BaseReelStrip1 format");

        InitializeReelStrips(baseReelStrip1);

        if (!kv.TryGetValue("BaseReelStrip2", out var brw2))
        {
            throw new Exception("BaseReelStrip2 not found in json object");
        }

        var baseReelStrip2 = JsonSerializer.Deserialize<Dictionary<string, string[]>>(brw2.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid BaseReelStrip2 format");

        InitializeReelStrips(baseReelStrip2);
    }

    private void InitializeReelStrips(Dictionary<string, string[]> rs)
    {
        // 릴 순서대로 정렬 (Reel1, Reel2, Reel3...)
        var orderedReels = rs.OrderBy(kv => kv.Key).ToList();

        // 각 릴의 심볼 배열 생성
        byte[][] reelStrip = new byte[rs.Count][];
        byte[] reelLengths = new byte[rs.Count];

        for (int i = 0; i < orderedReels.Count; i++)
        {
            // 문자열 심볼들을 byte로 변환
            reelStrip[i] = orderedReels[i].Value.Select(s => s.ToSymbolValue()).ToArray();
            reelLengths[i] = (byte)reelStrip[i].Length;
        }

        baseReelStrips.Add(reelStrip);
        baseReelStriptLengths.Add(reelLengths);
    }

    public (byte[], byte[][]) GetReelStrip(int idx)
    {
        return (baseReelStriptLengths[idx], baseReelStrips[idx]);
    }
}

