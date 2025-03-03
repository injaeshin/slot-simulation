using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace LottaCashMummy.Database;

public interface IConnection
{
    void Dispose();
    T QuerySingle<T>(string sql, object? param = null);
    IEnumerable<T> Query<T>(string sql, object? param = null);
    int Execute(string sql, object? param = null);
    int Execute(string sql, object? param = null, IDbTransaction? transaction = null);
    Task<T> QuerySingleAsync<T>(string sql, object? param = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
    Task<int> ExecuteAsync(string sql, object? param = null);

    IDbTransaction BeginTransaction();
    IDbCommand CreateCommand();
}

public class DbConnection : IConnection, IDisposable
{
    private readonly IDbConnection connection;


    public DbConnection(string conn)
    {
        connection = new SqliteConnection(conn);
        connection.Open();
    }

    public void Dispose()
    {
        connection.Dispose();
    }

    public T QuerySingle<T>(string sql, object? param = null)
    {
        return connection.QuerySingle<T>(sql, param);
    }

    public IEnumerable<T> Query<T>(string sql, object? param = null)
    {
        return connection.Query<T>(sql, param);
    }

    public int Execute(string sql, object? param = null)
    {
        return connection.Execute(sql, param);
    }

    public int Execute(string sql, object? param = null, IDbTransaction? transaction = null)
    {
        return connection.Execute(sql, param, transaction);
    }

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null)
    {
        return await connection.QuerySingleAsync<T>(sql, param);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        return await connection.QueryAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        return await connection.ExecuteAsync(sql, param);
    }

    public IDbTransaction BeginTransaction()
    {
        return connection.BeginTransaction();
    }

    public IDbCommand CreateCommand()
    {
        return connection.CreateCommand();
    }
}
