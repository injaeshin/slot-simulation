using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Table;

public interface IFeatureSymbol
{
    FeatureSymbolType GetRollSymbolSelectWithGem(int level, Random random);
    FeatureSymbolType GetRollSymbolSelectNoGem(int level, Random random);
    int GetRollSymbolSplitSelect(int level, Random random);
    FeatureSymbolValue GetRollSymbolValues(int level, FeatureBonusCombiType combiType, Random random);
}

public class FeatureSymbol : IFeatureSymbol
{
    // 레벨별 심볼 데이터를 배열로 변경 (1-based index)
    private readonly FeatureSymbolByLevel[] symbolsByLevel;

    private const int MAX_LEVEL = 4;

    public FeatureSymbol(GameDataLoader kv)
    {
        var parser = new FeatureSymbolModelParser();
        symbolsByLevel = new FeatureSymbolByLevel[MAX_LEVEL + 1];  // 0번 인덱스는 사용하지 않음

        foreach (var (lv, _) in FeatureSymbolModelParser.SymbolLevelKeys)
        {
            symbolsByLevel[lv] = parser.Read(kv, lv);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FeatureSymbolByLevel GetSymbols(int level)
    {
        return symbolsByLevel[level];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolSelectWithGem(int level, Random random)
    {
        return GetSymbols(level).GetRollSymbolSelectWithGem(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolSelectNoGem(int level, Random random)
    {
        return GetSymbols(level).GetRollSymbolSelectNoGem(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetRollSymbolSplitSelect(int level, Random random)
    {
        return GetSymbols(level).GetRollSplitSymbolSelect(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolValue GetRollSymbolValues(int level, FeatureBonusCombiType combiType, Random random)
    {
        return GetSymbols(level).GetRollSymbolValues(random, combiType);
    }
}
