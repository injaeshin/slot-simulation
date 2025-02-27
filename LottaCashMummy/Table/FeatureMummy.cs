
namespace LottaCashMummy.Table;

public class MummyLevel(int level, int area, int spin, int reqGem)
{
    public int Level = level;
    public int Area = area;
    public int Spin = spin;
    public int ReqGem = reqGem;
}

public interface IFeatureMummy
{
    bool TryGetMummyLevel(int level, out MummyLevel? mummyLevel);
}

public class FeatureMummy : IFeatureMummy
{
    private readonly SortedDictionary<int, MummyLevel> mummyLevel;

    public FeatureMummy(GameDataLoader kv)
    {
        var parser = new FeatureMummyModelParser();
        this.mummyLevel = parser.ReadMummyLevel(kv);
    }

    public bool TryGetMummyLevel(int level, out MummyLevel? mummyLevel)
    {
        return this.mummyLevel.TryGetValue(level, out mummyLevel);
    }
}
