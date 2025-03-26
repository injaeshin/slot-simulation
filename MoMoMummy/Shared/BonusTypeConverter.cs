using System.Runtime.CompilerServices;

namespace MoMoMummy.Shared;

public static class BonusTypeConverter
{
    // 비트 플래그 타입의 올바른 순서와 값을 매핑
    public static readonly Dictionary<FeatureBonusType, int> BonusTypeOrder = new Dictionary<FeatureBonusType, int>
    {
        { FeatureBonusType.Collect, 1 },
        { FeatureBonusType.Spins, 2 },
        { FeatureBonusType.Symbols, 3 },
        { FeatureBonusType.CollectSpins, 4 },
        { FeatureBonusType.CollectSymbols, 5 },
        { FeatureBonusType.SpinsSymbols, 6 },
        { FeatureBonusType.CollectSpinsSymbols, 7 }
    };

    public static int GetBonusTypeOrder(FeatureBonusType bonusType)
    {
        return BonusTypeOrder[bonusType];
    }

    public static FeatureSymbolType GetSymbolType(string symbol)
    {
        return symbol switch
        {
            "Coin" => FeatureSymbolType.Coin,
            "Gem" => FeatureSymbolType.Gem,
            "Blank" => FeatureSymbolType.Blank,
            _ => throw new Exception("Invalid symbol"),
        };
    }

    public static FeatureBonusValueType GetFeatureBonusValueType(string type)
    {
        return type switch
        {
            "Pay" => FeatureBonusValueType.Pay,
            "Grand" => FeatureBonusValueType.Grand,
            "Mega" => FeatureBonusValueType.Mega,
            "Major" => FeatureBonusValueType.Major,
            "Minor" => FeatureBonusValueType.Minor,
            "Mini" => FeatureBonusValueType.Mini,
            "1Spin" => FeatureBonusValueType.PlusSpin,
            _ => FeatureBonusValueType.Pay,
        };
    }


    public static FeatureBonusType GetBonusType(string type)
    {
        return type switch
        {
            "Collect" => FeatureBonusType.Collect,
            "Spins" => FeatureBonusType.Spins,
            "Symbols" => FeatureBonusType.Symbols,
            "CollectSpins" => FeatureBonusType.CollectSpins,
            "CollectSymbols" => FeatureBonusType.CollectSymbols,
            "SpinsSymbols" => FeatureBonusType.SpinsSymbols,
            "AllFeatures" => FeatureBonusType.CollectSpinsSymbols,
            _ => throw new ArgumentException($"Invalid bonus type: {type}"),
        };
    }
}