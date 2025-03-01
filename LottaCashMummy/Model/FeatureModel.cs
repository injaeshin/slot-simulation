using Dapper.ColumnMapper;
using LottaCashMummy.Common;

namespace LottaCashMummy.Model;

public class FeatureEnterModel
{
    // 보너스 타입
    // 

    [ColumnMapping("sym_type")]
    public SymbolType SymbolType { get; set; }
    [ColumnMapping("hit")]
    public int Hit { get; set; }
    [ColumnMapping("value")]
    public float Value { get; set; }
}

