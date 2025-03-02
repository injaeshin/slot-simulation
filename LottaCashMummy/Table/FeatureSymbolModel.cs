using System.Text.Json;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using System;
namespace LottaCashMummy.Table;

// 심볼 선택 가중치 (BonusSpin, CollectSpin)
public class FeatureSymbolSelectModel(string[] symbol, string[] weight)
{
    public string[] Symbol { get; set; } = symbol;
    public string[] Weight { get; set; } = weight;
}

// 심볼 분리 선택 가중치 (Symbol)
public class FeatureSplitSymbolSelectModel(string[] quantity, string[] weight)
{
    public string[] Quantity { get; set; } = quantity;
    public string[] Weight { get; set; } = weight;
}

public class FeatureSymbolValueModel(string[] value, string[] weight, string[] type)
{
    public string[] Value { get; set; } = value;
    public string[] Weight { get; set; } = weight;
    public string[] Type { get; set; } = type;
}

public class SymbolModelParser
{
    public static readonly Dictionary<int, string> SymbolLevelKeys = new()
    {
        { 1, "2x2" },
        { 2, "3x3" },
        { 3, "4x4" },
        { 4, "5x5" },
    };
    public static readonly string[] SymbolSelectKeys = new[] { "BonusSpin", "CollectSpin", "Symbol" };
    public static readonly string[] SymbolValueKeys = new[] { "CollectWithRedCoin", "CollectNoRedCoin", "Spins", "Symbols", "CollectSpinsWithRedCoin", "CollectSpinsNoRedCoin", "CollectSymbolsWithRedCoin", "CollectSymbolsNoRedCoin", "SpinsSymbols", "AllFeaturesWithRedCoin", "AllFeaturesNoRedCoin" };

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
            //"Jackpot" => FeatureBonusValueType.Jackpot,
            "Spin" => FeatureBonusValueType.Spin,
            "RedCoin" => FeatureBonusValueType.RedCoin,
            _ => FeatureBonusValueType.Pay,
        };
    }

    // public FeatureSymbolByLevel Read(GameDataLoader kv, int level)
    // {
    //     if (!SymbolLevelKeys.TryGetValue(level, out var symbolLevelKey))
    //     {
    //         throw new Exception($"Invalid level: {level}");
    //     }

    //     var (screenAreaSymbols, screenAreaSymbolsTotalWeight) = ReadSymbolWeights(kv, symbolLevelKey, SymbolSelectKeys[0]);
    //     var (mummyAreaSymbols, mummyAreaSymbolsTotalWeight) = ReadSymbolWeights(kv, symbolLevelKey, SymbolSelectKeys[1]);
    //     var (symbolSplitSelect, symbolSplitSelectTotalWeight) = ReadSymbolSplitSelect(kv, symbolLevelKey, SymbolSelectKeys[2]);

    //     var fs = new FeatureSymbolByLevel_Renew
    //     { 
    //         ScreenAreaSymbols = screenAreaSymbols,
    //         MummyAreaSymbols = mummyAreaSymbols,
    //         SymbolSplitSelect = symbolSplitSelect,
    //     };

    //     ReadSymbolValues(kv, symbolLevelKey, fs);
    //     fs.ExpandAllValues();

    //     return fs;
    // }

    public static (FeatureSymbolType[], int) ReadSymbolWeights(GameDataLoader kv, string levelKey, string symbolSelectKey)
    {
        var key = $"{levelKey}_{symbolSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"SymbolSelect not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureSymbolSelectModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid SymbolSelect format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var totalWeight = weights.Sum();
        var result = new FeatureSymbolType[totalWeight];

        int currentIndex = 0;
        for (int i = 0; i < model.Symbol.Length; i++)
        {
            var weight = weights[i];
            for (int j = 0; j < weight; j++)
            {
                result[currentIndex++] = GetSymbolType(model.Symbol[i]);
            }
        }

        return (result, totalWeight);
    }

    public static (int[], int) ReadSymbolSplitSelect(GameDataLoader kv, string levelKey, string symbolSplitSelectKey)
    {
        var key = $"{levelKey}_{symbolSplitSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"Symbol not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureSplitSymbolSelectModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid Symbol format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var totalWeight = weights.Sum();
        var result = new int[totalWeight];

        int currentIndex = 0;
        for (int i = 0; i < model.Quantity.Length; i++)
        {
            var weight = weights[i];
            for (int j = 0; j < weight; j++)
            {
                result[currentIndex++] = int.Parse(model.Quantity[i]);
            }
        }

        return (result, totalWeight);
    }

    public static (FeatureSymbolValue[][] values, int[] totalWeights) ReadSymbolValues(GameDataLoader kv, string levelKey)
    {
        var result = new FeatureSymbolValue[SymbolValueKeys.Length][];
        var totalWeights = new int[SymbolValueKeys.Length];

        foreach (var valueKey in SymbolValueKeys)
        {
            var key = $"{levelKey}_{valueKey}";
            if (!kv.TryGetValue(key, out var json))
                throw new Exception($"SymbolValues not found: {key}");

            var combiType = BonusTypeConverter.StringToCombiType(valueKey);
            if (combiType == FeatureBonusCombiType.None)
                throw new Exception($"Invalid symbol value type: {valueKey}");

            var model = JsonSerializer.Deserialize<FeatureSymbolValueModel>(json.ToString()!, JsonOptions.Opt)
                ?? throw new Exception($"Invalid SymbolValues format: {key}");

            var weights = model.Weight.Select(int.Parse).ToArray();
            var totalWeight = weights.Sum();
            var symbolValues = new FeatureSymbolValue[totalWeight];

            int currentIndex = 0;
            for (int i = 0; i < model.Value.Length; i++)
            {
                var weight = weights[i];
                for (int j = 0; j < weight; j++)
                {
                    symbolValues[currentIndex++] = new FeatureSymbolValue(GetFeatureBonusValueType(model.Type[i]), double.Parse(model.Value[i]));
                }
            }

            var combiIdx = BonusTypeConverter.GetCombiTypeIndex(combiType);
            result[combiIdx] = symbolValues;
            totalWeights[combiIdx] = totalWeight;
        }

        return (result, totalWeights);
    }

    // private void ReadSymbolValues(GameDataLoader kv, string levelKey, FeatureSymbolByLevel fs)
    // {
    //     foreach (var valueKey in SymbolValueKeys)
    //     {
    //         var key = $"{levelKey}_{valueKey}";
    //         if (!kv.TryGetValue(key, out var json))
    //             throw new Exception($"SymbolValues not found: {key}");

    //         var combiType = GetFeatureBonusCombiType(valueKey);
    //         if (combiType == FeatureBonusCombiType.None)
    //             throw new Exception($"Invalid symbol value type: {valueKey}");

    //         var model = JsonSerializer.Deserialize<FeatureSymbolValueModel>(json.ToString()!, JsonOptions.Opt)
    //             ?? throw new Exception($"Invalid SymbolValues format: {key}");

    //         var weights = model.Weight.Select(int.Parse).ToArray();
    //         var symbolValues = new FeatureSymbolValue[model.Value.Length];
    //         var accumulatedWeight = 0;

    //         for (int i = 0; i < model.Value.Length; i++)
    //         {
    //             accumulatedWeight += weights[i];
    //             symbolValues[i] = new FeatureSymbolValue(
    //                 double.Parse(model.Value[i]),
    //                 accumulatedWeight,
    //                 GetFeatureBonusValueType(model.Type[i])
    //             );
    //         }

    //         fs.SetSymbolValues(combiType, symbolValues, accumulatedWeight);
    //     }
    // }
}