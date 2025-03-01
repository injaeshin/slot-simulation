using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace LottaCashMummy.Database;

public interface IDatabaseConnection : IDisposable
{
    IDbConnection Connection { get; }
    T QuerySingle<T>(string sql, object? param = null);
    IEnumerable<T> Query<T>(string sql, object? param = null);
    int Execute(string sql, object? param = null);
    int Execute(string sql, object? param = null, IDbTransaction? transaction = null);

    Task<T> QuerySingleAsync<T>(string sql, object? param = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
    Task<int> ExecuteAsync(string sql, object? param = null);

    IDbTransaction BeginTransaction();
    void Open();
}

public class DbConnection : IDatabaseConnection, IDisposable
{
    private readonly IDbConnection connection;

    public DbConnection(string connectionString)
    {
        this.connection = new SqliteConnection(connectionString);
    }

    public IDbConnection Connection => connection;

    public void Open() => connection.Open();

    public void Dispose() => connection.Dispose();

    public T QuerySingle<T>(string sql, object? param = null)
    {
        return Connection.QuerySingle<T>(sql, param);
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        return Connection.Query<T>(sql, param);
    }

    public int Execute(string sql, object? param = null)
    {
        return Connection.Execute(sql, param);
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return Connection.Execute(sql, param, transaction);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null)
    {
        return await Connection.QuerySingleAsync<T>(sql, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return await Connection.QueryAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        return await Connection.ExecuteAsync(sql, param);
    }

    public IDbTransaction BeginTransaction()
    {
        return Connection.BeginTransaction();
    }
}
