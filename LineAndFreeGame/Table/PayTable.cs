using System.Collections.Immutable;
using System.Text.Json;
using LineAndFreeGame.Common;

namespace LineAndFreeGame.Table;

public record PayTableEntry(SymbolType Symbol, int Pay5, int Pay4, int Pay3);

public class PayTable
{
    private const SymbolType WILD = SymbolType.WW;

    private readonly ImmutableDictionary<SymbolType, PayTableEntry> payTable;

    public PayTable(GameDataLoader kv)
    {
        if (!kv.TryGetValue("PayTable", out var payTableObj))
        {
            throw new Exception("PayTable not found");
        }

        var payModel = JsonSerializer.Deserialize<Dictionary<string, object>>(payTableObj.ToString()!, JsonOptions.Opt)
            ?? throw new Exception("Invalid PayTable format");

        var symbols = (payModel["Symbol"] as JsonElement?)?.EnumerateArray().ToList() ?? throw new Exception("Invalid PayTable format");
        var fiveOfKind = (payModel["5 of a Kind"] as JsonElement?)?.EnumerateArray().ToList() ?? throw new Exception("Invalid PayTable format");
        var fourOfKind = (payModel["4 of a Kind"] as JsonElement?)?.EnumerateArray().ToList() ?? throw new Exception("Invalid PayTable format");
        var threeOfKind = (payModel["3 of a Kind"] as JsonElement?)?.EnumerateArray().ToList() ?? throw new Exception("Invalid PayTable format");

        var builder = ImmutableDictionary.CreateBuilder<SymbolType, PayTableEntry>();

        for (int i = 0; i < symbols.Count; i++)
        {
            var symbol = symbols[i];
            var symbolType = SlotConverter.ToSymbolType(symbol.GetString() ?? throw new Exception("Invalid PayTable format"));

            int pay5 = string.IsNullOrEmpty(fiveOfKind[i].ToString()) ? 0 : int.Parse(fiveOfKind[i].ToString() ?? "0");
            int pay4 = string.IsNullOrEmpty(fourOfKind[i].ToString()) ? 0 : int.Parse(fourOfKind[i].ToString() ?? "0");
            int pay3 = string.IsNullOrEmpty(threeOfKind[i].ToString()) ? 0 : int.Parse(threeOfKind[i].ToString() ?? "0");

            builder.Add(symbolType, new PayTableEntry(symbolType, pay5, pay4, pay3));
        }

        payTable = builder.ToImmutable();
    }

    public (SymbolType, int, int) CalculatePay(Span<SymbolType> middleSymbols)
    {
        if (middleSymbols.Length != 5)
        {
            throw new Exception("Invalid middleSymbols length");
        }

        SymbolType? firstNonWildSymbol = null;
        int consecutiveCount = 0;

        for (int i = 0; i < middleSymbols.Length; i++)
        {
            var symbol = middleSymbols[i];
            if (symbol == WILD)
            {
                consecutiveCount++;
                continue;
            }

            if (firstNonWildSymbol == null)
            {
                firstNonWildSymbol = symbol;
                consecutiveCount++;
                continue;
            }

            if (symbol == firstNonWildSymbol)
            {
                consecutiveCount++;
            }
            else
            {
                break;
            }
        }

        if (firstNonWildSymbol == null || !payTable.TryGetValue(firstNonWildSymbol.Value, out PayTableEntry? payEntry))
        {
            return default;
        }

        var pay = consecutiveCount switch
        {
            5 => payEntry.Pay5,
            4 => payEntry.Pay4,
            3 => payEntry.Pay3,
            _ => 0
        };

        return (payEntry.Symbol, consecutiveCount, pay);
    }
}
