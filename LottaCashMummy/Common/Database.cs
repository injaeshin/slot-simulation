namespace LottaCashMummy.Common;
public class DatabaseSettings
{
    public string Type { get; set; } = "Memory";
    public bool KeepAlive { get; set; } = true;
    public bool AutoCreateTables { get; set; } = true;
    public int ConnectionTimeout { get; set; } = 30;
    
    // 추가 속성
    public bool UseInMemoryDatabase => Type.Equals("Memory", StringComparison.OrdinalIgnoreCase);
    public string DatabasePath { get; set; } = "lottacashmummy.db";
    public int ConnectionPoolSize { get; set; } = 50;
}