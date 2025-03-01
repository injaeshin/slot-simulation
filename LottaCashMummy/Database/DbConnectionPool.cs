using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

using LottaCashMummy.Database;
using LottaCashMummy.Model;
using LottaCashMummy.Common;

public class DbConnectionPool : IDisposable
{
    private readonly string connectionString;
    private readonly ConcurrentBag<IDatabaseConnection> availableConnections = new();
    private readonly List<IDatabaseConnection> allConnections = new();
    private readonly object objectLock = new();
    private readonly int maxPoolSize;
    private readonly IDbConnection keepAliveConnection; // 공유 메모리 DB 유지를 위한 연결
    private readonly bool isSharedMemory;
    
    public DbConnectionPool(string connectionString, int maxPoolSize = 50, bool initializeDatabase = true)
    {
        this.connectionString = connectionString;
        this.maxPoolSize = maxPoolSize;
        
        isSharedMemory = connectionString.Contains("mode=memory") || connectionString.Contains(":memory:");        
        if (!isSharedMemory)
        {
            throw new Exception("file memory DB is not supported.");
        }

        keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();
        
        // 데이터베이스 초기화
        if (initializeDatabase)
        {
            InitializeDatabase();
        }
    }
    
    // 데이터베이스 초기화 (테이블 생성 등)
    private void InitializeDatabase()
    {
        if (keepAliveConnection != null)
        {
            // 기본 테이블 생성
            using var cmd = keepAliveConnection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS base_game (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SymbolType INTEGER NOT NULL,
                    Hit INTEGER NOT NULL,
                    Amount INTEGER NOT NULL
                )
            ";
            cmd.ExecuteNonQuery();
        }
    }
    
    // 공유 메모리 DB 연결 문자열 생성
    public static string CreateSharedMemoryConnectionString()
    {
        string dbName = $"memdb_{Guid.NewGuid():N}";
        return $"Data Source=file:{dbName}?mode=memory&cache=shared";
    }
    
    public IDatabaseConnection GetConnection()
    {
        if (availableConnections.TryTake(out var connection))
        {
            return new PooledConnection(connection, this);
        }
        
        lock (objectLock)
        {
            if (allConnections.Count < maxPoolSize)
            {
                var newConnection = new DbConnection(connectionString);
                allConnections.Add(newConnection);
                return new PooledConnection(newConnection, this);
            }
        }
        
        // 풀이 가득 찼으면 대기
        SpinWait.SpinUntil(() => availableConnections.TryTake(out connection));
        return new PooledConnection(connection!, this);
    }
    
    public void ReturnConnection(IDatabaseConnection connection)
    {
        availableConnections.Add(connection);
    }
    
    public void Dispose()
    {
        foreach (var connection in allConnections)
        {
            connection.Dispose();
        }
        allConnections.Clear();
        availableConnections.Clear();
        
        // 공유 메모리 DB 연결 정리
        keepAliveConnection?.Dispose();
    }
    
    // 풀에 반환되는 연결을 래핑하는 클래스
    private class PooledConnection : IDatabaseConnection
    {
        private readonly IDatabaseConnection _innerConnection;
        private readonly DbConnectionPool _pool;
        private bool _disposed = false;
        
        public PooledConnection(IDatabaseConnection connection, DbConnectionPool pool)
        {
            _innerConnection = connection;
            _pool = pool;
        }
        
        public IDbConnection Connection => _innerConnection.Connection;
        
        // 모든 메서드는 내부 연결에 위임
        public T QuerySingle<T>(string sql, object? param = null) => _innerConnection.QuerySingle<T>(sql, param);
        public IEnumerable<T> Query<T>(string sql, object? param = null) => _innerConnection.Query<T>(sql, param);
        public int Execute(string sql, object? param = null) => _innerConnection.Execute(sql, param);
        public async Task<T> QuerySingleAsync<T>(string sql, object? param = null) => await _innerConnection.QuerySingleAsync<T>(sql, param);
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null) => await _innerConnection.QueryAsync<T>(sql, param);
        public async Task<int> ExecuteAsync(string sql, object? param = null) => await _innerConnection.ExecuteAsync(sql, param);
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _pool.ReturnConnection(_innerConnection);
                _disposed = true;
            }
        }
    }
}
