
namespace SpinOfFortune.Shared;

public class SlotConverter
{
    private static Dictionary<string, SymbolType> _symbolValues = new()
    {
        { "5x", SymbolType.Wild5x },
        { "4x", SymbolType.Wild4x },
        { "3x", SymbolType.Wild3x },
        { "2x", SymbolType.Wild2x },
        { "7", SymbolType.Seven },
        { "7b", SymbolType.SevenBar },
        { "1b", SymbolType.OneBar },
        { "2b", SymbolType.TwoBar },
        { "3b", SymbolType.ThreeBar },
        { "-", SymbolType.Blank },
        { "bw", SymbolType.Bonus },
    };

    public static SymbolType ToSymbolType(string symbol)
    {
        if (!_symbolValues.TryGetValue(symbol.ToLower(), out var value))
        {
            throw new Exception($"Invalid symbol type: {symbol}");
        }

        return value;
    }
}

