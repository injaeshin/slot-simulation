using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using System;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Game;

public class FeatureSymbolCreate
{
    private readonly IFeatureData fd;

    public FeatureSymbolCreate(IFeatureData featureData)
    {
        this.fd = featureData;
    }

    public int CreateSymbolRespin(FeatureStorage fs, int splitSymbolIndex)
    {
        var level = fs.Mummy.Level;
        var symbolRandom = fs.SymbolRng;

        if (splitSymbolIndex == -1)
            splitSymbolIndex = GetSplitSymbolIdx(fs);

        var mummyActiveCount = fs.MummyActiveIndices.Length;
        for (int idx = 0; idx < mummyActiveCount; idx++)
        {
            if (!fs.IsActiveMummyArea(idx))
                throw new Exception($"Invalid mummy area index {idx}");

            if (fd.Symbol.GetRollSymbolInMummyArea(level, symbolRandom) != FeatureSymbolType.Coin)
                continue;

            if (splitSymbolIndex == idx)
                AddSplitSymbolWithRespin(idx, fs, FeatureSymbolType.Coin);
            else
                AddSymbolWithRespin(idx, fs, FeatureSymbolType.Coin);
        }

        return splitSymbolIndex;
    }

    public int CreateSymbol(FeatureStorage fs)
    {
        var symbolRandom = fs.SymbolRng;

        var level = fs.Mummy.Level;
        //var redCoinIndex = GetRedCoinIndex(fs);
        var splitSymbolIndex = -1; //GetSplitSymbolIdx(fs);

        for (int idx = 0; idx < SlotConst.SCREEN_AREA; idx++)
        {
            if (fs.IsActiveMummyArea(idx))  // Mummy Area
            {
                if (fd.Symbol.GetRollSymbolInScreenArea(level, symbolRandom) != FeatureSymbolType.Gem)
                    continue;

                AddSymbol(idx, fs, FeatureSymbolType.Gem);

                // if (redCoinIndex == idx)
                // {
                //     AddRedCoin(idx, fs);
                //     continue;
                // }

                // // Coin
                // if (fd.Symbol.GetRollSymbolInScreenArea(level, symbolRandom) != FeatureSymbolType.Coin)
                //     continue;

                // if (splitSymbolIndex == idx)
                //     AddSplitSymbol(idx, fs, FeatureSymbolType.Coin);
                // else
                //     AddSymbol(idx, fs, FeatureSymbolType.Coin);
            }
            else  // Screen Area
            {
                // if (fd.Symbol.GetRollSymbolInScreenArea(level, symbolRandom) != FeatureSymbolType.Gem)
                //     continue;

                // if (splitSymbolIndex == idx)
                //     AddSplitSymbol(idx, fs, FeatureSymbolType.Gem);
                // else
                //AddSymbol(idx, fs, FeatureSymbolType.Gem);
                //break;
            }
        }

        return splitSymbolIndex;
    }

    private void AddSplitSymbol(int idx, FeatureStorage fs, FeatureSymbolType symbolType)
    {
        fs.AddSplit();

        var level = fs.Mummy.Level;
        var valueRandom = fs.ValueRng;

        var featureValue = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbol(idx, symbolType, featureValue.BonusType, featureValue.Value);

        var featureValue2 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbol(idx, symbolType, featureValue2.BonusType, featureValue2.Value);
    }

    private void AddSplitSymbolWithRespin(int idx, FeatureStorage fs, FeatureSymbolType symbolType)
    {
        fs.AddSplit();

        var level = fs.Mummy.Level;
        var valueRandom = fs.ValueRng;

        var featureValue = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbolWithRespin(idx, symbolType, featureValue.BonusType, featureValue.Value);

        var featureValue2 = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbolWithRespin(idx, symbolType, featureValue2.BonusType, featureValue2.Value);
    }

    private void AddRedCoin(int idx, FeatureStorage fs)
    {
        fs.AddSymbol(idx, FeatureSymbolType.RedCoin, FeatureBonusValueType.None, 0);
    }

    private void AddSymbol(int idx, FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var valueRandom = fs.ValueRng;

        var symbolValue = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbol(idx, symbolType, symbolValue.BonusType, symbolValue.Value);
    }

    private void AddSymbolWithRespin(int idx, FeatureStorage fs, FeatureSymbolType symbolType)
    {
        var level = fs.Mummy.Level;
        var valueRandom = fs.ValueRng;

        var featureValue = fd.Symbol.GetRollSymbolValues(level, fs.FeatureBonusType, valueRandom);
        fs.AddSymbolWithRespin(idx, symbolType, featureValue.BonusType, featureValue.Value);
    }    

    private int GetRedCoinIndex(FeatureStorage fs)
    {
        var bonusType = fs.FeatureBonusType;
        if ((bonusType & FeatureBonusType.Collect) != FeatureBonusType.Collect)
        {
            return -1;
        }

        if (fd.Symbol.GetRollRedCoinSymbolCount(fs.Mummy.Level, fs.SymbolRng) == 0)
        {
            return -1;
        }

        var idx = fs.SymbolRng.Next(fs.MummyActiveIndices.Length);
        return fs.MummyActiveIndices[idx];
    }

    private int GetSplitSymbolIdx(FeatureStorage fs)
    {
        var bonusType = fs.FeatureBonusType;
        if ((bonusType & FeatureBonusType.Symbols) != FeatureBonusType.Symbols)
        {
            return -1;
        }

        if (fd.Symbol.GetRollSplitSymbolCount(fs.Mummy.Level, fs.SymbolRng) == 0)
        {
            return -1;
        }

        return fs.SymbolRng.Next(fs.ScreenArea.Length);
    }
}

