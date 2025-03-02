using System.Text.Json;
using LottaCashMummy.Common;
namespace LottaCashMummy.Table;

public class FeatureSymbolWeightModel(string[] symbol, string[] weight)
{
    public string[] Symbol { get; set; } = symbol;
    public string[] Weight { get; set; } = weight;
}

public class FeatureBonusSymbolWeightModel(string[] quantity, string[] weight)
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
    public static readonly string[] SymbolSelectKeys = new[] { "ScreenArea", "MummyArea", "Split", "RedCoin" };
    public static readonly string[] SymbolValueKeys = new[] { "Collect", "Spins", "Symbols", "CollectSpins", "CollectSymbols", "SpinsSymbols", "AllFeatures" };

    public static (FeatureSymbolType[], int) ReadSymbolWeights(GameDataLoader kv, string levelKey, string symbolSelectKey)
    {
        var key = $"{levelKey}_{symbolSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"SymbolSelect not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureSymbolWeightModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid SymbolSelect format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var totalWeight = weights.Sum();
        var result = new FeatureSymbolType[totalWeight];

        int currentIndex = 0;
        for (int i = 0; i < model.Symbol.Length; i++)
        {
            var weight = weights[i];
            if (weight <= 0)
            {
                Console.WriteLine($"[passed] - {key} - symbol: {model.Symbol[i]} - weight: {weight}");
                continue;
            }
            for (int j = 0; j < weight; j++)
            {
                result[currentIndex++] = BonusTypeConverter.GetSymbolType(model.Symbol[i]);
            }
        }

        return (result, totalWeight);
    }

    public static (int[], int) ReadSymbolBonusSelect(GameDataLoader kv, string levelKey, string symbolBonusSelectKey)
    {
        var key = $"{levelKey}_{symbolBonusSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"Symbol not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureBonusSymbolWeightModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid Symbol format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var totalWeight = weights.Sum();
        var result = new int[totalWeight];

        int currentIndex = 0;
        for (int i = 0; i < model.Quantity.Length; i++)
        {
            var weight = weights[i];
            if (weight <= 0)
            {
                Console.WriteLine($"[passed] - {key} - quantity: {model.Quantity[i]} - weight: {weight}");
                continue;
            }
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

            var bonusType = BonusTypeConverter.GetBonusType(valueKey);
            if (bonusType == FeatureBonusType.None)
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
                if (weight <= 0)
                {
                    Console.WriteLine($"[passed] - {key} - value: {model.Value[i]} - weight: {weight}");
                    continue;
                }
                for (int j = 0; j < weight; j++)
                {
                    symbolValues[currentIndex++] = new FeatureSymbolValue(BonusTypeConverter.GetFeatureBonusValueType(model.Type[i]), double.Parse(model.Value[i]));
                }
            }

            var typeIdx = BonusTypeConverter.GetBonusTypeOrder(bonusType) - 1;
            result[typeIdx] = symbolValues;
            totalWeights[typeIdx] = totalWeight;
        }

        return (result, totalWeights);
    }
}