using System.Data;
using System.Runtime.CompilerServices;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Model;
using Microsoft.Data.Sqlite;

namespace LottaCashMummy.Database;

public interface IDbRepository
{
    void UpsertBaseGame(BaseStats spinStats);
    void UpsertFeatureGame(FeatureStats spinStats);

    long GetTotalPayWinAmount(byte symType, int hit);
    (long, long) GetFeatureEnter(FeatureBonusType bonusType, int gem, int level);
    // long GetTotalFeatureLevelCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureSpinCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureGemCount(FeatureBonusType bonusType, int initGemCount, int level);
    // double GetTotalFeatureGemValue(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureCoinCount(FeatureBonusType bonusType, int initGemCount, int level);
    // double GetTotalFeatureCoinValue(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureSplitCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureSpinAdd1SpinCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureRedCoinCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureRespinsCount(FeatureBonusType bonusType, int initGemCount, int level);
    // long GetTotalFeatureRespinsCoinCount(FeatureBonusType bonusType, int initGemCount, int level);
    // double GetTotalFeatureRespinsCoinValue(FeatureBonusType bonusType, int initGemCount, int level);
}

public class DbRepository : IDbRepository, IDisposable
{
    private readonly IConnection connection;

    public DbRepository(IConnection connection)
    {
        this.connection = connection;
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    private void UpsertBaseSpinCount(int spinCount)
    {
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. 먼저 UPDATE 시도
            var updateSql = @"
                UPDATE base_spin 
                SET spin_count = spin_count + 1";

            int affectedRows = connection.Execute(updateSql, transaction: transaction);
            // 2. 영향받은 행이 없으면 INSERT 실행
            if (affectedRows == 0)
            {
                var insertSql = @"
                    INSERT INTO base_spin (spin_count) 
                    VALUES (1)";

                connection.Execute(insertSql, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private void BulkUpsertBasePayWin(Dictionary<(byte, int), long> payData)
    {
        var basePayWins = payData.Select(x => new BasePayWinModel { SymbolType = x.Key.Item1, Hit = x.Key.Item2, Amount = x.Value }).ToList();

        if (basePayWins == null || basePayWins.Count == 0)
            return;

        var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"
            INSERT INTO base_payout 
            (symbol_type, hit, amount) 
            VALUES
            (@SymbolType, @Hit, @Amount)
            ON CONFLICT(symbol_type, hit) DO UPDATE SET
            amount = amount + excluded.amount";

            var parameters = basePayWins.Select(item => new
            {
                SymbolType = item.SymbolType,
                Hit = item.Hit,
                Amount = item.Amount
            }).ToList();

            connection.Execute(sql, parameters, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public long GetTotalPayWinAmount(byte symType, int hit)
    {
        var sql = "SELECT COALESCE(SUM(amount), 0) FROM base_payout WHERE symbol_type = @symType AND hit = @hit";
        var amount = connection.Query<long>(sql, new { symType, hit }).FirstOrDefault();
        return amount;
    }

    // private void BulkUpsertData<TKey, TValue>(
    //     Dictionary<TKey, TValue> data,
    //     string tableName,
    //     string[] keyColumns,
    //     string[] valueColumns,
    //     string[] paramNames) where TKey : notnull
    // {
    //     if (data.Count == 0)
    //         return;

    //     using var conn = connectionPool.GetConnection();
    //     using var transaction = conn.BeginTransaction();

    //     try
    //     {
    //         var valuesList = new List<string>();
    //         var parameters = new List<object>();
    //         int paramIndex = 0;

    //         foreach (var entry in data)
    //         {
    //             var paramList = new List<string>();

    //             // 키 파라미터 처리
    //             if (entry.Key is ValueTuple<byte, int> tupleKey)
    //             {
    //                 string keyParam1 = $"@{keyColumns[0]}{paramIndex}";
    //                 string keyParam2 = $"@{keyColumns[1]}{paramIndex}";

    //                 paramList.Add(keyParam1);
    //                 paramList.Add(keyParam2);

    //                 parameters.Add(new SqliteParameter(keyParam1, DbType.Byte) { Value = tupleKey.Item1 });
    //                 parameters.Add(new SqliteParameter(keyParam2, DbType.Int32) { Value = tupleKey.Item2 });
    //             }
    //             else if (entry.Key is ValueTuple<FeatureBonusType, int, int> featureKey)
    //             {
    //                 string keyParam1 = $"@{keyColumns[0]}{paramIndex}";
    //                 string keyParam2 = $"@{keyColumns[1]}{paramIndex}";
    //                 string keyParam3 = $"@{keyColumns[2]}{paramIndex}";

    //                 paramList.Add(keyParam1);
    //                 paramList.Add(keyParam2);
    //                 paramList.Add(keyParam3);

    //                 parameters.Add(new SqliteParameter(keyParam1, DbType.Int32) { Value = featureKey.Item1 });
    //                 parameters.Add(new SqliteParameter(keyParam2, DbType.Int32) { Value = featureKey.Item2 });
    //                 parameters.Add(new SqliteParameter(keyParam3, DbType.Int32) { Value = featureKey.Item3 });
    //             }

    //             // 값 파라미터 처리 - 여러 값 컬럼 지원
    //             if (valueColumns.Length == 1)
    //             {
    //                 // 단일 값 컬럼 처리 (기존 방식)
    //                 string valueParam = $"@{paramNames[0]}{paramIndex}";
    //                 paramList.Add(valueParam);

    //                 if (entry.Value is long longValue)
    //                 {
    //                     parameters.Add(new SqliteParameter(valueParam, DbType.Int64) { Value = longValue });
    //                 }
    //                 else if (entry.Value is int intValue)
    //                 {
    //                     parameters.Add(new SqliteParameter(valueParam, DbType.Int32) { Value = intValue });
    //                 }
    //                 else if (entry.Value is double doubleValue)
    //                 {
    //                     parameters.Add(new SqliteParameter(valueParam, DbType.Double) { Value = doubleValue });
    //                 }
    //                 else
    //                 {
    //                     parameters.Add(new SqliteParameter(valueParam, DbType.Object) { Value = entry.Value });
    //                 }
    //             }
    //             else if (entry.Value is ITuple tuple)
    //             {
    //                 // 여러 값 컬럼 처리 (튜플 사용)
    //                 for (int i = 0; i < Math.Min(valueColumns.Length, tuple.Length); i++)
    //                 {
    //                     string valueParam = $"@{paramNames[i]}{paramIndex}";
    //                     paramList.Add(valueParam);

    //                     var value = tuple[i];
    //                     DbType dbType = GetDbType(value);
    //                     parameters.Add(new SqliteParameter(valueParam, dbType) { Value = value });
    //                 }
    //             }

    //             valuesList.Add($"({string.Join(", ", paramList)})");
    //             paramIndex++;
    //         }

    //         // DbType 결정 헬퍼 메서드
    //         DbType GetDbType(object? value)
    //         {
    //             return value switch
    //             {
    //                 long => DbType.Int64,
    //                 int => DbType.Int32,
    //                 byte => DbType.Byte,
    //                 double => DbType.Double,
    //                 float => DbType.Single,
    //                 decimal => DbType.Decimal,
    //                 string => DbType.String,
    //                 bool => DbType.Boolean,
    //                 DateTime => DbType.DateTime,
    //                 _ => DbType.Object
    //             };
    //         }

    //         // 모든 컬럼 이름 합치기
    //         var allColumns = keyColumns.Concat(valueColumns).ToArray();

    //         // UPDATE SET 부분 생성
    //         var updateSets = valueColumns.Select(col => $"{col} = {col} + excluded.{col}");

    //         string sql = $@"
    //             INSERT INTO {tableName} ({string.Join(", ", allColumns)}) 
    //             VALUES {string.Join(", ", valuesList)}
    //             ON CONFLICT({string.Join(", ", keyColumns)}) DO UPDATE SET
    //             {string.Join(", ", updateSets)}";

    //         using var cmd = conn.CreateCommand();
    //         cmd.Transaction = transaction;
    //         cmd.CommandText = sql;

    //         foreach (SqliteParameter param in parameters)
    //         {
    //             cmd.Parameters.Add(param);
    //         }

    //         cmd.ExecuteNonQuery();

    //         transaction.Commit();
    //     }
    //     catch
    //     {
    //         transaction.Rollback();
    //         throw;
    //     }
    // }

    public void UpsertBaseGame(BaseStats stats)
    {
        UpsertBaseSpinCount(stats.SpinCount);
        BulkUpsertBasePayWin(stats.PayData);
    }

    public void UpsertFeatureGame(FeatureStats stats)
    {
        AddFeatureLevel(stats.LevelCount);
        //AddFeatureSpinCount(stats.SpinCount);
        //AddFeatureGemCount(stats.GemCount);
        //AddFeatureGemValue(stats.GemValue);
        //AddFeatureCoinCount(stats.CoinCount);
        //AddFeatureCoinValue(stats.CoinValue);
        //AddFeatureSplitCount(stats.SplitCount);
        //AddFeatureSpinAdd1SpinCount(stats.SpinAdd1SpinCount);
        //AddFeatureRedCoinCount(stats.RedCoinCount);
        //AddFeatureRespinsCount(stats.RespinCount);
        //AddFeatureRespinsCoinCount(stats.RespinCoinCount);
        //AddFeatureRespinsCoinValue(stats.RespinCoinValue);
    }

    private void AddFeatureLevel(Dictionary<(FeatureBonusType, int, int), Enter> enter)
    {
        //foreach (var e in enter)
        //{
        //    if (e.Key.Item3 < 3)
        //        Console.WriteLine($"BonusType: {e.Key.Item1}, Gem: {e.Key.Item2}, Level: {e.Key.Item3}, EnterCount: {e.Value.EnterCount}, SpinCount: {e.Value.SpinCount}");
        //}

        var featureEnter = enter.Select(x => new FeatureEnter
        {
            BonusType = (int)x.Key.Item1,
            Gem = x.Key.Item2,
            Level = x.Key.Item3,
            EnterCount = x.Value.EnterCount,
            SpinCount = x.Value.SpinCount
        }).ToList();

        var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"
            INSERT INTO feature_enter (bonus_type, gem, level, enter_count, spin_count)
            VALUES (@BonusType, @Gem, @Level, @EnterCount, @SpinCount)
            ON CONFLICT(bonus_type, gem, level) DO UPDATE SET
            enter_count = enter_count + excluded.enter_count,
            spin_count = spin_count + excluded.spin_count";

            var parameters = featureEnter.Select(item => new
            {
                BonusType = item.BonusType,
                Gem = item.Gem,
                Level = item.Level,
                EnterCount = item.EnterCount,
                SpinCount = item.SpinCount
            }).ToList();

            connection.Execute(sql, parameters, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public (long, long) GetFeatureEnter(FeatureBonusType bonusType, int gem, int level)
    {
        var sql = @"SELECT COALESCE(SUM(enter_count), 0), COALESCE(SUM(spin_count), 0) FROM feature_enter
                    WHERE bonus_type = @bonusType AND gem = @gem AND level = @level";
        var result = connection.Query<(long, long)>(sql, new { bonusType, gem, level }).FirstOrDefault();
        return result;
    }
}

    // private void AddFeatureLevel(Dictionary<(FeatureBonusType, int, int), int> levelCount)
    // {
    //     BulkUpsertData(
    //         levelCount,
    //         "feature_level",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "level_count" },
    //         new[] { "level_count" });
    // }

    // private void AddFeatureSpinCount(Dictionary<(FeatureBonusType, int, int), int> spinCount)
    // {
    //     BulkUpsertData(
    //         spinCount,
    //         "feature_spin",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "spin_count" },
    //         new[] { "spin_count" });
    // }

    // private void AddFeatureGemCount(Dictionary<(FeatureBonusType, int, int), int> gemCount)
    // {
    //     BulkUpsertData(
    //         gemCount,
    //         "feature_gem_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "gem_count" },
    //         new[] { "gem_count" });
    // }

    // private void AddFeatureGemValue(Dictionary<(FeatureBonusType, int, int), double> gemValue)
    // {
    //     BulkUpsertData(
    //         gemValue,
    //         "feature_gem_value",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "gem_value" },
    //         new[] { "gem_value" });
    // }

    // private void AddFeatureCoinCount(Dictionary<(FeatureBonusType, int, int), int> coinCount)
    // {
    //     BulkUpsertData(
    //         coinCount,
    //         "feature_coin_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "coin_count" },
    //         new[] { "coin_count" });
    // }

    // private void AddFeatureCoinValue(Dictionary<(FeatureBonusType, int, int), double> coinValue)
    // {
    //     BulkUpsertData(
    //         coinValue,
    //         "feature_coin_value",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "coin_value" },
    //         new[] { "coin_value" });
    // }

    // private void AddFeatureSplitCount(Dictionary<(FeatureBonusType, int, int), int> splitCount)
    // {
    //     BulkUpsertData(
    //         splitCount,
    //         "feature_split_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "split_count" },
    //         new[] { "split_count" });
    // }

    // private void AddFeatureSpinAdd1SpinCount(Dictionary<(FeatureBonusType, int, int), int> spinAdd1SpinCount)
    // {
    //     BulkUpsertData(
    //         spinAdd1SpinCount,
    //         "feature_spin_add1_spin_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "spin_add1_spin_count" },
    //         new[] { "spin_add1_spin_count" });
    // }

    // private void AddFeatureRedCoinCount(Dictionary<(FeatureBonusType, int, int), int> redCoinCount)
    // {
    //     BulkUpsertData(
    //         redCoinCount,
    //         "feature_red_coin_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "red_coin_count" },
    //         new[] { "red_coin_count" });
    // }

    // private void AddFeatureRespinsCount(Dictionary<(FeatureBonusType, int, int), int> respinsCount)
    // {
    //     BulkUpsertData(
    //         respinsCount,
    //         "feature_respins_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "respins_count" },
    //         new[] { "respins_count" });
    // }

    // private void AddFeatureRespinsCoinCount(Dictionary<(FeatureBonusType, int, int), int> respinsCoinCount)
    // {
    //     BulkUpsertData(
    //         respinsCoinCount,
    //         "feature_respins_coin_count",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "respins_coin_count" },
    //         new[] { "respins_coin_count" });
    // }

    // private void AddFeatureRespinsCoinValue(Dictionary<(FeatureBonusType, int, int), double> respinsCoinValue)
    // {
    //     BulkUpsertData(
    //         respinsCoinValue,
    //         "feature_respins_coin_value",
    //         new[] { "bonus_type", "gem", "level" },
    //         new[] { "respins_coin_value" },
    //         new[] { "respins_coin_value" });
    // }

    // public long GetTotalFeatureLevelCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(level_count), 0) FROM feature_level WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureSpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(spin_count), 0) FROM feature_spin WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureGemCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(gem_count), 0) FROM feature_gem_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public double GetTotalFeatureGemValue(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(gem_value), 0) FROM feature_gem_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return value;
    // }

    // public long GetTotalFeatureCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(coin_count), 0) FROM feature_coin_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public double GetTotalFeatureCoinValue(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(coin_value), 0) FROM feature_coin_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return value;
    // }

    // public long GetTotalFeatureSplitCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(split_count), 0) FROM feature_split_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureSpinAdd1SpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(spin_add1_spin_count), 0) FROM feature_spin_add1_spin_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureRedCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(red_coin_count), 0) FROM feature_red_coin_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureRespinsCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(respins_count), 0) FROM feature_respins_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public long GetTotalFeatureRespinsCoinCount(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    //     var sql = "SELECT COALESCE(SUM(respins_coin_count), 0) FROM feature_respins_coin_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    //     var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return count;
    // }

    // public double GetTotalFeatureRespinsCoinValue(FeatureBonusType bonusType, int initGemCount, int level)
    // {
    //     using var conn = connectionPool.GetConnection();
    // var sql = "SELECT COALESCE(SUM(respins_coin_value), 0) FROM feature_respins_coin_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
    // var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
    //     return value;
    // }

