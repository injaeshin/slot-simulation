using System.Data;
using System.Runtime.CompilerServices;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using Microsoft.Data.Sqlite;

namespace LottaCashMummy.Database;

public interface IDbRepository
{
    void UpsertBaseGame(BaseStats spinStats);
    void UpsertFeatureGame(FeatureStats spinStats);

    long GetTotalPayWinAmount(byte symType, int hit);
    long GetTotalFeatureLevelCount(FeatureBonusType bonusType, int initGemCount, int level);
    long GetTotalFeatureSpinCount(FeatureBonusType bonusType, int initGemCount, int level);
    long GetTotalFeatureGemCount(FeatureBonusType bonusType, int initGemCount, int level);
    double GetTotalFeatureGemValue(FeatureBonusType bonusType, int initGemCount, int level);
    long GetTotalFeatureCoinWithRedCount(FeatureBonusType bonusType, int initGemCount, int level);
    double GetTotalFeatureCoinWithRedValue(FeatureBonusType bonusType, int initGemCount, int level);
    long GetTotalFeatureCoinNoRedCount(FeatureBonusType bonusType, int initGemCount, int level);
    double GetTotalFeatureCoinNoRedValue(FeatureBonusType bonusType, int initGemCount, int level);
}

public class DbRepository : IDbRepository, IDisposable
{
    private readonly DbConnectionPool connectionPool;

    public DbRepository(DbConnectionPool connectionPool)
    {
        this.connectionPool = connectionPool;
    }

    /// <summary>
    /// 데이터베이스 리포지토리를 생성합니다.
    /// </summary>
    /// <param name="dbPath">데이터베이스 파일 경로</param>
    /// <param name="useInMemory">메모리 데이터베이스 사용 여부</param>
    /// <param name="poolSize">연결 풀 크기</param>
    /// <param name="recreateDatabase">데이터베이스 파일을 새로 생성할지 여부</param>
    /// <returns>DbRepository 인스턴스</returns>
    public static DbRepository Create(string dbPath, bool useInMemory = false, int poolSize = 50, bool recreateDatabase = false)
    {
        string connectionString;
        if (useInMemory)
        {
            connectionString = DbConnectionPool.CreateSharedMemoryConnectionString();
        }
        else
        {
            connectionString = DbConnectionPool.CreateLocalFileConnectionString(dbPath);
        }

        var connectionPool = new DbConnectionPool(connectionString, poolSize, true, recreateDatabase);
        return new DbRepository(connectionPool);
    }

    public void Dispose()
    {
        connectionPool?.Dispose();
    }

    public void UpsertBaseGame(BaseStats stats)
    {
        UpsertBaseSpinCount(stats.SpinCount);
        BulkUpsertBasePayWin(stats.PayData);
    }

