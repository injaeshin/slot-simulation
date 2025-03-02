using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Table;
public class FeatureSymbolBonusSpin(FeatureSymbolType symbol, int weight)
{
    public FeatureSymbolType Symbol { get; set; } = symbol;
    public int Weight { get; set; } = weight;
}

public class FeatureSymbolSplitSelect(int value, int weight)
{
    public int Value { get; set; } = value;
    public int Weight { get; set; } = weight;
}

public class FeatureSymbolValue(FeatureBonusValueType bonusType, double value)
{
    public FeatureBonusValueType BonusType { get; set; } = bonusType;
    public double Value { get; set; } = value;
}

public class FeatureSymbolByLevel_Renew
{
    private FeatureSymbolType[] ScreenAreaSymbols { get; set; } = [];
    private int ScreenAreaSymbolsTotalWeight { get; set; }

    private FeatureSymbolType[] MummyAreaSymbols { get; set; } = [];
    private int MummyAreaSymbolsTotalWeight { get; set; }

    private int[] SplitValues { get; set; } = [];
    private int SplitValuesTotalWeight { get; set; }

    private FeatureSymbolValue[][] SymbolValues { get; set; } = [];
    private int[] SymbolValuesTotalWeights { get; set; } = [];

    public FeatureSymbolByLevel_Renew(GameDataLoader kv, int level)
    {
        if (!SymbolModelParser.SymbolLevelKeys.TryGetValue(level, out var symbolLevelKey))
        {
            throw new Exception($"Invalid level: {level}");
        }

        var (screenAreaSymbols, screenAreaSymbolsTotalWeight) = SymbolModelParser.ReadSymbolWeights(kv, symbolLevelKey, SymbolModelParser.SymbolSelectKeys[0]);
        var (mummyAreaSymbols, mummyAreaSymbolsTotalWeight) = SymbolModelParser.ReadSymbolWeights(kv, symbolLevelKey, SymbolModelParser.SymbolSelectKeys[1]);
        var (symbolSplitSelect, symbolSplitSelectTotalWeight) = SymbolModelParser.ReadSymbolSplitSelect(kv, symbolLevelKey, SymbolModelParser.SymbolSelectKeys[2]);
        var (symbolValues, symbolValuesTotalWeights) = SymbolModelParser.ReadSymbolValues(kv, symbolLevelKey);

        ScreenAreaSymbols = screenAreaSymbols;
        ScreenAreaSymbolsTotalWeight = screenAreaSymbolsTotalWeight;
        MummyAreaSymbols = mummyAreaSymbols;
        MummyAreaSymbolsTotalWeight = mummyAreaSymbolsTotalWeight;
        SplitValues = symbolSplitSelect;
        SplitValuesTotalWeight = symbolSplitSelectTotalWeight;
        SymbolValues = symbolValues;
        SymbolValuesTotalWeights = symbolValuesTotalWeights;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolInScreenArea(Random random)
    {
        return ScreenAreaSymbols[random.Next(ScreenAreaSymbolsTotalWeight)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolInMummyArea(Random random)
    {
        return MummyAreaSymbols[random.Next(MummyAreaSymbolsTotalWeight)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetRollSplitSymbolCount(Random random)
    {
        return SplitValues[random.Next(SplitValuesTotalWeight)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolValue GetRollSymbolValues(Random random, FeatureBonusCombiType combiType)
    {
        var combiIdx = BonusTypeConverter.GetCombiTypeIndex(combiType);
        var totalWeight = SymbolValuesTotalWeights[combiIdx];
        var index = random.Next(totalWeight);
        return SymbolValues[combiIdx][index];
    }
}

/*
public class FeatureSymbolByLevel
{
    private const int COMBI_TYPE_COUNT = 11; // FeatureBonusCombiType의 실제 사용되는 타입 수
    private const int MAX_BONUS_TYPES = 11;

    public int Level { get; set; }
    public FeatureSymbolBonusSpin[] SymbolSelectWithGem { get; set; } = Array.Empty<FeatureSymbolBonusSpin>();
    public int SymbolSelectWithGemTotalWeight { get; set; }
    public FeatureSymbolBonusSpin[] SymbolSelectNoGem { get; set; } = Array.Empty<FeatureSymbolBonusSpin>();
    public int SymbolSelectNoGemTotalWeight { get; set; }
    public FeatureSymbolSplitSelect[] SymbolSplitSelect { get; set; } = Array.Empty<FeatureSymbolSplitSelect>();
    public int SymbolSplitSelectTotalWeight { get; set; }

    // Dictionary 대신 배열 사용
    private readonly FeatureSymbolValue[][] symbolValues = new FeatureSymbolValue[MAX_BONUS_TYPES][];
    private readonly int[] symbolValueTotalWeights = new int[MAX_BONUS_TYPES];

    // 가중치 테이블을 미리 전개
    private readonly FeatureSymbolValue[][] expandedValues;

    // 배열 선언만 하고 초기화는 하지 않음
    private FeatureSymbolType[] expandedSymbolSelectWithGem = Array.Empty<FeatureSymbolType>();
    private FeatureSymbolType[] expandedSymbolSelectNoGem = Array.Empty<FeatureSymbolType>();
    private int[] expandedSymbolSplitSelect = Array.Empty<int>();

    public FeatureSymbolByLevel()
    {
        expandedValues = new FeatureSymbolValue[MAX_BONUS_TYPES][];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetCombiTypeIndex(FeatureBonusCombiType combiType)
    {
        return combiType switch
        {
            FeatureBonusCombiType.CollectWithRedCoin => 0,
            FeatureBonusCombiType.CollectNoRedCoin => 1,
            FeatureBonusCombiType.Spins => 2,
            FeatureBonusCombiType.Symbols => 3,
            FeatureBonusCombiType.CollectSpinsWithRedCoin => 4,
            FeatureBonusCombiType.CollectSpinsNoRedCoin => 5,
            FeatureBonusCombiType.CollectSymbolsWithRedCoin => 6,
            FeatureBonusCombiType.CollectSymbolsNoRedCoin => 7,
            FeatureBonusCombiType.SpinsSymbols => 8,
            FeatureBonusCombiType.AllFeaturesWithRedCoin => 9,
            FeatureBonusCombiType.AllFeaturesNoRedCoin => 10,
            _ => throw new ArgumentException("Invalid combi type")
        };
    }

    public void SetSymbolValues(FeatureBonusCombiType combiType, FeatureSymbolValue[] values, int totalWeight)
    {
        var index = GetCombiTypeIndex(combiType);
        symbolValues[index] = values;
        symbolValueTotalWeights[index] = totalWeight;
    }

    // 모든 데이터가 로드된 후 한 번만 호출
    private void ExpandSymbolValues()
    {
        for (int index = 0; index < MAX_BONUS_TYPES; index++)
        {
            var values = symbolValues[index];
            if (values == null) continue;

            var totalWeight = symbolValueTotalWeights[index];
            expandedValues[index] = new FeatureSymbolValue[totalWeight];

            int currentIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                var weight = i == 0 ? values[i].Weight : values[i].Weight - values[i - 1].Weight;
                for (int j = 0; j < weight; j++)
                {
                    expandedValues[index][currentIndex++] = values[i];
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolValue GetRollSymbolValues(Random random, FeatureBonusCombiType combiType)
    {
        var index = GetCombiTypeIndex(combiType);
        var expanded = expandedValues[index];
        return expanded[random.Next(expanded.Length)];
    }

    public void ExpandAllValues()
    {
        // 데이터가 로드된 후의 실제 크기로 배열 재초기화
        expandedSymbolSelectWithGem = new FeatureSymbolType[SymbolSelectWithGemTotalWeight];
        expandedSymbolSelectNoGem = new FeatureSymbolType[SymbolSelectNoGemTotalWeight];
        expandedSymbolSplitSelect = new int[SymbolSplitSelectTotalWeight];

        // 기존 SymbolValues 확장
        ExpandSymbolValues();

        // SymbolSelectWithGem 확장
        int currentIndex = 0;
        for (int i = 0; i < SymbolSelectWithGem.Length; i++)
        {
            var weight = i == 0 ? SymbolSelectWithGem[i].Weight
                : SymbolSelectWithGem[i].Weight - SymbolSelectWithGem[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedSymbolSelectWithGem[currentIndex++] = SymbolSelectWithGem[i].Symbol;
            }
        }

        // SymbolSelectNoGem 확장
        currentIndex = 0;
        for (int i = 0; i < SymbolSelectNoGem.Length; i++)
        {
            var weight = i == 0 ? SymbolSelectNoGem[i].Weight
                : SymbolSelectNoGem[i].Weight - SymbolSelectNoGem[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedSymbolSelectNoGem[currentIndex++] = SymbolSelectNoGem[i].Symbol;
            }
        }

        // SymbolSplitSelect 확장
        currentIndex = 0;
        for (int i = 0; i < SymbolSplitSelect.Length; i++)
        {
            var weight = i == 0 ? SymbolSplitSelect[i].Weight
                : SymbolSplitSelect[i].Weight - SymbolSplitSelect[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedSymbolSplitSelect[currentIndex++] = SymbolSplitSelect[i].Value;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolSelectWithGem(Random random)
    {
        return expandedSymbolSelectWithGem[random.Next(expandedSymbolSelectWithGem.Length)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolSelectNoGem(Random random)
    {
        return expandedSymbolSelectNoGem[random.Next(expandedSymbolSelectNoGem.Length)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetRollSplitSymbolSelect(Random random)
    {
        return expandedSymbolSplitSelect[random.Next(expandedSymbolSplitSelect.Length)];
    }
}
*/