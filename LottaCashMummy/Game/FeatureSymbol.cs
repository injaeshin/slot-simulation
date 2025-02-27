using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Game;

public class FeatureSymbol
{
    private readonly IFeatureData featureData;

    public FeatureSymbol(IFeatureData featureData)
    {
        this.featureData = featureData;
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

        // 머미 영역만 루프
        for (int i = 0; i < mummyCount; i++)
        {
            int idx = mummyIndices[i];
            var symbol = fs.ScreenArea[idx];
            if (symbol.IsFull())
            {
                throw new Exception();
            }

            var featureSymbolType = featureData.FeatureSymbol.GetRollSymbolSelectNoGem(level, random);
            if (featureSymbolType == FeatureSymbolType.Blank)
            {
                fs.AddSymbol(idx, FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
                continue;
            }

            var firstSymbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
            fs.AddSymbol(idx, featureSymbolType, firstSymbolValue.BonusType, firstSymbolValue.Value);

            if (targetSymbolIdx == idx)
            {
                var secondSymbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
                fs.AddSymbol(idx, featureSymbolType, secondSymbolValue.BonusType, secondSymbolValue.Value);
            }
        }
    }

    public void CreateWithoutMummyArea(FeatureStorage fs, Random random)
    {
        var level = fs.Mummy.Level;
        var mummyArea = fs.MummyArea;

        // 비머미 영역 인덱스 구하기
        Span<int> nonMummyIndices = stackalloc int[mummyArea.Length];
        int nonMummyCount = GetAreaIndices(fs, nonMummyIndices, false);
        int targetSymbolIdx = (nonMummyCount > 0) ? GetSplitSymbolIdx(fs, random, nonMummyIndices[..nonMummyCount]) : -1;

        var bonusCombiType = BonusTypeConverter.Convert(fs.FeatureBonusType, false);

        // 비머미 영역만 루프
        for (int i = 0; i < nonMummyCount; i++)
        {
            int idx = nonMummyIndices[i];

            var featureSymbolType = featureData.FeatureSymbol.GetRollSymbolSelectWithGem(level, random);
            if (featureSymbolType == FeatureSymbolType.Blank)
            {
                fs.AddSymbol(idx, FeatureSymbolType.Blank, FeatureBonusValueType.None, 0);
                continue;
            }

            var symbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
            fs.AddSymbol(idx, featureSymbolType, symbolValue.BonusType, symbolValue.Value);

            if (targetSymbolIdx == idx)
            {
                symbolValue = featureData.FeatureSymbol.GetRollSymbolValues(level, bonusCombiType, random);
                fs.AddSymbol(idx, featureSymbolType, symbolValue.BonusType, symbolValue.Value);
            }
        }
    }

    private int GetSplitSymbolIdx(FeatureStorage fs, Random random, ReadOnlySpan<int> targetIndices)
    {
        if ((fs.FeatureBonusType & FeatureBonusType.Symbols) == FeatureBonusType.Symbols &&
            featureData.FeatureSymbol.GetRollSymbolSplitSelect(fs.Mummy.Level, random) == 2)
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

