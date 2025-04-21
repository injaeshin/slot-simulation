using Microsoft.Extensions.Logging;
using System.Diagnostics;

using LineAndFree.Shared;
using LineAndFree.Service;
using LineAndFree.ThreadStorage;

namespace LineAndFree;

public class GameSimulation
{
#if DEBUG
    private const long TOTAL_ITERATIONS = 100_000_000;
    private static readonly int THREAD_COUNT = Environment.ProcessorCount;
    private const int BATCH_SIZE = 1_000_000;
#else
    private const long TOTAL_ITERATIONS = 15_000_000_000;
    private static readonly int THREAD_COUNT = Environment.ProcessorCount;
    private const int BATCH_SIZE = 2_500_000;
#endif

    private readonly ILogger<GameSimulation> logger;
    private readonly IGameService gameService;
    private readonly StatsService statsService;
    private readonly ThreadLocal<ThreadBuffer> threadStorage;

    public GameSimulation(ILogger<GameSimulation> logger, IGameService gameService, StatsService statsService)
    {
        this.logger = logger;
        this.gameService = gameService;
        this.statsService = statsService;
        this.threadStorage = new ThreadLocal<ThreadBuffer>(() => new ThreadBuffer());
    }

    public async Task RunAsync()
    {
        var sw = new Stopwatch();
        sw.Start();

        Console.WriteLine($"Starting simulation with {THREAD_COUNT} threads");
        Console.WriteLine($"Total spins: {TOTAL_ITERATIONS:N0}");
        Console.WriteLine("Progress: ");

        PrintReelStrip();
        PrintSymbolDistribution();

        using (var progressReporter = new ProgressReporter(TOTAL_ITERATIONS, sw))
        {
            await SimulateGameAsync(progressReporter);
            sw.Stop();
            progressReporter.PrintFinalStats(sw);
        }

        Console.WriteLine();
        PrintBaseGameResult();
        PrintFreeGameResult();
    }

    private async Task SimulateGameAsync(ProgressReporter progressReporter)
    {
        var totalBatches = (int)Math.Ceiling(TOTAL_ITERATIONS / (double)BATCH_SIZE);
        var options = new ParallelOptions { MaxDegreeOfParallelism = THREAD_COUNT };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, totalBatches), options, async (batchIndex, ct) =>
            {
                var buffer = this.threadStorage.Value!;
                var currentBatchSpins = Math.Min(BATCH_SIZE, TOTAL_ITERATIONS - batchIndex * (long)BATCH_SIZE);
                for (long i = 0; i < currentBatchSpins; i++)
                {
                    await this.gameService.SimulateSingleSpin(buffer);
                }

                statsService.AddSpinStats(buffer.SpinStats);
                buffer.StatsClear();

                progressReporter.UpdateProgress(currentBatchSpins);
            }
        );
    }

    private void PrintBaseGameResult()
    {
        List<int> scatters = [3, 4, 5];
        List<SymbolType> symbolTypeOrder = [
            SymbolType.AA, SymbolType.BB, SymbolType.CC, SymbolType.DD, SymbolType.EE, SymbolType.FF,
            SymbolType.GG, SymbolType.HH, SymbolType.II, SymbolType.JJ,
        ];

        Console.WriteLine();
        Console.WriteLine("PayWinResult - Base Game");
        Console.WriteLine();
        var totalBaseSpinCount = statsService.GetBaseGameTotalSpinCount();
        var totalTriggerCount = statsService.GetBaseGameTotalWinCountWithScatter();

        Console.WriteLine($"Total base spins: {totalBaseSpinCount}");

        long totalBaseWinAmount = 0;
        for (int i = 5; i >= 3; i--)
        {
            foreach (var symbolType in symbolTypeOrder)
            {
                var baseWinPayAmount = statsService.GetBaseGameTotalWinPayAmount(symbolType, i);
                var frequency = totalBaseSpinCount > 0 ? baseWinPayAmount / (double)totalBaseSpinCount : 0;
                Console.WriteLine($"{symbolType,10} {i,10} {baseWinPayAmount,10:N0} {frequency:F5}");

                totalBaseWinAmount += baseWinPayAmount;
            }
        }

        Console.WriteLine($"Total trigger count: {totalTriggerCount}");

        foreach (var scatter in scatters)
        {
            var baseHitsWinWithScatter = statsService.GetBaseGameTotalWinCountWithScatter(scatter);
            var frequency = baseHitsWinWithScatter > 0 ? baseHitsWinWithScatter / (double)totalBaseSpinCount : 0;
            Console.WriteLine($"{scatter,10} {baseHitsWinWithScatter,10:N0} {frequency:F5}");
        }

        var baseGameRTP = (double)totalBaseWinAmount / totalBaseSpinCount;
        Console.WriteLine($"Base Game RTP: {baseGameRTP:F5}");
    }

    private void PrintFreeGameResult()
    {
        List<int> scatters = [3, 4, 5];
        List<SymbolType> symbolTypeOrder = [
            SymbolType.AA, SymbolType.BB, SymbolType.CC, SymbolType.DD, SymbolType.EE, SymbolType.FF,
            SymbolType.GG, SymbolType.HH, SymbolType.II, SymbolType.JJ,
        ];

        Console.WriteLine();
        Console.WriteLine("PayWinResult - Free Game");
        Console.WriteLine();
        var totalFreeSpinCount = statsService.GetFreeGameTotalSpinCount();
        Console.WriteLine($"Total free spins: {totalFreeSpinCount}");

        long totalFreeWinAmount = 0;
        for (int i = 5; i >= 3; i--)
        {
            foreach (var symbolType in symbolTypeOrder)
            {
                var freeWinPayAmount = statsService.GetFreeGameWinPayAmount(symbolType, i);
                var frequency = freeWinPayAmount > 0 ? freeWinPayAmount / (double)totalFreeSpinCount : 0;
                Console.WriteLine($"{symbolType,10} {i,10} {freeWinPayAmount,10:N0} {frequency:F5}");

                totalFreeWinAmount += freeWinPayAmount;
            }
        }

        Console.WriteLine();
        foreach (var scatter in scatters)
        {
            var freeHitsWinWithScatter = statsService.GetFreeGameWinCountWithScatter(scatter);
            var frequency = freeHitsWinWithScatter > 0 ? freeHitsWinWithScatter / (double)totalFreeSpinCount : 0;
            Console.WriteLine($"{scatter,10} {freeHitsWinWithScatter,10:N0} {frequency:F5}");
        }

        var freeRetriggerCountWithScatter = statsService.GetAllFreeGameRetriggerStats();
        foreach (var scatter in scatters)
        {
            var retriggerCount = freeRetriggerCountWithScatter[scatter];
            Console.WriteLine($"Free Game retrigger count with scatter {scatter}");
            foreach (var retrigger in retriggerCount)
            {
                Console.WriteLine($"  Retrigger {retrigger.Key}: {retrigger.Value}");
            }
        }

        // 프리 게임의 일반 심볼 RTP
        var freeGameSymbolRTP = (double)totalFreeWinAmount / totalFreeSpinCount;
        Console.WriteLine($"Free Game Symbol RTP: {freeGameSymbolRTP:F5}");

        // 프리 게임의 RTP
        var totalBaseSpinCount = statsService.GetBaseGameTotalSpinCount();
        var freeGameRTP = (double)totalFreeWinAmount / totalBaseSpinCount;
        Console.WriteLine($"Free Game RTP: {freeGameRTP:F5}");
    }

    private void PrintSymbolDistribution() => this.gameService.PrintSymbolDistribution();

    private void PrintReelStrip() => this.gameService.PrintReelStrip();
}