    private void UpsertBaseSpinCount(int spinCount)
    {
        using var conn = connectionPool.GetConnection();
        using var transaction = conn.Connection.BeginTransaction();

        try
        {
            // 1. 먼저 UPDATE 시도
            var updateSql = @"
                UPDATE base_spin 
                SET spin_count = spin_count + 1";

            int affectedRows = conn.Execute(updateSql, transaction: transaction);
            // 2. 영향받은 행이 없으면 INSERT 실행
            if (affectedRows == 0)
            {
                var insertSql = @"
                    INSERT INTO base_spin (spin_count) 
                    VALUES (1)";

                conn.Execute(insertSql, transaction);
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
        BulkUpsertData(
            payData,
            "base_payout",
            new[] { "symbol_type", "hit" },
            new[] { "amount" },
            new[] { "amount" });
    }

    /// <summary>
    /// 데이터를 벌크 UPSERT하는 일반화된 함수
    /// </summary>
    /// <typeparam name="TKey">키 타입</typeparam>
    /// <typeparam name="TValue">값 타입</typeparam>
    /// <param name="data">업서트할 데이터 딕셔너리</param>
    /// <param name="tableName">테이블 이름</param>
    /// <param name="keyColumns">키 컬럼 이름 배열</param>
    /// <param name="valueColumns">값 컬럼 이름 배열</param>
    /// <param name="paramNames">파라미터 이름 배열</param>
    private void BulkUpsertData<TKey, TValue>(
        Dictionary<TKey, TValue> data,
        string tableName,
        string[] keyColumns,
        string[] valueColumns,
        string[] paramNames) where TKey : notnull
    {
        if (data.Count == 0)
            return;

        using var conn = connectionPool.GetConnection();
        using var transaction = conn.BeginTransaction();

        try
        {
            var valuesList = new List<string>();
            var parameters = new List<object>();
            int paramIndex = 0;

            foreach (var entry in data)
            {
                var paramList = new List<string>();

                // 키 파라미터 처리
                if (entry.Key is ValueTuple<byte, int> tupleKey)
                {
                    string keyParam1 = $"@{keyColumns[0]}{paramIndex}";
                    string keyParam2 = $"@{keyColumns[1]}{paramIndex}";
                    
                    paramList.Add(keyParam1);
                    paramList.Add(keyParam2);
                    
                    parameters.Add(new SqliteParameter(keyParam1, DbType.Byte) { Value = tupleKey.Item1 });
                    parameters.Add(new SqliteParameter(keyParam2, DbType.Int32) { Value = tupleKey.Item2 });
                }
                else if (entry.Key is ValueTuple<FeatureBonusType, int, int> featureKey)
                {
                    string keyParam1 = $"@{keyColumns[0]}{paramIndex}";
                    string keyParam2 = $"@{keyColumns[1]}{paramIndex}";
                    string keyParam3 = $"@{keyColumns[2]}{paramIndex}";
                    
                    paramList.Add(keyParam1);
                    paramList.Add(keyParam2);
                    paramList.Add(keyParam3);
                    
                    parameters.Add(new SqliteParameter(keyParam1, DbType.Int32) { Value = featureKey.Item1 });
                    parameters.Add(new SqliteParameter(keyParam2, DbType.Int32) { Value = featureKey.Item2 });
                    parameters.Add(new SqliteParameter(keyParam3, DbType.Int32) { Value = featureKey.Item3 });
                }
                
                // 값 파라미터 처리 - 여러 값 컬럼 지원
                if (valueColumns.Length == 1)
                {
                    // 단일 값 컬럼 처리 (기존 방식)
                    string valueParam = $"@{paramNames[0]}{paramIndex}";
                    paramList.Add(valueParam);
                    
                    if (entry.Value is long longValue)
                    {
                        parameters.Add(new SqliteParameter(valueParam, DbType.Int64) { Value = longValue });
                    }
                    else if (entry.Value is int intValue)
                    {
                        parameters.Add(new SqliteParameter(valueParam, DbType.Int32) { Value = intValue });
                    }
                    else if (entry.Value is double doubleValue)
                    {
                        parameters.Add(new SqliteParameter(valueParam, DbType.Double) { Value = doubleValue });
                    }
                    else
                    {
                        parameters.Add(new SqliteParameter(valueParam, DbType.Object) { Value = entry.Value });
                    }
                }
                else if (entry.Value is ITuple tuple)
                {
                    // 여러 값 컬럼 처리 (튜플 사용)
                    for (int i = 0; i < Math.Min(valueColumns.Length, tuple.Length); i++)
                    {
                        string valueParam = $"@{paramNames[i]}{paramIndex}";
                        paramList.Add(valueParam);
                        
                        var value = tuple[i];
                        DbType dbType = GetDbType(value);
                        parameters.Add(new SqliteParameter(valueParam, dbType) { Value = value });
                    }
                }
                
                valuesList.Add($"({string.Join(", ", paramList)})");
                paramIndex++;
            }
            
            // DbType 결정 헬퍼 메서드
            DbType GetDbType(object? value)
            {
                return value switch
                {
                    long => DbType.Int64,
                    int => DbType.Int32,
                    byte => DbType.Byte,
                    double => DbType.Double,
                    float => DbType.Single,
                    decimal => DbType.Decimal,
                    string => DbType.String,
                    bool => DbType.Boolean,
                    DateTime => DbType.DateTime,
                    _ => DbType.Object
                };
            }

            // 모든 컬럼 이름 합치기
            var allColumns = keyColumns.Concat(valueColumns).ToArray();

            // UPDATE SET 부분 생성
            var updateSets = valueColumns.Select(col => $"{col} = {col} + excluded.{col}");

            string sql = $@"
                INSERT INTO {tableName} ({string.Join(", ", allColumns)}) 
                VALUES {string.Join(", ", valuesList)}
                ON CONFLICT({string.Join(", ", keyColumns)}) DO UPDATE SET
                {string.Join(", ", updateSets)}";

            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = sql;

            foreach (SqliteParameter param in parameters)
            {
                cmd.Parameters.Add(param);
            }

            cmd.ExecuteNonQuery();

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
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(amount), 0) FROM base_payout WHERE symbol_type = @symType AND hit = @hit";
        var amount = conn.Query<long>(sql, new { symType, hit }).FirstOrDefault();
        return amount;
    }

    public void UpsertFeatureGame(FeatureStats stats)
    {
        AddFeatureLevel(stats.LevelCount);
        AddFeatureSpinCount(stats.SpinCount);
        AddFeatureGemCount(stats.GemCount);
        AddFeatureGemValue(stats.GemValue);
        // AddFeatureCoinWithRedCount(stats.CoinWithRedCount);
        // AddFeatureCoinWithRedValue(stats.CoinWithRedValue);
        // AddFeatureCoinNoRedCount(stats.CoinNoRedCount);
        // AddFeatureCoinNoRedValue(stats.CoinNoRedValue);
    }

    private void AddFeatureLevel(Dictionary<(FeatureBonusType, int, int), int> levelCount)
    {
        BulkUpsertData(
            levelCount, 
            "feature_level", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "level_count" }, 
            new[] { "level_count" });
    }

    private void AddFeatureSpinCount(Dictionary<(FeatureBonusType, int, int), int> spinCount)
    {
        BulkUpsertData(
            spinCount, 
            "feature_spin", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "spin_count" }, 
            new[] { "spin_count" });
    }
    
