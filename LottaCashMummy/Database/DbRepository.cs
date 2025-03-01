using Dapper;
using Dapper.ColumnMapper;
using LottaCashMummy.Model;
using LottaCashMummy.Common;

namespace LottaCashMummy.Database;

public interface IDbRepository
{
    void AddPayWin(byte symType, int hit, int amount);
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

    public void AddPayWin(byte symType, int hit, int amount)
    {
        using var conn = connectionPool.GetConnection();
        var sql = "INSERT INTO base_game (SymbolType, Hit, Amount) VALUES (@symType, @hit, @amount)";
        conn.Execute(sql, new { symType, hit, amount });
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
