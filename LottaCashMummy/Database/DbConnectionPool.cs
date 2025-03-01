using System.Collections.Concurrent;
using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;

using LottaCashMummy.Database;


public class DbConnectionPool : IDisposable
{
    private readonly string connectionString;
    private readonly ConcurrentBag<IDatabaseConnection> availableConnections = new();
    private readonly List<IDatabaseConnection> allConnections = new();
    private readonly object objectLock = new();
    private readonly int maxPoolSize;
    private readonly IDbConnection? keepAliveConnection; // 공유 메모리 DB 유지를 위한 연결
    //private readonly bool isSharedMemory;
    
    public DbConnectionPool(string connectionString, int maxPoolSize = 50, bool initializeDatabase = true, bool recreateDatabase = true)
    {
        // 파일 기반 DB이고 recreateDatabase가 true인 경우 기존 파일 삭제
        if (recreateDatabase && !connectionString.Contains("mode=memory") && !connectionString.Contains(":memory:"))
        {
            string? dbFilePath = ExtractDatabasePath(connectionString);
            if (!string.IsNullOrEmpty(dbFilePath) && File.Exists(dbFilePath))
            {
                try
                {
                    File.Delete(dbFilePath);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"데이터베이스 파일 삭제 중 오류 발생: {ex.Message}");
                }
            }
        }

        this.connectionString = connectionString;
        this.maxPoolSize = maxPoolSize;

        //isSharedMemory = connectionString.Contains("mode=memory") || connectionString.Contains(":memory:");
        //if (isSharedMemory)

        keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();

        // 데이터베이스 초기화
        if (initializeDatabase)
        {
            InitializeDatabase();
        }
    }
    
    // 연결 문자열에서 데이터베이스 파일 경로 추출
    private string? ExtractDatabasePath(string connectionString)
    {
        // Data Source=파일경로 형식에서 파일 경로 추출
        var dataSourcePart = connectionString.Split(';')
            .FirstOrDefault(part => part.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase));
        
        if (dataSourcePart != null)
        {
            var path = dataSourcePart.Substring("Data Source=".Length).Trim();
            // file: 접두사 제거
            if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                var questionMarkIndex = path.IndexOf('?');
                if (questionMarkIndex > 0)
                {
                    path = path.Substring(5, questionMarkIndex - 5);
                }
                else
                {
                    path = path.Substring(5);
                }
            }
            return path;
        }
        
        return null;
    }
    
    // 데이터베이스 초기화 (테이블 생성 등)
    private void InitializeDatabase()
    {
        if (keepAliveConnection != null)
        {
            // 기본 테이블 생성
            using var cmd = keepAliveConnection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS base_spin (
                    spin_count INTEGER PRIMARY KEY
                );

                CREATE TABLE IF NOT EXISTS base_payout (
                    symbol_type INTEGER NOT NULL,
                    hit INTEGER NOT NULL,
                    amount BIGINT NOT NULL,
                    PRIMARY KEY (symbol_type, hit)
                );

                CREATE TABLE IF NOT EXISTS feature_level (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    level_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_spin (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    spin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_gem_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    gem_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_gem_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    gem_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_with_red_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_with_red_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_without_red_count (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_count BIGINT NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );

                CREATE TABLE IF NOT EXISTS feature_coin_without_red_value (
                    bonus_type INTEGER NOT NULL,
                    gem INTEGER NOT NULL,
                    level INTEGER NOT NULL,
                    coin_value REAL NOT NULL,
                    PRIMARY KEY (bonus_type, gem, level)
                );
            ";
            cmd.ExecuteNonQuery();
        }
    }
    
    // 공유 메모리 DB 연결 문자열 생성
    public static string CreateSharedMemoryConnectionString()
    {
        string dbName = $"memdb_{Guid.NewGuid():N}";
        return $"Data Source=file:{dbName}?mode=memory&cache=shared&Pooling=true&Max Pool Size=50;";
    }

    public static string CreateLocalFileConnectionString(string dbName)
    {
        return $"Data Source={dbName};Pooling=true;";
    }
    
    public IDatabaseConnection GetConnection()
    {
        if (availableConnections.TryTake(out var connection))
        {
            if (connection.Connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return new PooledConnection(connection, this);
        }
        
        lock (objectLock)
        {
            if (allConnections.Count < maxPoolSize)
            {
                var newConnection = new DbConnection(connectionString);
                newConnection.Open();
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
        public void Open() => _innerConnection.Open();
        public T QuerySingle<T>(string sql, object? param = null) => _innerConnection.QuerySingle<T>(sql, param);
        public IEnumerable<T> Query<T>(string sql, object? param = null) => _innerConnection.Query<T>(sql, param);
        public int Execute(string sql, object? param = null) => _innerConnection.Execute(sql, param);
        public int Execute(string sql, object? param = null, IDbTransaction? transaction = null) => _innerConnection.Execute(sql, param, transaction);
        public IDbTransaction BeginTransaction() => _innerConnection.BeginTransaction();
        public async Task<T> QuerySingleAsync<T>(string sql, object? param = null) => await _innerConnection.QuerySingleAsync<T>(sql, param);
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null) => await _innerConnection.QueryAsync<T>(sql, param);
        public async Task<int> ExecuteAsync(string sql, object? param = null) => await _innerConnection.ExecuteAsync(sql, param);
        public IDbCommand CreateCommand() => _innerConnection.CreateCommand();
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
