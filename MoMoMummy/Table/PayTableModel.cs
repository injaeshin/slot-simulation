using System.Text.Json;
using System.Text.Json.Serialization;

using MoMoMummy.Shared;

namespace MoMoMummy.Table;


public class PayTableModel
{
    public string[] Symbol { get; set; } = [];

    [JsonPropertyName("1 Hit")]
    public string[] Hit1 { get; set; } = [];

    [JsonPropertyName("2 Hit")]
    public string[] Hit2 { get; set; } = [];

    [JsonPropertyName("3 Hit")]
    public string[] Hit3 { get; set; } = [];

    [JsonPropertyName("4 Hit")]
    public string[] Hit4 { get; set; } = [];

    [JsonPropertyName("5 Hit")]
    public string[] Hit5 { get; set; } = [];
}

public class PayTableModelParser
{
    public Dictionary<byte, int[]> ReadPayTable(GameDataLoader kv)
    {
        if (!kv.TryGetValue("PayTable", out var pt))
        {
            throw new Exception("PayTable not found in configuration");
        }

        try
        {
            var model = JsonSerializer.Deserialize<PayTableModel>(pt.ToString()!, JsonOptions.Opt)
                ?? throw new Exception("Invalid PayTable format");

            var payTable = new Dictionary<byte, int[]>();
            var symbols = model.Symbol.Select(s => s.ToSymbolValue()).ToArray();

            for (int i = 0; i < symbols.Length; i++)
            {
                payTable[symbols[i]] = new[]
                {
                    int.Parse(model.Hit1[i]),
                    int.Parse(model.Hit2[i]),
                    int.Parse(model.Hit3[i]),
                    int.Parse(model.Hit4[i]),
                    int.Parse(model.Hit5[i])
                };
            }

            return payTable;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to parse PayTable", ex);
        }
    }
}