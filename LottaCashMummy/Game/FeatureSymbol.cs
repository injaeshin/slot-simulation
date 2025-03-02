using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using System;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Game;

public class FeatureSymbol
{
    private readonly IFeatureData featureData;

    private readonly double[] value = [500, 100, 80, 60, 50, /*0,*/ 40, 30, 20, 15, 12.5, 10, 8, 5, 4, 3, 2, 1, 0.5];
    private readonly int[] weight = [1, 4, 10, 10, 15, /*15,*/ 15, 15, 15, 15, 15, 15, 20, 20, 20, 20, 30, 300, 500];
    private readonly int[] cumulativeWeight;

    public FeatureSymbol(IFeatureData featureData)
    {
        this.featureData = featureData;

        cumulativeWeight = new int[value.Length];
        cumulativeWeight[0] = weight[0];
        for (int i = 1; i < value.Length; i++)
        {
            cumulativeWeight[i] = cumulativeWeight[i - 1] + weight[i];
        }
    }

    public double GetRandomSymbolValue(Random random)
    {
        var cv = random.Next(0, cumulativeWeight[cumulativeWeight.Length - 1]);

        int selectedIndex = -1;
        for (int i = 0; i < cumulativeWeight.Length; i++)
        {
            if (cv < cumulativeWeight[i])
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex == -1)
        {
            throw new Exception($"Invalid cumulative weight {cv}");
        }

        return cumulativeWeight[selectedIndex];
    }

    public void CreateWithMummyArea(FeatureStorage fs, Random random, bool isRespin = false)
    {
        var level = fs.Mummy.Level;
        var mummyArea = fs.MummyArea;

        // 머미 영역 인덱스 구하기
        Span<int> mummyIndices = stackalloc int[mummyArea.Length];
        int mummyCount = GetAreaIndices(fs, mummyIndices, true);
        int targetSymbolIdx = GetSplitSymbolIdx(fs, random, mummyIndices[..mummyCount]);

        var includeRedCoin = isRespin == false;
        var bonusCombiType = BonusTypeConverter.Convert(fs.FeatureBonusType, includeRedCoin);

        if (isRespin && includeRedCoin)
        {
            throw new Exception("");
        }

        // 머미 영역만 루프
        for (int i = 0; i < mummyCount; i++)
        {
            int idx = mummyIndices[i];
            var symbol = fs.ScreenArea[idx];
            if (symbol.IsFull())
            {
                throw new Exception();
            }

            var featureSymbolType = featureData.FeatureSymbol.GetRollSymbolInMummyArea(level, random);
            if (featureSymbolType == FeatureSymbolType.Blank)
            {
                fs.AddSymbol(idx, FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
                continue;
            }

            var firstSymbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
            fs.AddSymbol(idx, featureSymbolType, firstSymbolValue.BonusType, firstSymbolValue.Value);
            if (isRespin && firstSymbolValue.BonusType == FeatureBonusValueType.RedCoin)
            {
                throw new Exception("Red coin is not allowed to be respin");
            }

            if (targetSymbolIdx == idx)
            {
                var secondSymbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
                fs.AddSymbol(idx, featureSymbolType, secondSymbolValue.BonusType, secondSymbolValue.Value);

                if (isRespin && secondSymbolValue.BonusType == FeatureBonusValueType.RedCoin)
                {
                    throw new Exception("Red coin is not allowed to be respin");
                }
            }
        }
    }

    public (int gemCount, int coinCount) CreateSymbol(FeatureStorage fs)
    {
        var symbolRandom = fs.SymbolRng;
        var valueRandom = fs.ValueRng;

        var level = fs.Mummy.Level;
        var mummyArea = fs.MummyArea;

        var gemCount = 0;
        var coinCount = 0;
        for (int idx = 0; idx < SlotConst.SCREEN_AREA; idx++)
        {
            if (fs.IsActiveMummyArea(idx))
            {
                // Gem
                var featureSymbol = featureData.FeatureSymbol.GetRollSymbolInScreenArea(level, symbolRandom);
                if (featureSymbol == FeatureSymbolType.Gem)
                {
                    gemCount++;
                    var bonusCombiType = BonusTypeConverter.Convert(fs.FeatureBonusType, false);
                    var featureBonus = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, valueRandom);
                    fs.AddSymbol(idx, FeatureSymbolType.Gem, featureBonus.BonusType, featureBonus.Value);
                }
            }
            else
            {
                // Coin
                var featureSymbol = featureData.FeatureSymbol.GetRollSymbolInScreenArea(level, symbolRandom);
                if (featureSymbol == FeatureSymbolType.Coin)
                {
                    coinCount++;
                    var bonusCombiType = BonusTypeConverter.Convert(fs.FeatureBonusType, false);
                    var featureBonus = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, valueRandom);
                    fs.AddSymbol(idx, featureSymbol, featureBonus.BonusType, featureBonus.Value);
                }
            }
        }

