
using MoMoMummy.Shared;
using System.Text.Json;

namespace MoMoMummy.Table;

public class MummyLevelModel(string[] level, string[] area, string[] spin, string[] reqGem)
{
    public string[] Level { get; set; } = level;
    public string[] Area { get; set; } = area;
    public string[] Spin { get; set; } = spin;
    public string[] ReqGem { get; set; } = reqGem;
}

public class FeatureMummyModelParser
{
    public SortedDictionary<int, MummyLevel> ReadMummyLevel(GameDataLoader kv)
    {
        var mummyLevel = new SortedDictionary<int, MummyLevel>();

        if (!kv.TryGetValue("MummyLevel", out var mlJson))
        {
            throw new Exception("MummyLevel not found in json object");
        }

        var model = JsonSerializer.Deserialize<MummyLevelModel>(mlJson.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid MummyLevel format");

        for (int i = 0; i < model.Level.Length; i++)
        {
            var level = int.Parse(model.Level[i]);
            var area = int.Parse(model.Area[i]);
            var spin = int.Parse(model.Spin[i]);
            var reqGem = int.Parse(model.ReqGem[i]);

            mummyLevel.Add(level, new MummyLevel(level, area, spin, reqGem));
        }

        return mummyLevel;
    }
}
