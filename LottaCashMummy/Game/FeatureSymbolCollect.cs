using LottaCashMummy.Buffer;

namespace LottaCashMummy.Game;

public class FeatureSymbolCollect
{
    private readonly FeatureMummy mummy;

    public FeatureSymbolCollect(FeatureMummy mummy)
    {
        this.mummy = mummy;
    }

    public void CollectCoin(FeatureStorage fs, int splitSymbolIndex, bool isRespin, out bool hasRedCoin)
    {
        hasRedCoin = false;
        var coinCount = fs.CoinCount;
        if (coinCount <= 0)
            return;

        var coinIndices = fs.CoinIndices;
        for (int i = 0; i < coinCount; i++)
        {
            var idx = coinIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasCoin() && !symbol.HasRedCoin())
            {
                throw new Exception("Coin not found");
            }

            if (symbol.HasRedCoin())
            {
                hasRedCoin = true;
            }

            if (splitSymbolIndex == idx)
            {
                CollectBasedOnSymbol(fs, idx, symbol.First, isRespin);
                CollectBasedOnSymbol(fs, idx, symbol.Second, isRespin);
            }
            else
            {
                CollectBasedOnSymbol(fs, idx, symbol.First, isRespin);
            }
        }
    }

    public void CollectGem(FeatureStorage fs, int splitSymbolIndex, bool isRespin)
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

            if (splitSymbolIndex == idx)
            {
                CollectBasedOnSymbol(fs, idx, symbol.First, isRespin);
                CollectBasedOnSymbol(fs, idx, symbol.Second, isRespin);
            }
            else
            {
                CollectBasedOnSymbol(fs, idx, symbol.First, isRespin);
            }
        }

        mummy.LevelUp(fs);
    }

    private static void CollectBasedOnSymbol(FeatureStorage fs, int idx, FeatureSymbol symbol, bool isRespin)
    {
        if (isRespin)
        {
            fs.CollectSymbolRespin(idx, symbol);
        }
        else
        {
            fs.CollectSymbol(idx, symbol);
        }
    }
}
