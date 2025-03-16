using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

using LottaCashMummy.Common;
using LottaCashMummy.Tests;

namespace LottaCashMummy;

class Program
{
    static async Task Main(string[] args)
    {
        var conn = $"Data Source=file:memdb_{Guid.NewGuid():N}?mode=memory&cache=shared&Pooling=true&Max Pool Size=50;";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<Application>();
        var serviceProvider = services.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();
        // 명령줄 인수로 "test"가 전달되면 테스트 코드 실행
        if (args.Length > 0 && args[0].ToLower() == "test")
        {
            await RunTests(scope.ServiceProvider);
        }
        else
        {
            // 기존 시뮬레이션 실행
            Application app = scope.ServiceProvider.GetRequiredService<Application>();
            await app.RunAsync();
        }
    }

    static async Task RunTests(IServiceProvider serviceProvider)
    {
        Console.WriteLine("테스트 모드로 실행합니다...");

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var filePath = configuration.GetSection("file").Value ?? throw new Exception("Reel strip path not found in configuration");
        var kv = GameDataLoader.Read(filePath) ?? throw new Exception("Failed to load reel strip");

        var featureData = new FeatureData(kv);

        // 젬 심볼 값 분포 테스트 실행
        var symbolValueTest = new FeatureSymbolValueTest(featureData);
        symbolValueTest.RunTest();

        await Task.CompletedTask;
    }
}

public class Application
{
    private readonly IConfiguration configuration;

    //private const int TOTAL_ITERATIONS = 1_000_000_000;
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