        return (gemCount, coinCount);
    }

    public void CreateGem(FeatureStorage fs, Random random)
    {
        var level = fs.Mummy.Level;
        var mummyArea = fs.MummyArea;

        // 비머미 영역 인덱스 구하기
        Span<int> nonMummyIndices = stackalloc int[21];
        int cnt = 0;
        for (int i = 0; i < fs.ScreenArea.Length; i++)
        {
            if (mummyArea[i] == 0)
                nonMummyIndices[cnt++] = 1;
        }
        //int nonMummyCount = GetAreaIndices(fs, nonMummyIndices, false);
        //int targetSymbolIdx = (nonMummyCount > 0) ? GetSplitSymbolIdx(fs, random, nonMummyIndices[..nonMummyCount]) : -1;

        var bonusCombiType = BonusTypeConverter.Convert(fs.FeatureBonusType, false);

        // 비머미 영역만 루프
        //for (int i = 0; i < nonMummyCount; i++)
        for (int i = 0; i < cnt; i++)
        {
            int idx = nonMummyIndices[i];

            var featureSymbolType = featureData.FeatureSymbol.GetRollSymbolInScreenArea(level, random);
            if (featureSymbolType == FeatureSymbolType.Blank)
            {
                fs.AddSymbol(idx, FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
                continue;
            }
            else if (featureSymbolType == FeatureSymbolType.Gem)
            {
                var symbolValue = GetRandomSymbolValue(random);
                fs.AddSymbol(idx, featureSymbolType, FeatureBonusValueType.None, symbolValue);
            }
            else if (featureSymbolType == FeatureSymbolType.Coin)
            {
                //var symbolValue = GetRandomSymbolValue(random);
                //fs.AddSymbol(idx, featureSymbolType, FeatureBonusValueType.Coin, symbolValue);
            }
            else
                throw new Exception("!@#");

            //var symbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
            //fs.AddSymbol(idx, featureSymbolType, symbolValue.BonusType, symbolValue.Value);

            //if (targetSymbolIdx == idx)
            //{
            //    symbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
            //    fs.AddSymbol(idx, featureSymbolType, symbolValue.BonusType, symbolValue.Value);
            //}
        }
    }

    private int GetSplitSymbolIdx(FeatureStorage fs, Random random, ReadOnlySpan<int> targetIndices)
    {
        if ((fs.FeatureBonusType & FeatureBonusType.Symbols) == FeatureBonusType.Symbols &&
            featureData.FeatureSymbol.GetRollSplitSymbolCount(fs.Mummy.Level, random) == 2)
        {
            // Split 을 지정할 인덱스를 랜덤으로 선택
            var idx = random.Next(targetIndices.Length);
            return targetIndices[idx];
        }

        return -1;
    }

    private static int GetAreaIndices(FeatureStorage fs, Span<int> indices, bool insideArea)
    {
        var mummyArea = fs.MummyArea;
        int count = 0;
        int targetValue = insideArea ? 1 : 0;

        for (int i = 0; i < mummyArea.Length; i++)
        {
            if (mummyArea[i] == targetValue)
            {
                indices[count++] = i;
            }
        }

        return count;
    }


}

