using LottaCashMummy.Shared;

namespace LottaCashMummy.Buffer;

public class MummyState
{
    public int CenterIndex { get; set; }
    public int Area { get; private set; }
    public byte Level { get; private set; }
    public int GemCount { get; private set; }
    public int GemsToLevel { get; private set; }

    public void Init(int centerIndex, int area, int level, int gemsToLevel)
    {
        CenterIndex = centerIndex;
        Area = area;
        Level = (byte)level;
        GemCount = 0;
        GemsToLevel = gemsToLevel;
    }

    public void Clear()
    {
        CenterIndex = 0;
        Area = 0;
        Level = 0;
        GemCount = 0;
        GemsToLevel = 0;
    }

    public void ObtainGem(int count)
    {
        GemCount += count;
    }

    public bool CanLevelUp()
    {
        return Level < SlotConst.MAX_FEATURE_LEVEL && GemCount >= GemsToLevel;
    }

    public bool LevelUp(int newArea, int nextGemsToLevel, out int remainGemCount)
    {
        remainGemCount = 0;
        GemCount -= GemsToLevel;
        if (GemCount < 0)
        {
            return false;
        }

        remainGemCount = GemCount;
        Level++;
        Area = newArea;
        GemsToLevel = nextGemsToLevel;

        return true;
    }

    public bool IsMaxLevel()
    {
        return Level >= SlotConst.MAX_FEATURE_LEVEL;
    }
}