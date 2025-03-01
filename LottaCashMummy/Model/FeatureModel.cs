using Dapper.ColumnMapper;
using LottaCashMummy.Common;
using SQLite;

namespace LottaCashMummy.Model;

[Table("feature_enter")]
public class FeatureEnterModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public byte Level { get; set; }

    [NotNull]
    public int EnterCount { get; set; }
}


