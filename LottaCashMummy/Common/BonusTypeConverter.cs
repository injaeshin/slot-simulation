using System.Runtime.CompilerServices;

namespace LottaCashMummy.Common;

public static class BonusTypeConverter
{
    // 비트 플래그 타입의 올바른 순서와 값을 매핑
    public static readonly Dictionary<FeatureBonusType, int> CombiTypeOrder = new Dictionary<FeatureBonusType, int>
    {
        { FeatureBonusType.Collect, 1 },
        { FeatureBonusType.Spins, 2 },
        { FeatureBonusType.Symbols, 3 },
        { FeatureBonusType.CollectSpins, 4 },
        { FeatureBonusType.CollectSymbols, 5 },
        { FeatureBonusType.SpinsSymbols, 6 },
        { FeatureBonusType.CollectSpinsSymbols, 7 }
    };

    public static int GetCombiTypeOrder(FeatureBonusType bonusType)
    {
        return CombiTypeOrder[bonusType];
    }

    public static FeatureBonusCombiType StringToCombiType(string type)
    {
        return type switch
        {
            "CollectWithRedCoin" => FeatureBonusCombiType.CollectWithRedCoin,
            "CollectNoRedCoin" => FeatureBonusCombiType.CollectNoRedCoin,
            "Spins" => FeatureBonusCombiType.Spins,
            "Symbols" => FeatureBonusCombiType.Symbols,
            "CollectSpinsWithRedCoin" => FeatureBonusCombiType.CollectSpinsWithRedCoin,
            "CollectSpinsNoRedCoin" => FeatureBonusCombiType.CollectSpinsNoRedCoin,
            "CollectSymbolsWithRedCoin" => FeatureBonusCombiType.CollectSymbolsWithRedCoin,
            "CollectSymbolsNoRedCoin" => FeatureBonusCombiType.CollectSymbolsNoRedCoin,
            "SpinsSymbols" => FeatureBonusCombiType.SpinsSymbols,
            "AllFeaturesWithRedCoin" => FeatureBonusCombiType.AllFeaturesWithRedCoin,
            "AllFeaturesNoRedCoin" => FeatureBonusCombiType.AllFeaturesNoRedCoin,
            _ => FeatureBonusCombiType.None,
        };
    }

    public static Dictionary<FeatureBonusCombiType, int> CombiTypeToIndex = new()
    {
        { FeatureBonusCombiType.CollectWithRedCoin, 0 },
        { FeatureBonusCombiType.CollectNoRedCoin, 1 },
        { FeatureBonusCombiType.Spins, 2 },
        { FeatureBonusCombiType.Symbols, 3 },
        { FeatureBonusCombiType.CollectSpinsWithRedCoin, 4 },
        { FeatureBonusCombiType.CollectSpinsNoRedCoin, 5 },
        { FeatureBonusCombiType.CollectSymbolsWithRedCoin, 6 },
        { FeatureBonusCombiType.CollectSymbolsNoRedCoin, 7 },
        { FeatureBonusCombiType.SpinsSymbols, 8 },
        { FeatureBonusCombiType.AllFeaturesWithRedCoin, 9 },
        { FeatureBonusCombiType.AllFeaturesNoRedCoin, 10 },
    };

    public static int GetCombiTypeIndex(FeatureBonusCombiType combiType)
    {
        return CombiTypeToIndex[combiType];
    }


    private static readonly Dictionary<(FeatureBonusType, bool), FeatureBonusCombiType> CombiTypeMap = new()
    {
        // 단일 기능
        { (FeatureBonusType.Collect, false), FeatureBonusCombiType.CollectNoRedCoin },
        { (FeatureBonusType.Collect, true), FeatureBonusCombiType.CollectWithRedCoin },
        { (FeatureBonusType.Spins, false), FeatureBonusCombiType.Spins },
        { (FeatureBonusType.Spins, true), FeatureBonusCombiType.Spins },
        { (FeatureBonusType.Symbols, false), FeatureBonusCombiType.Symbols },
        { (FeatureBonusType.Symbols, true), FeatureBonusCombiType.Symbols },
        
        // 2개 조합
        { (FeatureBonusType.CollectSpins, false), FeatureBonusCombiType.CollectSpinsNoRedCoin },
        { (FeatureBonusType.CollectSpins, true), FeatureBonusCombiType.CollectSpinsWithRedCoin },
        { (FeatureBonusType.CollectSymbols, false), FeatureBonusCombiType.CollectSymbolsNoRedCoin },
        { (FeatureBonusType.CollectSymbols, true), FeatureBonusCombiType.CollectSymbolsWithRedCoin },
        { (FeatureBonusType.SpinsSymbols, false), FeatureBonusCombiType.SpinsSymbols },
        { (FeatureBonusType.SpinsSymbols, true), FeatureBonusCombiType.SpinsSymbols },
        
        // 3개 조합
        { (FeatureBonusType.CollectSpinsSymbols, false), FeatureBonusCombiType.AllFeaturesNoRedCoin },
        { (FeatureBonusType.CollectSpinsSymbols, true), FeatureBonusCombiType.AllFeaturesWithRedCoin },
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FeatureBonusCombiType Convert(FeatureBonusType bonusType, bool includeRedCoin)
    {
        // bonusType 에 Collect 값이 있다면, hasRedCoin 은 true 로 설정
        // includeRedCoin 값이 false 라면, hasRedCoin 은 false 로 설정
        bool hasRedCoin = (bonusType & FeatureBonusType.Collect) == FeatureBonusType.Collect;
        if (!includeRedCoin) hasRedCoin = false;

        return CombiTypeMap.TryGetValue((bonusType, hasRedCoin), out var combiType)
            ? combiType
            : throw new NotImplementedException();
    }
} 