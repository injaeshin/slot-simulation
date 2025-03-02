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

    public bool CollectCoinInMummyArea(FeatureStorage fs, bool isRespin)
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
                hasRedCoin = fs.CollectSymbolValue(idx, symbol.First, isRespin);
                fs.CollectSymbol(idx, symbol.First, isRespin);
            }
            else if (symbol.Second.Type == FeatureSymbolType.Coin)
            {
                hasRedCoin = fs.CollectSymbolValue(idx, symbol.Second, isRespin);
                fs.CollectSymbol(idx, symbol.Second, isRespin);
            }
        }

        return hasRedCoin;
    }

    public void CollectGem(FeatureStorage fs, bool shouldLog = true)
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
                //FeatureMummy.Move(fs, idx);
            }

            var isRespin = false;
            if (symbol.First.Type == FeatureSymbolType.Gem)
            {
                //fs.CollectSymbolValue(idx, symbol.First, isRespin, shouldLog);
                fs.CollectSymbol(idx, symbol.First, isRespin, shouldLog);
            }
            else if (symbol.Second.Type == FeatureSymbolType.Gem)
            {
                //fs.CollectSymbolValue(idx, symbol.Second, isRespin, shouldLog);
                fs.CollectSymbol(idx, symbol.Second, isRespin, shouldLog);
            }

            //mummy.LevelUp(fs);
        }
    }

    public void RemoveCoin(FeatureStorage fs)
    {
        var coinCount = fs.CoinCount;
        if (coinCount <= 0)
            throw new Exception("Coin not found");

        var coinIndices = fs.CoinIndices;
        for (int i = 0; i < coinCount; i++)
        {
            var idx = coinIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasCoin())
                throw new Exception("Coin not found");

            fs.ObtainCoin(symbol.First);
        }
    }

    public void ForceRemoveGem(FeatureStorage fs)
    {
        var gemCount = fs.GemCount;
        if (gemCount <= 0)
            throw new Exception("Gem not found");

        var gemIndices = fs.GemIndices;
        for (int i = 0; i < gemCount; i++)
        {
            var idx = gemIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasGem())
                throw new Exception("Gem not found");

            fs.ForceObtainGem(symbol.First);
        }
    }

    public void RemoveGem(FeatureStorage fs)
    {
        var gemCount = fs.GemCount;
        if (gemCount <= 0)
            throw new Exception("Gem not found");

        var gemIndices = fs.GemIndices;
        for (int i = 0; i < gemCount; i++)
        {
            var idx = gemIndices[i];
            var symbol = fs.GetSymbol(idx);
            if (!symbol.HasGem())
                throw new Exception("Gem not found");
            
            fs.ObtainGem(symbol.First);
        }
    }
}
