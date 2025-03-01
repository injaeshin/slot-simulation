
using Dapper.ColumnMapper;
using LottaCashMummy.Common;
using SQLite;

namespace LottaCashMummy.Model;

[Table("base_spin")]
public class BaseSpinModel
{
    [PrimaryKey]
    public int SpinCount { get; set; }
}

[Table("base_payout")]
public class BasePayoutModel
{
    [PrimaryKey]
    //[ColumnMapping("sym_type")]
    public SymbolType SymbolType { get; set; }

    [PrimaryKey]
    //[ColumnMapping("hit")]
    public int Hit { get; set; }

    [NotNull]
    //[ColumnMapping("amount")]
    public int Amount { get; set; }
}

