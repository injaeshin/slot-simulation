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

public class FeatureSymbolModelParser
{
    public static readonly Dictionary<int, string> SymbolLevelKeys = new()
    {
        { 1, "2x2" },
        { 2, "3x3" },
        { 3, "4x4" },
        { 4, "5x5" },
    };
    private static readonly string[] SymbolSelectKeys = new[] { "BonusSpin", "CollectSpin", "Symbol" };
    private static readonly string[] SymbolValueKeys = new[] { "CollectWithRedCoin", "CollectNoRedCoin", "Spins", "Symbols", "CollectSpinsWithRedCoin", "CollectSpinsNoRedCoin", "CollectSymbolsWithRedCoin", "CollectSymbolsNoRedCoin", "SpinsSymbols", "AllFeaturesWithRedCoin", "AllFeaturesNoRedCoin" };

    private FeatureSymbolType GetSymbolType(string symbol)
    {
        return symbol switch
        {
            "Coin" => FeatureSymbolType.Coin,
            "Gem" => FeatureSymbolType.Gem,
            "Blank" => FeatureSymbolType.Blank,
            _ => throw new Exception("Invalid symbol"),
        };
    }

    private FeatureBonusValueType GetFeatureBonusValueType(string type)
    {
        return type switch
        {
            "Jackpot" => FeatureBonusValueType.Jackpot,
            "Spin" => FeatureBonusValueType.Spin,
            "RedCoin" => FeatureBonusValueType.RedCoin,
            _ => FeatureBonusValueType.None,
        };
    }

    private FeatureBonusCombiType GetFeatureBonusCombiType(string type)
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

    public FeatureSymbolByLevel Read(GameDataLoader kv, int level)
    {
        if (!SymbolLevelKeys.TryGetValue(level, out var symbolLevelKey))
        {
            throw new Exception($"Invalid level: {level}");
        }

        var (symbolSelectWithGem, symbolSelectWithGemTotalWeight) = ReadSymbolSelect(kv, symbolLevelKey, SymbolSelectKeys[0]);
        var (symbolSelectNoGem, symbolSelectNoGemTotalWeight) = ReadSymbolSelect(kv, symbolLevelKey, SymbolSelectKeys[1]);
        var (symbolSplitSelect, symbolSplitSelectTotalWeight) = ReadSymbolSplitSelect(kv, symbolLevelKey, SymbolSelectKeys[2]);

        var fs = new FeatureSymbolByLevel
        { 
            Level = level,
            SymbolSelectWithGem = symbolSelectWithGem,
            SymbolSelectWithGemTotalWeight = symbolSelectWithGemTotalWeight,
            SymbolSelectNoGem = symbolSelectNoGem,
            SymbolSelectNoGemTotalWeight = symbolSelectNoGemTotalWeight,
            SymbolSplitSelect = symbolSplitSelect,
            SymbolSplitSelectTotalWeight = symbolSplitSelectTotalWeight,
        };

        ReadSymbolValues(kv, symbolLevelKey, fs);
        fs.ExpandAllValues();

        return fs;
    }

    private (FeatureSymbolSelect[], int) ReadSymbolSelect(GameDataLoader kv, string levelKey, string symbolSelectKey)
    {
        var key = $"{levelKey}_{symbolSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"SymbolSelect not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureSymbolSelectModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid SymbolSelect format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var result = new FeatureSymbolSelect[model.Symbol.Length];
        var accumulatedWeight = 0;

        for (int i = 0; i < model.Symbol.Length; i++)
        {
            accumulatedWeight += weights[i];
            result[i] = new FeatureSymbolSelect(
                GetSymbolType(model.Symbol[i]),
                accumulatedWeight
            );
        }

        return (result, accumulatedWeight);
    }

    private (FeatureSymbolSplitSelect[], int) ReadSymbolSplitSelect(GameDataLoader kv, string levelKey, string symbolSplitSelectKey)
    {
        var key = $"{levelKey}_{symbolSplitSelectKey}";
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"Symbol not found: {key}");

        var model = JsonSerializer.Deserialize<FeatureSplitSymbolSelectModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid Symbol format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var result = new FeatureSymbolSplitSelect[model.Quantity.Length];
        var accumulatedWeight = 0;

        for (int i = 0; i < model.Quantity.Length; i++)
        {
            accumulatedWeight += weights[i];
            result[i] = new FeatureSymbolSplitSelect(
                int.Parse(model.Quantity[i]),
                accumulatedWeight
            );
        }

        return (result, accumulatedWeight);
    }

    private void ReadSymbolValues(GameDataLoader kv, string levelKey, FeatureSymbolByLevel fs)
    {
        foreach (var valueKey in SymbolValueKeys)
        {
            var key = $"{levelKey}_{valueKey}";
            if (!kv.TryGetValue(key, out var json))
                throw new Exception($"SymbolValues not found: {key}");

            var combiType = GetFeatureBonusCombiType(valueKey);
            if (combiType == FeatureBonusCombiType.None)
                throw new Exception($"Invalid symbol value type: {valueKey}");

            var model = JsonSerializer.Deserialize<FeatureSymbolValueModel>(json.ToString()!, JsonOptions.Opt)
                ?? throw new Exception($"Invalid SymbolValues format: {key}");

            var weights = model.Weight.Select(int.Parse).ToArray();
            var symbolValues = new FeatureSymbolValue[model.Value.Length];
            var accumulatedWeight = 0;

            for (int i = 0; i < model.Value.Length; i++)
            {
                accumulatedWeight += weights[i];
                symbolValues[i] = new FeatureSymbolValue(
                    double.Parse(model.Value[i]),
                    accumulatedWeight,
                    GetFeatureBonusValueType(model.Type[i])
                );
            }

            fs.SetSymbolValues(combiType, symbolValues, accumulatedWeight);
        }
    }
}