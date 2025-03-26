using MoMoMummy.ThreadStorage;
using MoMoMummy.Shared;

namespace MoMoMummy.Game;

public class FeatureSymbolCreate
{
    private readonly IFeatureData fd;

    public FeatureSymbolCreate(IFeatureData featureData)
    {
        this.fd = featureData;
    }

    public (int splitSymbolIndex, bool hasRedCoin) CreateSymbol(FeatureStorage fs)
    {
        var spinResult = fs.SpinResult;
        var random = fs.Random;

        var level = fs.Mummy.Level;
        var redCoinIndex = GetRedCoinIndex(fs);
        var splitSymbolIndex = GetSplitSymbolIdx(fs);
        var hasRedCoin = false;

        for (int idx = 0; idx < SlotConst.SCREEN_AREA; idx++)
        {
            if (fs.IsActiveMummyArea(idx))  // Mummy Area
            {
                if (redCoinIndex == idx)
                {
                    hasRedCoin = true;
                    continue;
                }

                spinResult.AddCoinSpinCount();
                if (fd.Symbol.GetRollSymbolInMummyArea(level, random) != FeatureSymbolType.Coin)
                    continue;

                if (splitSymbolIndex == idx)
                    SetSplitSymbolValue(fs, FeatureSymbolType.Coin);
                else
                    SetSymbolValue(fs, FeatureSymbolType.Coin);
            }
            else  // Screen Area
            {
                spinResult.AddGemSpinCount();
                if (fd.Symbol.GetRollSymbolInScreenArea(level, random) != FeatureSymbolType.Gem)
                    continue;

                if (splitSymbolIndex == idx)
                    SetSplitSymbolValue(fs, FeatureSymbolType.Gem);
                else
                    SetSymbolValue(fs, FeatureSymbolType.Gem);
            }
        }

        return (splitSymbolIndex, hasRedCoin);
    }

    public (int, bool) CreateSymbolRespin(FeatureStorage fs, int splitSymbolIndex)
    {
        var spinResult = fs.SpinResult;
        var level = fs.Mummy.Level;
        var random = fs.Random;

        if (splitSymbolIndex == -1)
            splitSymbolIndex = GetSplitSymbolIdx(fs);

        var hasCoin = false;

        var mummyActiveCount = fs.MummyActiveIndices.Length;
        for (int idx = 0; idx < mummyActiveCount; idx++)
        {
            if (!fs.IsActiveMummyArea(idx))
                throw new Exception($"Invalid mummy area index {idx}");

            spinResult.AddCoinSpinCountRespin();
            if (fd.Symbol.GetRollSymbolInMummyArea(level, random) != FeatureSymbolType.Coin)
                continue;

            hasCoin = true;

            if (splitSymbolIndex == idx)
                SetSplitSymbolValueRespin(fs, FeatureSymbolType.Coin);
            else
                SetSymbolValueRespin(fs, FeatureSymbolType.Coin);
        }

        return (splitSymbolIndex, hasCoin);
    }

    private void SetSymbolValue(FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var random = fs.Random;
        var spinResult = fs.SpinResult;

        spinResult.AddSymbolCount(symbolType);
        var value = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);
        spinResult.AddSymbolValue(symbolType, value.Value);
    }

    private void SetSymbolValueRespin(FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var random = fs.Random;
        var spinResult = fs.SpinResult;

        spinResult.AddRespinSymbolCount(symbolType);
        var value = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);
        spinResult.AddRespinSymbolValue(symbolType, value.Value);
    }

    private void SetSplitSymbolValue(FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var random = fs.Random;
        var spinResult = fs.SpinResult;

        var value1 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);
        var value2 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);

        spinResult.AddSymbolCount(symbolType);
        spinResult.AddSymbolValue(symbolType, value1.Value);

        spinResult.AddSymbolCount(symbolType);
        spinResult.AddSymbolValue(symbolType, value2.Value);
    }

    private void SetSplitSymbolValueRespin(FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var random = fs.Random;
        var spinResult = fs.SpinResult;

        var value1 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);
        var value2 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, random);

        spinResult.AddRespinSymbolCount(symbolType);
        spinResult.AddRespinSymbolValue(symbolType, value1.Value);

        spinResult.AddRespinSymbolCount(symbolType);
        spinResult.AddRespinSymbolValue(symbolType, value2.Value);
    }

    private int GetRedCoinIndex(FeatureStorage fs)
    {
        var bonusType = fs.FeatureBonusType;
        if ((bonusType & FeatureBonusType.Collect) != FeatureBonusType.Collect)
        {
            return -1;
        }

        if (fd.Symbol.GetRollRedCoinSymbolCount(fs.Mummy.Level, fs.Random) == 0)
        {
            return -1;
        }

        var idx = fs.Random.Next(fs.MummyActiveIndices.Length);
        return fs.MummyActiveIndices[idx];
    }

    private int GetSplitSymbolIdx(FeatureStorage fs)
    {
        var bonusType = fs.FeatureBonusType;
        if ((bonusType & FeatureBonusType.Symbols) != FeatureBonusType.Symbols)
        {
            return -1;
        }

        if (fd.Symbol.GetRollSplitSymbolCount(fs.Mummy.Level, fs.Random) == 0)
        {
            return -1;
        }

        return fs.Random.Next(SlotConst.SCREEN_AREA);
    }
}

