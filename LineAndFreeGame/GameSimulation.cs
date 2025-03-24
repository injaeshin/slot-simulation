using Microsoft.Extensions.Logging;
using System.Diagnostics;

using LineAndFreeGame.Common;
using LineAndFreeGame.Service;
using LineAndFreeGame.ThreadStorage;

namespace LineAndFreeGame;

public class GameSimulation
{
#if DEBUG
    private const long TOTAL_ITERATIONS = 6_272_640_000;
    private static readonly int THREAD_COUNT = 6;
    private const int BATCH_SIZE = 1_500_000;
#else
    private const long TOTAL_ITERATIONS = 6_272_640_000;
    private static readonly int THREAD_COUNT = Environment.ProcessorCount;
    private const int BATCH_SIZE = 500_000;
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

        using var progressReporter = new ProgressReporter(TOTAL_ITERATIONS, sw);
        await SimulateGameAsync(progressReporter);

        PrintResults();

        sw.Stop();
        progressReporter.PrintFinalStats(sw);
    }

    private async Task SimulateGameAsync(ProgressReporter progressReporter)
    {
        var totalBatches = (int)Math.Ceiling(TOTAL_ITERATIONS / (double)BATCH_SIZE);
        var options = new ParallelOptions { MaxDegreeOfParallelism = THREAD_COUNT };

        await Task.Run(() =>
        {
            Parallel.For(0, totalBatches, options, batchIndex =>
            {
                var buffer = this.threadStorage.Value!;
                buffer.Clear();

                var currentBatchSpins = Math.Min(BATCH_SIZE, TOTAL_ITERATIONS - batchIndex * (long)BATCH_SIZE);
                for (long i = 0; i < currentBatchSpins; i++)
                {
                    this.gameService.SimulateSingleSpin(buffer);
                }

                statsService.AddSpinStats(buffer.SpinStats);
                buffer.StatsClear();

                progressReporter.UpdateProgress(currentBatchSpins);
            });
        });
    }

    private void PrintResults()
    {
        Console.WriteLine("PayWinResult");

        var totalWinPay = statsService.GetTotalWinPay();
        var totalBonusPay = statsService.GetTotalBonusPay();
        var totalSpinCount = statsService.GetTotalSpinCount();

        List<SymbolType> symbolTypeOrder = [
            SymbolType.WW, SymbolType.AA, SymbolType.BB, SymbolType.CC, SymbolType.DD, SymbolType.EE, SymbolType.FF,
            SymbolType.GG, SymbolType.HH, SymbolType.II, SymbolType.JJ, SymbolType.SS,
        ];

        foreach (var symbolType in symbolTypeOrder)
        {
            for (int i = 3; i <= 5; i++)
            {
                var amount = statsService.GetBaseGameTotalPayWinAmount(symbolType, i);
                var frequency = amount / (double)totalSpinCount;
                Console.WriteLine($"{symbolType,10} {i,10} {amount,10:N0} {frequency,10:F5}");
            }
        }

        Console.WriteLine();

        Console.WriteLine($"Total spins: {totalSpinCount}");
        Console.WriteLine($"Total win pay: {totalWinPay} / RTP: {totalWinPay / (double)totalSpinCount:F5}");
        Console.WriteLine($"Total bonus pay: {totalBonusPay}");

        //statsService.WriteBBSymbolToFile();
    }

    private void PrintSymbolDistribution() => this.gameService.PrintSymbolDistribution();
}
