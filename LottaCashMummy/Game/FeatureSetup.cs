using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy.Game;

public class FeatureSetup
{
    private readonly IFeatureData featureData;
    private readonly FeatureMummy mummy;

    public FeatureSetup(IFeatureData featureData, FeatureMummy mummy)
    {
        this.featureData = featureData;
        this.mummy = mummy;
    }

    public void Init(BaseStorage bs, FeatureStorage fs, Random random)
    {
        InitializeFeatureGame(fs, bs.FeatureBonusType);
        CopyGemsFromBaseGame(fs, bs);
        SetupInitialMummy(bs, fs, random);
    }

    private void InitializeFeatureGame(FeatureStorage featureStorage, byte bonusType)
    {
        featureStorage.Clear();
        featureStorage.SetBonusType(bonusType);
    }

    private void CopyGemsFromBaseGame(FeatureStorage featureStorage, BaseStorage baseStorage)
    {
        if (baseStorage.WinGemCount <= 0)
        {
            throw new Exception("Win gem count is not set");
        }

        var winGemCount = baseStorage.WinGemCount;
        for (byte i = 0; i < winGemCount; i++)
        {
            var symbol = baseStorage.GetWinGemSymbol(i);
            if (symbol.Type != SymbolType.Gem)
            {
                throw new Exception("Win gem symbol is not set");
            }

            featureStorage.CopyGemSymbol(symbol.Index, FeatureSymbolType.Gem, FeatureBonusValueType.None, symbol.Value);
        }

        featureStorage.SetInitGemCount(winGemCount);
    }

    private void SetupInitialMummy(BaseStorage bs, FeatureStorage fs, Random random)
    {
        int idx = bs.HasMummySymbol ? bs.MummyIndex : bs.GetRollNormalSymbol(random);
        mummy.Init(fs, idx);
    }
}
