using LottaCashMummy.Common;

namespace LottaCashMummy.Database;

public interface IDbRepository
{
    void AddBaseSpinCount();
    void AddBasePayWin(byte symType, int hit, int amount);
    long GetTotalPayWinAmount(byte symType, int hit);
}

public class DbRepository : IDbRepository, IDisposable
{
    private readonly DbConnectionPool connectionPool;

    public DbRepository(DatabaseSettings settings)
    {
        string connectionString;

        if (settings.UseInMemoryDatabase)
        {
            connectionString = DbConnectionPool.CreateSharedMemoryConnectionString();
        }
        else
        {
            connectionString = $"Data Source={settings.DatabasePath}";
        }

        connectionPool = new DbConnectionPool(connectionString, settings.ConnectionPoolSize, settings.AutoCreateTables);
    }

    public DbRepository(DbConnectionPool connectionPool)
    {
        this.connectionPool = connectionPool;
    }

    public void AddBaseSpinCount()
    {
        using var conn = connectionPool.GetConnection();
        using var transaction = conn.Connection.BeginTransaction();

        try
        {
            // 1. 먼저 UPDATE 시도
            var updateSql = @"
                UPDATE base_spin 
                SET SpinCount = SpinCount + 1";

            int affectedRows = conn.Execute(updateSql, transaction: transaction);
            // 2. 영향받은 행이 없으면 INSERT 실행
            if (affectedRows == 0)
            {
                var insertSql = @"
                    INSERT INTO base_spin (SpinCount) 
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

    public void AddBasePayWin(byte symType, int hit, int amount)
    {
        using var conn = connectionPool.GetConnection();
        conn.Open();
        using var transaction = conn.Connection.BeginTransaction();

        try
        {
            var updateSql = @"
                UPDATE base_payout 
                SET amount = amount + @amount 
                WHERE symbol_type = @symType AND hit = @hit";

            int affectedRows = conn.Execute(updateSql, new { symType, hit, amount }, transaction);
            if (affectedRows == 0)
            {
                var insertSql = @"
                    INSERT INTO base_payout (symbol_type, hit, amount) 
                    VALUES (@symType, @hit, @amount)";

                conn.Execute(insertSql, new { symType, hit, amount }, transaction);
            }
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
        var sql = "SELECT COALESCE(SUM(Amount), 0) FROM base_game WHERE SymbolType = @symType AND Hit = @hit";
        var amount = conn.Query<long>(sql, new { symType, hit }).FirstOrDefault();
        return amount;
    }

    public void Dispose()
    {
        connectionPool?.Dispose();
    }
}