    private void AddFeatureGemCount(Dictionary<(FeatureBonusType, int, int), int> gemCount)
    {
        BulkUpsertData(
            gemCount, 
            "feature_gem_count", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "gem_count" }, 
            new[] { "gem_count" });
    }
    
    private void AddFeatureGemValue(Dictionary<(FeatureBonusType, int, int), double> gemValue)
    {
        BulkUpsertData(
            gemValue, 
            "feature_gem_value", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "gem_value" }, 
            new[] { "gem_value" });
    }

    private void AddFeatureCoinWithRedCount(Dictionary<(FeatureBonusType, int, int), int> coinWithRedCount)
    {
        BulkUpsertData(
            coinWithRedCount, 
            "feature_coin_with_red_count", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "coin_count" }, 
            new[] { "coin_count" });
    }

    private void AddFeatureCoinWithRedValue(Dictionary<(FeatureBonusType, int, int), double> coinWithRedValue)
    {
        BulkUpsertData(
            coinWithRedValue, 
            "feature_coin_with_red_value", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "coin_value" }, 
            new[] { "coin_value" });
    }

    private void AddFeatureCoinNoRedCount(Dictionary<(FeatureBonusType, int, int), int> coinNoRedCount)
    {
        BulkUpsertData(
            coinNoRedCount, 
            "feature_coin_without_red_count", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "coin_count" }, 
            new[] { "coin_count" });
    }

    private void AddFeatureCoinNoRedValue(Dictionary<(FeatureBonusType, int, int), double> coinNoRedValue)
    {
        BulkUpsertData(
            coinNoRedValue, 
            "feature_coin_without_red_value", 
            new[] { "bonus_type", "gem", "level" }, 
            new[] { "coin_value" }, 
            new[] { "coin_value" });
    }

    public long GetTotalFeatureLevelCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(level_count), 0) FROM feature_level WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return count;
    }

    public long GetTotalFeatureSpinCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(spin_count), 0) FROM feature_spin WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return count;
    }

    public long GetTotalFeatureGemCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(gem_count), 0) FROM feature_gem_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return count;
    }

    public double GetTotalFeatureGemValue(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(gem_value), 0) FROM feature_gem_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return value;
    }

    public long GetTotalFeatureCoinWithRedCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(coin_count), 0) FROM feature_coin_with_red_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return count;
    }

    public double GetTotalFeatureCoinWithRedValue(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(coin_value), 0) FROM feature_coin_with_red_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return value;
    }

    public long GetTotalFeatureCoinNoRedCount(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(coin_count), 0) FROM feature_coin_without_red_count WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var count = conn.Query<long>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return count;
    }

    public double GetTotalFeatureCoinNoRedValue(FeatureBonusType bonusType, int initGemCount, int level)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "SELECT COALESCE(SUM(coin_value), 0) FROM feature_coin_without_red_value WHERE bonus_type = @bonusType AND gem = @initGemCount AND level = @level";
        var value = conn.Query<double>(sql, new { bonusType, initGemCount, level }).FirstOrDefault();
        return value;
    }
}
