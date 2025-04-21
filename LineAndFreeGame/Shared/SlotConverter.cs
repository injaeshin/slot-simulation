
namespace LineAndFree.Shared;

public class SlotConverter
{
    private static Dictionary<string, SymbolType> _symbolValues = new()
    {
        { "AA", SymbolType.AA },
        { "BB", SymbolType.BB },
        { "CC", SymbolType.CC },
        { "DD", SymbolType.DD },
        { "EE", SymbolType.EE },
        { "FF", SymbolType.FF },
        { "GG", SymbolType.GG },
        { "HH", SymbolType.HH },
        { "II", SymbolType.II },
        { "JJ", SymbolType.JJ },
        { "SS", SymbolType.SS },
        { "WW", SymbolType.WW },
    };

    public static SymbolType ToSymbolType(string symbol)
    {
        if (!_symbolValues.TryGetValue(symbol, out var value))
        {
            throw new Exception($"Invalid symbol type: {symbol}");
        }

        return value;
    }
}

