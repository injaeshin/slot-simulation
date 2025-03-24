using System.Diagnostics;

using LineAndFreeGame.Common;
using LineAndFreeGame.Service;
using LineAndFreeGame.ThreadStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LineAndFreeGame;

public class GameSimulation
{
#if DEBUG
    private const int TOTAL_ITERATIONS = 100_000_000;
    private static readonly int THREAD_COUNT = 1;
    private const int BATCH_SIZE = 500_000;
#else
    private const int TOTAL_ITERATIONS = 312_500_000;
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

    private void PrintResults() => this.gameService.PrintResults();

    private void PrintSymbolDistribution() => this.gameService.PrintSymbolDistribution();
}
