using Microsoft.Extensions.Logging;
using System.Diagnostics;

using LineAndFreeGame.Common;
using LineAndFreeGame.Service;
using LineAndFreeGame.ThreadStorage;

namespace LineAndFreeGame;

public class GameSimulation
{
#if DEBUG
    private const long TOTAL_ITERATIONS = 200_000_000;
    private static readonly int THREAD_COUNT = Environment.ProcessorCount;
    private const int BATCH_SIZE = 1_500_000;
#else
    private const long TOTAL_ITERATIONS = 6_272_640_000;
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

        PrintSymbolDistribution();

        using (var progressReporter = new ProgressReporter(TOTAL_ITERATIONS, sw))
        {
            await SimulateGameAsync(progressReporter);
            sw.Stop();
            progressReporter.PrintFinalStats(sw);
        }

        PrintBaseResults();
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

    private void PrintBaseResults()
    {
        List<SymbolType> symbolTypeOrder = [
            SymbolType.WW, SymbolType.AA, SymbolType.BB, SymbolType.CC, SymbolType.DD, SymbolType.EE, SymbolType.FF,
            SymbolType.GG, SymbolType.HH, SymbolType.II, SymbolType.JJ, SymbolType.SS,
        ];

        Console.WriteLine("PayWinResult - Base Game");

        var totalBaseSpinCount = statsService.GetTotalBaseSpinCount();
        var totalWinPayAmount = statsService.GetTotalBaseGameWinPay();

        for (int i = 5; i >= 3; i--)
        {
            foreach (var symbolType in symbolTypeOrder)
            {
                var amount = statsService.GetLineGameTotalPayWinAmount(symbolType, i);
                var frequency = amount / (double)totalBaseSpinCount;
                Console.WriteLine($"{symbolType,10} {i,10} {amount,10:N0} {frequency,10:F5}");
            }
        }

        Console.WriteLine();

        Console.WriteLine("ScatterCount - Base Game");
        for (int i = 5; i >= 3; i--)
        {
            var hits = statsService.GetLineScatterCount(i);
            var frequency = hits / (double)totalBaseSpinCount;
            Console.WriteLine($"SS - {i,10} {hits,10:N0} {frequency,10:F5}");
        }

        var totalTriggerCount = statsService.GetTotalFreeGameTriggerCount();
        Console.WriteLine($"Total trigger count: {totalTriggerCount:N0} / Frequency: {totalTriggerCount / (double)totalBaseSpinCount:F5}");

        Console.WriteLine($"Total base spins: {totalBaseSpinCount}");
        Console.WriteLine($"Total base win pay: {totalWinPayAmount} / RTP: {totalWinPayAmount / (double)totalBaseSpinCount:F5}");

        Console.WriteLine("\nPayWinResult - Free Game");

        var totalFreeSpinCount = statsService.GetTotalFreeSpinCount();
        var totalFreeWinPayAmount = statsService.GetTotalFreeGameWinPay();

        for (int i = 5; i >= 3; i--)
        {
            foreach (var symbolType in symbolTypeOrder)
            {
                var amount = statsService.GetFreeGameTotalPayWinAmount(symbolType, i);
                var frequency = amount / (double)totalFreeSpinCount;
                Console.WriteLine($"{symbolType,10} {i,10} {amount,10:N0} {frequency,10:F5}");
            }
        }

        Console.WriteLine();

        Console.WriteLine($"Total free spins: {totalFreeSpinCount}");
        Console.WriteLine($"Total free win pay: {totalFreeWinPayAmount} / RTP: {totalFreeWinPayAmount / (double)totalFreeSpinCount:F5}");

        Console.WriteLine();

        // // 평균 스핀 수 (10, 15, 20)
        // var freeGameAvgSpinCount10 = statsService.GetAvgFreeSpinExecutions(10);
        // var freeGameAvgSpinCount15 = statsService.GetAvgFreeSpinExecutions(15);
        // var freeGameAvgSpinCount20 = statsService.GetAvgFreeSpinExecutions(20);

        // Console.WriteLine($"FreeGameAvgSpinCount - initSpin: 10, avgSpinCount: {freeGameAvgSpinCount10}");
        // Console.WriteLine($"FreeGameAvgSpinCount - initSpin: 15, avgSpinCount: {freeGameAvgSpinCount15}");
        // Console.WriteLine($"FreeGameAvgSpinCount - initSpin: 20, avgSpinCount: {freeGameAvgSpinCount20}");

        // 3개의 평균
        //var freeGameAvgSpinCount = (freeGameAvgSpinCount10 + freeGameAvgSpinCount15 + freeGameAvgSpinCount20) / 3;
        //Console.WriteLine($"FreeGameAvgSpinCount - avgSpinCount: {freeGameAvgSpinCount}");
    }

    private void PrintSymbolDistribution() => this.gameService.PrintSymbolDistribution();
}
