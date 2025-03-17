
using System.Text.Json;
using Common;

namespace SpinOfFortune.Table;

public class BonusValue
{
    public double Value { get; set; }
    public int Weight { get; set; }
}

public class BonusValueModel(string[] value, string[] weight)
{
    public string[] Value { get; set; } = value;
    public string[] Weight { get; set; } = weight;
}

public class BonusValueModelParser
{
    public static (BonusValue[] values, int totalWeights) ReadSymbolValues(string key, GameDataLoader kv)
    {
        if (!kv.TryGetValue(key, out var json))
            throw new Exception($"SymbolValues not found: {key}");

        var model = JsonSerializer.Deserialize<BonusValueModel>(json.ToString()!, JsonOptions.Opt)
            ?? throw new Exception($"Invalid SymbolValues format: {key}");

        var weights = model.Weight.Select(int.Parse).ToArray();
        var totalWeight = weights.Sum();
        var symbolValues = new BonusValue[totalWeight];

        int currentIndex = 0;
        for (int i = 0; i < model.Value.Length; i++)
        {
            var weight = weights[i];
            if (weight <= 0)
            {
                continue;
            }

            var value = double.Parse(model.Value[i]);
            for (int j = 0; j < weight; j++)
            {
                symbolValues[currentIndex++] = new BonusValue { Value = value, Weight = weight };
            }
        }

        return (symbolValues, totalWeight);
    }
}


