
using MoMoMummy.Shared;
using System.Text.Json;

namespace MoMoMummy.Table;

public class BaseGemCreditModel(string[] value, string[] weight)
{
    public string[] Value { get; set; } = value;
    public string[] Weight { get; set; } = weight;
}

public class BaseGemBonusTypeModel(string[] name, string[] weight)
{
    public string[] Name { get; set; } = name;
    public string[] Weight { get; set; } = weight;
}

public class BaseFeatureBonusTriggerModel(string[] type, string[] quantity, string[] weight)
{
    public string[] Type { get; set; } = type;
    public string[] Quantity { get; set; } = quantity;
    public string[] Weight { get; set; } = weight;
}

public class BaseSymbolModelParser
{
    public (List<GemCredit>, int) ReadGemCredit(GameDataLoader kv)
    {
        var gemCredit = new List<GemCredit>();
        var gemCreditTotalWeight = 0;

        if (!kv.TryGetValue("GemCredit", out var gemCreditJson))
        {
            throw new Exception("GemCredit not found in json object");
        }

        var model = JsonSerializer.Deserialize<BaseGemCreditModel>(gemCreditJson.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid GemCredit format");

        var weights = model.Weight.Select(w => int.Parse(w)).ToList();
        gemCreditTotalWeight = weights.Sum();
        var accumulatedWeight = 0;

        for (int i = 0; i < model.Value.Length; i++)
        {
            accumulatedWeight += weights[i];
            gemCredit.Add(new GemCredit(double.Parse(model.Value[i]), accumulatedWeight));
        }

        return (gemCredit, gemCreditTotalWeight);
    }

    public (List<GemBonus>, int) ReadGemAttribute(GameDataLoader kv)
    {
        var gemBonus = new List<GemBonus>();
        var gemBonusTotalWeight = 0;

        if (!kv.TryGetValue("GemBonus", out var gemAttributeJson))
        {
            throw new Exception("GemBonus not found in json object");
        }

        var model = JsonSerializer.Deserialize<BaseGemBonusTypeModel>(gemAttributeJson.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid GemBonus format");

        var weights = model.Weight.Select(w => int.Parse(w)).ToList();
        gemBonusTotalWeight = weights.Sum();
        var accumulatedWeight = 0;

        for (int i = 0; i < model.Name.Length; i++)
        {
            accumulatedWeight += weights[i];
            gemBonus.Add(new GemBonus(BaseSymbolModelParser.GetGemBonusType(model.Name[i]), accumulatedWeight));
        }

        return (gemBonus, gemBonusTotalWeight);
    }

    public (List<FeatureBonusTrigger>, int) ReadBonusTrigger(GameDataLoader kv)
    {
        var bonusTrigger = new List<FeatureBonusTrigger>();
        var bonusTriggerTotalWeight = 0;

        if (!kv.TryGetValue("BonusTrigger", out var ftJson))
        {
            throw new Exception("BonusTrigger not found in json object");
        }

        var model = JsonSerializer.Deserialize<BaseFeatureBonusTriggerModel>(ftJson.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid BonusTrigger format");

        var weights = model.Weight.Select(w => int.Parse(w)).ToList();
        bonusTriggerTotalWeight = weights.Sum();
        var accumulatedWeight = 0;

        for (int i = 0; i < model.Type.Length; i++)
        {
            accumulatedWeight += weights[i];
            bonusTrigger.Add(new FeatureBonusTrigger(ParseFeatureBonusType(model.Type[i]), int.Parse(model.Quantity[i]), accumulatedWeight));
        }

        return (bonusTrigger, bonusTriggerTotalWeight);
    }

    private static GemBonusType GetGemBonusType(string name) => name switch
    {
        "Collect" => Shared.GemBonusType.Collect,
        "Spins" => Shared.GemBonusType.Spins,
        "Symbols" => Shared.GemBonusType.Symbols,
        _ => Shared.GemBonusType.None
    };

    private static readonly Dictionary<string, FeatureBonusType> FeatureBonusTypeMap = new()
    {
        { "Collect", FeatureBonusType.Collect },
        { "Spins", FeatureBonusType.Spins },
        { "Symbols", FeatureBonusType.Symbols },
    };

    private static FeatureBonusType ParseFeatureBonusType(string value)
    {
        FeatureBonusType result = FeatureBonusType.None;
        var values = value.Split(',').Select(v => v.Trim());
        foreach (var v in values)
        {
            if (FeatureBonusTypeMap.TryGetValue(v, out var featureBonusType))
            {
                result |= featureBonusType;
            }
        }

        return result;
    }
}