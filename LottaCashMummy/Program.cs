using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

using LottaCashMummy.Shared;


namespace LottaCashMummy;

public class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
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

        Console.WriteLine("Initializing symbols...");

        var filePath = configuration.GetSection("file").Value ?? throw new Exception("Reel strip path not found in configuration");
        var kv = GameDataLoader.Read(filePath) ?? throw new Exception("Failed to load reel strip");

        var baseData = new BaseData(kv);
        var featureData = new FeatureData(kv);
        var jackpotData = new JackpotData(kv);

        //var (lengths, reels) = baseData.BaseReelSet.GetReelStrip(0);
        //PrintSymbolDistribution(reels);

        sw.Restart();

        Console.WriteLine($"Starting simulation with {THREAD_COUNT} threads");
        Console.WriteLine($"Total spins: {TOTAL_ITERATIONS:N0}");
        Console.WriteLine("Progress: ");

        // 이미 DI를 통해 주입된 리포지토리 사용
        var game = new LottaCashMummy(baseData, featureData, jackpotData);

        // 진행상황 출력을 위한 타이머
        var progressTimer = new Timer(_ =>
        {
            var currentProgress = Interlocked.Read(ref game.Progress);  // game 인스턴스의 progress 참조
            var percentage = (double)currentProgress / TOTAL_ITERATIONS * 100;
            var elapsed = sw.ElapsedMilliseconds / 1000.0;
            var spinsPerSec = currentProgress / elapsed;

            Console.WriteLine($"{percentage:F2}% ({currentProgress:N0} spins, {spinsPerSec:N0} spins/sec)");
        }, null, 0, 3500);

        // 메모리 모니터링 타이머 추가
        var memoryTimer = new Timer(_ =>
        {
            var currentMemory = GC.GetTotalMemory(false) / 1024 / 1024;
            Console.WriteLine($"[메모리] 현재 사용량: {currentMemory}MB");
        }, null, 0, 5000);  // 5초마다 체크

        game.Run(TOTAL_ITERATIONS, BATCH_SIZE, THREAD_COUNT);

        memoryTimer.Dispose();
        progressTimer.Dispose();

        sw.Stop();

        game.PrintPayWinResult(TOTAL_ITERATIONS);
        game.PrintFeatureLevelStats(TOTAL_ITERATIONS);

        var totalSeconds = sw.ElapsedMilliseconds / 1000.0;
        var avgSpinsPerSec = TOTAL_ITERATIONS / totalSeconds;

        Console.WriteLine();
        Console.WriteLine($"Simulation completed in {totalSeconds:F2} seconds");
        Console.WriteLine($"Average speed: {avgSpinsPerSec:N0} spins/sec");
        Console.WriteLine($"Total elapsed time: {sw.ElapsedMilliseconds:N0}ms");

        await Task.CompletedTask;
    }

    private void PrintSymbolDistribution(byte[][] reels)
    {
        Console.WriteLine("\n심볼 분포:");
        Console.WriteLine("\t\tReel 1\tReel 2\tReel 3\tReel 4\tReel 5");
        Console.WriteLine(new string('-', 50));

        var symbolCounts = new int[(int)SymbolType.Max, reels.Length];

        // 각 릴의 심볼 카운트
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            foreach (var symbol in reels[reelIndex])
            {
                symbolCounts[symbol, reelIndex]++;
            }
        }

        // 심볼별 카운트 출력
        for (int symbolIndex = 1; symbolIndex < (int)SymbolType.Max; symbolIndex++)
        {
            Console.Write($"{(SymbolType)symbolIndex,-8}");
            for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
            {
                Console.Write($"\t{symbolCounts[symbolIndex, reelIndex]}");
            }
            Console.WriteLine();
        }

        // Total 출력
        Console.WriteLine(new string('-', 50));
        Console.Write("Total\t");
        for (int reelIndex = 0; reelIndex < reels.Length; reelIndex++)
        {
            Console.Write($"\t{reels[reelIndex].Length}");
        }
        Console.WriteLine();

        // Cycle 계산 및 출력
        long cycle = 1;
        for (int i = 0; i < reels.Length; i++)
        {
            cycle *= reels[i].Length;
        }

        Console.WriteLine($"\t\t\tCycle\t {cycle:N0}");
        Console.WriteLine();
    }
}