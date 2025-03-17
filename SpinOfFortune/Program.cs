using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;


namespace SpinOfFortune;

public class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<Application>();
        var serviceProvider = services.BuildServiceProvider();

        Application app = serviceProvider.GetRequiredService<Application>();
        await app.RunAsync();
    }
}

public class Application
{
    private readonly IConfiguration configuration;
#if DEBUG
    private const int TOTAL_ITERATIONS = 100_000_000;
    private static readonly int THREAD_COUNT = 1;
    private const int BATCH_SIZE = 500_000;
#else
    private const int TOTAL_ITERATIONS = 312_500_000;
    private static readonly int THREAD_COUNT = Environment.ProcessorCount;
    private const int BATCH_SIZE = 500_000;
#endif

    public Application(IConfiguration conf)
    {
        this.configuration = conf;
    }

    public async Task RunAsync()
    {
        var sw = new Stopwatch();
        sw.Start();

        Console.WriteLine($"Starting simulation with {THREAD_COUNT} threads");
        Console.WriteLine($"Total spins: {TOTAL_ITERATIONS:N0}");
        Console.WriteLine("Progress: ");

        var game = new SpinOfFortune(configuration);

        // 진행상황 출력을 위한 타이머
        var progressTimer = new Timer(_ =>
        {
            var currentProgress = Interlocked.Read(ref game.Progress);  // game 인스턴스의 progress 참조
            var percentage = (double)currentProgress / TOTAL_ITERATIONS * 100;
            var elapsed = sw.ElapsedMilliseconds / 1000.0;
            var spinsPerSec = currentProgress / elapsed;
            var currentMemory = GC.GetTotalMemory(false) / 1024 / 1024;
            Console.WriteLine($"{percentage:F2}% ({currentProgress:N0} spins, {spinsPerSec:N0} spins/sec) / [메모리] 현재 사용량: {currentMemory}MB");
        }, null, 0, 4000);

        game.Run(TOTAL_ITERATIONS, BATCH_SIZE, THREAD_COUNT);

        progressTimer.Dispose();

        sw.Stop();

        game.PrintSymbolDistribution();
        game.PrintPayWinResult(TOTAL_ITERATIONS);

        var totalSeconds = sw.ElapsedMilliseconds / 1000.0;
        var avgSpinsPerSec = TOTAL_ITERATIONS / totalSeconds;

        Console.WriteLine();
        Console.WriteLine($"Simulation completed in {totalSeconds:F2} seconds");
        Console.WriteLine($"Average speed: {avgSpinsPerSec:N0} spins/sec");
        Console.WriteLine($"Total elapsed time: {sw.ElapsedMilliseconds:N0}ms");

        await Task.CompletedTask;
    }
}