using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy.Game;

public class FeatureSymbolCollect
{
    private readonly FeatureMummy mummy;

    public FeatureSymbolCollect(FeatureMummy mummy)
    {
        this.mummy = mummy;
    }

    public bool CollectCoinInMummyArea(FeatureStorage fs)
    {
        var coinCount = fs.CoinCount;
        if (coinCount <= 0)
            return false;

        bool hasRedCoin = false;
        var coinIndices = fs.CoinIndices;
        for (int i = 0; i < coinCount; i++)
        {
            var idx = coinIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasCoin())
            {
                throw new Exception("Coin not found");
            }

            if (symbol.First.Type == FeatureSymbolType.Coin)
            {
                hasRedCoin = fs.CollectSymbolValue(idx, symbol.First);
                fs.CollectSymbol(idx, symbol.First);
            }
            else if (symbol.Second.Type == FeatureSymbolType.Coin)
            {
                hasRedCoin = fs.CollectSymbolValue(idx, symbol.Second);
                fs.CollectSymbol(idx, symbol.Second);
            }
        }

        return hasRedCoin;
    }

    public void CollectGemScreenArea(FeatureStorage fs)
    {
        var gemCount = fs.GemCount;
        if (gemCount <= 0)
            return;

        var gemIndices = fs.GemIndices;
        for (int i = 0; i < gemCount; i++)
        {
            var idx = gemIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasGem())
            {
                throw new Exception("Gem not found");
            }

            if (!fs.IsActiveMummyArea(idx))
            {
                FeatureMummy.Move(fs, idx);
            }

            if (symbol.First.Type == FeatureSymbolType.Gem)
            {
                fs.CollectSymbol(idx, symbol.First);
            }
            else if (symbol.Second.Type == FeatureSymbolType.Gem)
            {
                fs.CollectSymbol(idx, symbol.Second);
            }

            mummy.LevelUp(fs);
        }
    }
}
