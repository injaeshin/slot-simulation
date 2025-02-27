using System.Runtime.CompilerServices;
using LottaCashMummy.Common;

namespace LottaCashMummy.Buffer;

public class Symbol
{
    public byte Index { get; set; }
    public double Value { get; set; }
    public SymbolType Type { get; set; }
    public GemBonusType BonusType { get; set; }

    public Symbol() : base()
    {
        Index = 0;
        Value = 0;
        Type = SymbolType.None;
        BonusType = GemBonusType.None;
    }

    public void SetSymbol(byte index, SymbolType symbolType, GemBonusType bonusType, double value)
    {
        Index = index;
        Type = symbolType;
        BonusType = bonusType;
        Value = value;
    }

    public void Clear()
    {
        Index = 0;
        Value = 0;
        Type = SymbolType.None;
        BonusType = GemBonusType.None;
    }
}

public class FeatureSymbol
{
    public FeatureSymbolType Type { get; set; }
    public FeatureBonusValueType BonusType { get; set; }
    public double Value { get; set; }

    public FeatureSymbol()
    {
        Type = FeatureSymbolType.None;
        BonusType = FeatureBonusValueType.None;
        Value = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(FeatureSymbolType type, FeatureBonusValueType bonusType, double value)
    {
        Type = type;
        BonusType = bonusType;
        Value = value;
    }

    public void Clear()
    {
        Type = FeatureSymbolType.None;
        BonusType = FeatureBonusValueType.None;
        Value = 0;
    }
}

public class SymbolPair
{
    private readonly FeatureSymbol first;
    private readonly FeatureSymbol second;

    public FeatureSymbol First => first;
    public FeatureSymbol Second => second;

    public SymbolPair()
    {
        first = new FeatureSymbol();
        second = new FeatureSymbol();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasGem()
    {
        return first.Type == FeatureSymbolType.Gem || second.Type == FeatureSymbolType.Gem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSymbol(FeatureSymbolType type, FeatureBonusValueType bonusType, double value)
    {
        // 분기문 최소화를 위한 비트 연산
        var target = first.Type == FeatureSymbolType.None ? first : second;
        target.Set(type, bonusType, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasCoin()
    {
        return first.Type == FeatureSymbolType.Coin || second.Type == FeatureSymbolType.Coin;
    }

    public (FeatureSymbol, FeatureSymbol) GetSymbols()
    {
        return (first, second);
    }

    public bool IsEmpty()
    {
        return first.Type == FeatureSymbolType.None && second.Type == FeatureSymbolType.None;
    }

    public bool IsFull()
    {
        return first.Type != FeatureSymbolType.None && second.Type != FeatureSymbolType.None;
    }

    public void Clear()
    {
        first.Clear();
        second.Clear();
    }


}