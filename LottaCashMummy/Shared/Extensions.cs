
namespace LottaCashMummy.Shared;

public static class Extensions
{
    private static readonly Dictionary<string, byte> symbolValues = new()
    {
        { "none", (byte)SymbolType.None },
        { "wild", (byte)SymbolType.Wild },
        { "m1", (byte)SymbolType.M1 },
        { "m2", (byte)SymbolType.M2 },
        { "m3", (byte)SymbolType.M3 },
        { "m4", (byte)SymbolType.M4 },
        { "m5", (byte)SymbolType.M5 },
        { "l1", (byte)SymbolType.L1 },
        { "l2", (byte)SymbolType.L2 },
        { "l3", (byte)SymbolType.L3 },
        { "l4", (byte)SymbolType.L4 },
        { "mm", (byte)SymbolType.Mummy },
        { "gem", (byte)SymbolType.Gem },
    };

    public static int GetGemAttributeType(this FeatureBonusType type, Span<GemBonusType> result)
    {
        if (type == FeatureBonusType.None)
            return 0;

        int count = 0;
        if ((type & FeatureBonusType.Collect) == FeatureBonusType.Collect)
            result[count++] = GemBonusType.Collect;
        if ((type & FeatureBonusType.Spins) == FeatureBonusType.Spins)
            result[count++] = GemBonusType.Spins;
        if ((type & FeatureBonusType.Symbols) == FeatureBonusType.Symbols)
            result[count++] = GemBonusType.Symbols;

        return count;
    }

    public static byte ToSymbolValue(this string value)
    {
        if (!symbolValues.TryGetValue(value.ToLower(), out byte symbolValue))
        {
            throw new ArgumentException($"Invalid symbol: {value}");
        }
        
        return symbolValue;
    }
}