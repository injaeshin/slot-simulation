using LottaCashMummy.Common;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Table;

public interface IFeatureSymbol
{
    FeatureSymbolType GetRollSymbolInScreenArea(int level, Random random);
    FeatureSymbolType GetRollSymbolInMummyArea(int level, Random random);
    int GetRollSplitSymbolCount(int level, Random random);
    FeatureSymbolValue GetRollSymbolValues(int level, FeatureBonusCombiType combiType, Random random);
}

public class FeatureSymbol : IFeatureSymbol
{
    // 레벨별 심볼 데이터를 배열로 변경 (1-based index)
    private readonly FeatureSymbolByLevel_Renew[] symbolsByLevel;

    public FeatureSymbol(GameDataLoader kv)
    {
        symbolsByLevel = new FeatureSymbolByLevel_Renew[SymbolModelParser.SymbolLevelKeys.Count];

        foreach (var (lv, _) in SymbolModelParser.SymbolLevelKeys)
        {
            symbolsByLevel[lv - 1] = new FeatureSymbolByLevel_Renew(kv, lv);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FeatureSymbolByLevel_Renew GetSymbols(int level)
    {
        return symbolsByLevel[level - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolInScreenArea(int level, Random random)
    {
        return GetSymbols(level).GetRollSymbolInScreenArea(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolType GetRollSymbolInMummyArea(int level, Random random)
    {
        return GetSymbols(level).GetRollSymbolInMummyArea(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetRollSplitSymbolCount(int level, Random random)
    {
        return GetSymbols(level).GetRollSplitSymbolCount(random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FeatureSymbolValue GetRollSymbolValues(int level, FeatureBonusCombiType combiType, Random random)
    {
        return GetSymbols(level).GetRollSymbolValues(random, combiType);
    }
}