        var (lengths, reels) = baseData.BaseReelSet.GetReelStrip(0);
        PrintSymbolDistribution(reels);

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




/*
public class Application
{
    public Application()
    {

    }

    public async Task RunAsync()
    {
        long totalSpins = 312_500_000;
        int threadCount = Environment.ProcessorCount;
        
        // 레벨별 설정
        int[] slotsPerLevel = { 21, 16, 9 };  // 레벨 1, 2, 3의 슬롯 수
        int[] gemsRequiredForNextLevel = { 5, 4, 3 };  // 레벨 1→2, 2→3, 3→4에 필요한 젬 수
        
        Console.WriteLine($"CPU 코어 수: {threadCount}");
        Console.WriteLine($"총 스핀 수: {totalSpins:N0}");
        Console.WriteLine("게임 규칙:");
        Console.WriteLine("- 레벨 1-3까지 시뮬레이션");
        Console.WriteLine("- 레벨별 슬롯 수: 레벨 1(21개), 레벨 2(16개), 레벨 3(9개)");
        Console.WriteLine("- 레벨업 필요 젬 수: 레벨 1→2(5개), 레벨 2→3(4개), 레벨 3→4(3개)");
        Console.WriteLine("시뮬레이션 시작...");
        
        var sw = Stopwatch.StartNew();
        
        // 레벨별 결과를 저장할 배열
        long[] gemsGeneratedByLevel = new long[3];  // 레벨별 생성된 젬 수
        long[] spinsUsedByLevel = new long[3];      // 레벨별 사용된 스핀 수
        long[] levelUpsCompleted = new long[3];     // 레벨별 레벨업 완료 횟수
        
        // 각 스레드가 처리할 스핀 수 계산
        long spinsPerThread = totalSpins / threadCount;
        var tasks = new Task[threadCount];
        
        // 각 스레드별 작업 할당
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t; // 클로저에서 사용하기 위해 로컬 변수로 복사
            long startSpin = t * spinsPerThread;
            long endSpin = (t == threadCount - 1) ? totalSpins : (t + 1) * spinsPerThread;
            long spinsForThisThread = endSpin - startSpin;
            
            tasks[t] = Task.Run(() =>
            {
                // 각 스레드는 자체 Random 인스턴스 사용
                var localRandom = new Random(Guid.NewGuid().GetHashCode());
                
                // 레벨별 로컬 카운터
                long[] localGemsGenerated = new long[3];
                long[] localSpinsUsed = new long[3];
                long[] localLevelUps = new long[3];
                
                // 레벨별 누적 젬 카운터 추가
                long[] accumulatedGems = new long[3];
                
                // 할당된 스핀 처리
                for (long i = 0; i < spinsForThisThread; i++)
                {
                    // 각 레벨별 시뮬레이션 수행
                    for (int level = 0; level < 3; level++)
                    {
                        int slots = slotsPerLevel[level];
                        int gemsRequired = gemsRequiredForNextLevel[level];
                        
                        // 한 스핀에서 생성된 젬 수
                        int gemsInThisSpin = 0;
                        
                        // 슬롯별 젬 생성 확률 체크
                        for (int slot = 0; slot < slots; slot++)
                        {
                            var num = localRandom.Next(0, 100);
                            if (num < 3)  // 3% 확률로 젬 생성
                            {
                                gemsInThisSpin++;
                            }
                        }
                        
                        // 생성된 젬 수 누적
                        localGemsGenerated[level] += gemsInThisSpin;
                        localSpinsUsed[level]++;
                        
                        // 누적 방식으로 레벨업 조건 체크 변경
                        accumulatedGems[level] += gemsInThisSpin;
                        
                        // 누적된 젬이 필요 수에 도달하면 레벨업
                        if (accumulatedGems[level] >= gemsRequired)
                        {
                            localLevelUps[level]++;
                            accumulatedGems[level] -= gemsRequired; // 남은 젬은 이월
                        }
                    }
                    
                    // 진행 상황 주기적 출력 (1천만 스핀마다)
                    if (i > 0 && i % 10_000_000 == 0)
                    {
                        Console.WriteLine($"스레드 {threadId}: {i:N0}/{spinsForThisThread:N0} 스핀 완료");
                    }
                }
                
                // 최종 결과를 원자적으로 합산
                for (int level = 0; level < 3; level++)
                {
                    Interlocked.Add(ref gemsGeneratedByLevel[level], localGemsGenerated[level]);
                    Interlocked.Add(ref spinsUsedByLevel[level], localSpinsUsed[level]);
                    Interlocked.Add(ref levelUpsCompleted[level], localLevelUps[level]);
                }
                
                Console.WriteLine($"스레드 {threadId} 완료");
            });
        }
        
        await Task.WhenAll(tasks);
        sw.Stop();
        
        Console.WriteLine($"\n시뮬레이션 완료: {sw.ElapsedMilliseconds / 1000.0:F2}초 소요");
        Console.WriteLine($"초당 처리된 스핀 수: {totalSpins / (sw.ElapsedMilliseconds / 1000.0):N0}");
        
        // 레벨별 결과 출력 - 가독성 개선
        Console.WriteLine("\n┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                                        레벨별 시뮬레이션 결과                                               │");
        Console.WriteLine("├───────┬──────────┬────────────┬─────────────────┬─────────────────┬─────────────────┬─────────────────┬─────────────────┤");
        Console.WriteLine("│ 레벨  │ 슬롯 수  │ 필요 젬 수 │   생성된 젬 수   │  사용된 스핀 수  │   레벨업 횟수    │  스핀당 젬 수    │  평균 스핀 수    │");
        Console.WriteLine("├───────┼──────────┼────────────┼─────────────────┼─────────────────┼─────────────────┼─────────────────┼─────────────────┤");
        
        for (int level = 0; level < 3; level++)
        {
            int slots = slotsPerLevel[level];
            int gemsRequired = gemsRequiredForNextLevel[level];
            long gems = gemsGeneratedByLevel[level];
            long spins = spinsUsedByLevel[level];
            long levelUps = levelUpsCompleted[level];
            
            // 통계 계산
            double gemsPerSpin = (double)gems / spins;
            double avgSpinsPerLevelUp = (double)spins / levelUps;
            
            // 이론적 확률 계산
            double theoreticalGemsPerSpin = slots * 0.03;
            double theoreticalLevelUpProbability = CalculateLevelUpProbability(slots, gemsRequired, 0.03);
            
            Console.WriteLine($"│ {level+1,-5} │ {slots,-8} │ {gemsRequired,-10} │ {gems,-15:N0} │ {spins,-15:N0} │ {levelUps,-15:N0} │ {gemsPerSpin,-15:F6} │ {avgSpinsPerLevelUp,-15:F2} │");
        }
        
        Console.WriteLine("└───────┴──────────┴────────────┴─────────────────┴─────────────────┴─────────────────┴─────────────────┴─────────────────┘");
        
        // 이론적 기댓값 vs 실제 결과 출력 - 가독성 개선
        Console.WriteLine("\n┌─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                                           이론적 기댓값 vs 실제 결과                                                        │");
        Console.WriteLine("├───────┬──────────┬─────────────────┬─────────────────┬──────────┬──────────────────────┬─────────────────┬──────────┤");
        Console.WriteLine("│ 레벨  │ 슬롯 수  │ 이론적 젬/스핀   │  실제 젬/스핀    │   비율   │  이론적 평균 스핀 수   │ 실제 평균 스핀 수  │   비율   │");
        Console.WriteLine("├───────┼──────────┼─────────────────┼─────────────────┼──────────┼──────────────────────┼─────────────────┼──────────┤");
        
        for (int level = 0; level < 3; level++)
        {
            int slots = slotsPerLevel[level];
            int gemsRequired = gemsRequiredForNextLevel[level];
            long gems = gemsGeneratedByLevel[level];
            long spins = spinsUsedByLevel[level];
            long levelUps = levelUpsCompleted[level];
            
            // 통계 계산
            double gemsPerSpin = (double)gems / spins;
            double avgSpinsPerLevelUp = (double)spins / levelUps;
            
            // 이론적 확률 계산 (누적 방식)
            double theoreticalGemsPerSpin = slots * 0.03;
            double theoreticalAvgSpins = gemsRequired / theoreticalGemsPerSpin;
            
            // 비율 계산
            double gemsRatio = gemsPerSpin / theoreticalGemsPerSpin;
            double spinsRatio = theoreticalAvgSpins / avgSpinsPerLevelUp;
            
            Console.WriteLine($"│ {level+1,-5} │ {slots,-8} │ {theoreticalGemsPerSpin,-15:F6} │ {gemsPerSpin,-15:F6} │ {gemsRatio,-8:F4} │ {theoreticalAvgSpins,-20:F2} │ {avgSpinsPerLevelUp,-15:F2} │ {spinsRatio,-8:F4} │");
        }
        
        Console.WriteLine("└───────┴──────────┴─────────────────┴─────────────────┴──────────┴──────────────────────┴─────────────────┴──────────┘");
        
        // 레벨업에 필요한 평균 스핀 수 출력 (추가)
        Console.WriteLine("\n┌───────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                     레벨업에 필요한 평균 스핀 수                      │");
        Console.WriteLine("├───────┬──────────────────────────┬────────────────────────┬──────────┤");
        Console.WriteLine("│ 레벨  │     이론적 평균 스핀 수   │     실제 평균 스핀 수   │   비율   │");
        Console.WriteLine("├───────┼──────────────────────────┼────────────────────────┼──────────┤");
        
        for (int level = 0; level < 3; level++)
        {
            long levelUps = levelUpsCompleted[level];
            long spins = spinsUsedByLevel[level];
            
            // 이론적 평균 스핀 수 계산 (누적 방식)
            int slots = slotsPerLevel[level];
            int gemsRequired = gemsRequiredForNextLevel[level];
            double theoreticalGemsPerSpin = slots * 0.03;
            double theoreticalAvgSpins = gemsRequired / theoreticalGemsPerSpin;
            
            // 평균 스핀 수 계산
            double actualAvgSpins = (double)spins / levelUps;
            double ratio = theoreticalAvgSpins / actualAvgSpins;
            
            Console.WriteLine($"│ {level+1,-5} │ {theoreticalAvgSpins,-24:F2} │ {actualAvgSpins,-22:F2} │ {ratio,-8:F4} │");
        }
        
        Console.WriteLine("└───────┴──────────────────────────┴────────────────────────┴──────────┘");
        
        await Task.CompletedTask;
    }
    
    // 레벨업 확률 계산 (이항 분포 사용)
    private double CalculateLevelUpProbability(int slots, int gemsRequired, double gemProbability)
    {
        // 누적 방식에서는 스핀당 평균 젬 수를 계산하고, 필요한 젬 수로 나누어 레벨업 확률 계산
        double gemsPerSpin = slots * gemProbability;
        return 1.0 / (gemsRequired / gemsPerSpin);
    }
    
    // 이항 분포 확률 계산 (더 이상 사용하지 않음)
    private double BinomialProbability(int n, int k, double p)
    {
        return BinomialCoefficient(n, k) * Math.Pow(p, k) * Math.Pow(1 - p, n - k);
    }
    
    // 이항 계수 계산
    private double BinomialCoefficient(int n, int k)
    {
        double result = 1;
        
        for (int i = 1; i <= k; i++)
        {
            result *= (n - (k - i));
            result /= i;
        }
        
        return result;
    }
}
*/