
using Dapper.ColumnMapper;
using LottaCashMummy.Common;
using SQLite;

namespace LottaCashMummy.Model;

[Table("base_game")]
public class BaseGameModel
{
    [PrimaryKey, AutoIncrement]
    //[ColumnMapping("id")]
    public int Id { get; set; }

    [NotNull]
    //[ColumnMapping("sym_type")]
    public SymbolType SymbolType { get; set; }

    [NotNull]
    //[ColumnMapping("hit")]
    public int Hit { get; set; }

    [NotNull]
    //[ColumnMapping("amount")]
    public int Amount { get; set; }
}

