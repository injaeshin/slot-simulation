//using SQLite;

using Dapper.ColumnMapper;

namespace LottaCashMummy.Model;

public class FeatureEnter
{
    [ColumnMapping("bonus_type")]
    public int BonusType { get; set; }
    [ColumnMapping("gem")]
    public int Gem { get; set; }
    [ColumnMapping("level")]
    public int Level { get; set; }
    [ColumnMapping("enter_count")]
    public int EnterCount { get; set; }
    [ColumnMapping("spin_count")]
    public int SpinCount { get; set; }
}

// public class FeatureModel
// {
//     [PrimaryKey]
//     public int BonusType { get; set; }

//     [PrimaryKey]
//     public int Gem { get; set; }

//     [PrimaryKey]
//     public int Level { get; set; }
// }

// [Table("feature_level")]
// public class FeatureLevelModel : FeatureModel
// {
//     [NotNull]
//     public int LevelCount { get; set; }
// }

// [Table("feature_spin")]
// public class FeatureSpinModel : FeatureModel
// {
//     [NotNull]
//     public int SpinCount { get; set; }
// }

// [Table("feature_gem")]
// public class FeatureGemModel : FeatureModel
// {
//     [NotNull]
//     public int GemCount { get; set; }
//     [NotNull]
//     public double GemValue { get; set; }
// }

// [Table("feature_coin_with_redcoin")]
// public class FeatureCoinWithRedCoinModel : FeatureModel
// {
//     [NotNull]
//     public int CoinCount { get; set; }

//     [NotNull]
//     public double CoinValue { get; set; }
// }

// [Table("feature_coin_without_redcoin")]
// public class FeatureCoinWithoutRedCoinModel : FeatureModel
// {
//     [NotNull]
//     public int CoinCount { get; set; }

//     [NotNull]
//     public double CoinValue { get; set; }
// }

// [Table("feature_red_coin")]
// public class FeatureRedCoinModel : FeatureModel
// {
//     [NotNull]
//     public int RedCoinCount { get; set; }
// }

// [Table("feature_respins")]
// public class FeatureRespinsModel : FeatureModel
// {
//     [NotNull]
//     public int RespinCount { get; set; }
// }

// [Table("feature_respins_coin")]
// public class FeatureRespinsCoinModel : FeatureModel
// {
//     [NotNull]
//     public int RespinCreateCoinCount { get; set; }

//     [NotNull]
//     public double RespinObtainCoinValue { get; set; }
// }
