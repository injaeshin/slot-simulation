
using MoMoMummy.Shared;
using System.Runtime.CompilerServices;

namespace MoMoMummy.Table;

public class GemCredit(double value, int weight)
{
    public double Value { get; private set; } = value;
    public int Weight { get; private set; } = weight;
}

public class GemBonus(GemBonusType type, int weight)
{
    public GemBonusType Type { get; private set; } = type;
    public int Weight { get; private set; } = weight;
}

public class FeatureBonusTrigger(FeatureBonusType type, int quantity, int weight)
{
    public FeatureBonusType Type { get; private set; } = type;
    public int Quantity { get; private set; } = quantity;
    public int Weight { get; internal set; } = weight;
}

public interface IBaseSymbol
{
    double GetRollGemCredit(Random rng);
    GemBonusType GetRollGemBonusType(Random rng);
    FeatureBonusTrigger GetRollBonusTrigger(Random rng);
}

public class BaseSymbol : IBaseSymbol
{
    private readonly List<GemCredit> gemCredit;
    private readonly int gemCreditTotalWeight;
    private readonly List<GemBonus> gemBonus;
    private readonly int gemBonusTotalWeight;
    private readonly List<FeatureBonusTrigger> featureTrigger;
    private readonly int featureTriggerTotalWeight;

    // 확장된 배열들
    private double[] expandedGemCredit = Array.Empty<double>();
    private GemBonusType[] expandedGemBonus = Array.Empty<GemBonusType>();
    private FeatureBonusTrigger[] expandedFeatureTrigger = Array.Empty<FeatureBonusTrigger>();

    public BaseSymbol(GameDataLoader kv)
    {
        var parser = new BaseSymbolModelParser();

        (gemCredit, gemCreditTotalWeight) = parser.ReadGemCredit(kv);
        (gemBonus, gemBonusTotalWeight) = parser.ReadGemAttribute(kv);
        (featureTrigger, featureTriggerTotalWeight) = parser.ReadBonusTrigger(kv);

        // 데이터 로드 후 확장 배열 초기화
        ExpandAllValues();
    }

    // 모든 가중치 데이터를 확장 배열로 변환
    private void ExpandAllValues()
    {
        // GemCredit 확장
        expandedGemCredit = new double[gemCreditTotalWeight];
        int currentIndex = 0;
        for (int i = 0; i < gemCredit.Count; i++)
        {
            var weight = i == 0 ? gemCredit[i].Weight : gemCredit[i].Weight - gemCredit[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedGemCredit[currentIndex++] = gemCredit[i].Value;
            }
        }

        // GemBonus 확장
        expandedGemBonus = new GemBonusType[gemBonusTotalWeight];
        currentIndex = 0;
        for (int i = 0; i < gemBonus.Count; i++)
        {
            var weight = i == 0 ? gemBonus[i].Weight : gemBonus[i].Weight - gemBonus[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedGemBonus[currentIndex++] = gemBonus[i].Type;
            }
        }

        // FeatureTrigger 확장
        expandedFeatureTrigger = new FeatureBonusTrigger[featureTriggerTotalWeight];
        currentIndex = 0;
        for (int i = 0; i < featureTrigger.Count; i++)
        {
            var weight = i == 0 ? featureTrigger[i].Weight : featureTrigger[i].Weight - featureTrigger[i - 1].Weight;
            for (int j = 0; j < weight; j++)
            {
                expandedFeatureTrigger[currentIndex++] = featureTrigger[i];
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureBonusTrigger GetRollBonusTrigger(Random rng)
    {
        return expandedFeatureTrigger[rng.Next(expandedFeatureTrigger.Length)];
    }    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetRollGemCredit(Random rng)
    {
        return expandedGemCredit[rng.Next(expandedGemCredit.Length)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GemBonusType GetRollGemBonusType(Random rng)
    {
        return expandedGemBonus[rng.Next(expandedGemBonus.Length)];
    }
}