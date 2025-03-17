
using Microsoft.Extensions.Configuration;
using SpinOfFortune.Service;
using SpinOfFortune.Shared;
using SpinOfFortune.Statistics;
using SpinOfFortune.ThreadBuffer;

namespace SpinOfFortune;

public class SpinOfFortune
{
    private readonly GameService gameService;
    private readonly StatsService statsService;
    private readonly ThreadLocal<ThreadStorage> tls;
    private long progress;
    public ref long Progress => ref progress;


    public SpinOfFortune(IConfiguration conf)
    {
        tls = new ThreadLocal<ThreadStorage>(() => new ThreadStorage());

        gameService = new GameService(conf);
        statsService = new StatsService();
    }

    public void Run(long totalSpins, int batchSize, int threadCount)
    {
        var totalBatches = (int)Math.Ceiling(totalSpins / (double)batchSize);
        var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

        Parallel.For(0, totalBatches, options, batchIndex =>
        {
            var buffer = this.tls.Value!;
            buffer.Clear();

            var currentBatchSpins = Math.Min(batchSize, totalSpins - batchIndex * (long)batchSize);
            for (long i = 0; i < currentBatchSpins; i++)
            {
                gameService.SimulateSingleSpin(buffer);
            }

            statsService.AddSpinStats(buffer.SpinStats);
            buffer.StatsClear();

            Interlocked.Add(ref progress, currentBatchSpins);
        });
    }

    public void PrintSymbolDistribution()
    {
        var reels = gameService.GetRawReelStrip();

        Console.WriteLine("\n심볼 분포:");
        Console.WriteLine("\t\tReel 1\tReel 2\tReel 3\tReel 4\tReel 5");
        Console.WriteLine(new string('-', 50));

        var symbolCounts = new int[(int)SymbolType.Max, reels.Count];

        // 각 릴의 심볼 카운트
        for (int reelIndex = 0; reelIndex < reels.Count; reelIndex++)
        {
            foreach (var symbol in reels[reelIndex])
            {
                symbolCounts[(int)symbol, reelIndex]++;
            }
        }

        // 심볼별 카운트 출력
        for (int symbolIndex = 1; symbolIndex < (int)SymbolType.Max; symbolIndex++)
        {
            Console.Write($"{(SymbolType)symbolIndex,-8}");
            for (int reelIndex = 0; reelIndex < reels.Count; reelIndex++)
            {
                Console.Write($"\t{symbolCounts[symbolIndex, reelIndex]}");
            }
            Console.WriteLine();
        }

        // Total 출력
        Console.WriteLine(new string('-', 50));
        Console.Write("Total\t");
        for (int reelIndex = 0; reelIndex < reels.Count; reelIndex++)
        {
            Console.Write($"\t{reels[reelIndex].Length}");
        }
        Console.WriteLine();

        // Cycle 계산 및 출력
        long cycle = 1;
        for (int i = 0; i < reels.Count; i++)
        {
            cycle *= reels[i].Length;
        }

        Console.WriteLine($"\t\t\tCycle\t {cycle:N0}");
        Console.WriteLine();
    }

    public void PrintPayWinResult(long totalSpins)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("PayWinResult");
        Console.WriteLine("--------------------------------");

        var totalWinPay = statsService.GetTotalWinPay();
        var totalBonusPay = statsService.GetTotalBonusPay();
        var totalSpinCount = statsService.GetTotalSpinCount();

        List<CombinationPayType> combinationPayTypeOrder = [
            CombinationPayType.Wild2x5x2x, CombinationPayType.Wild2x4x2x, CombinationPayType.Wild2x3x2x, CombinationPayType.Wild2x2x2x,
            CombinationPayType.Three7, CombinationPayType.Three7Bar, CombinationPayType.Three3Bar, CombinationPayType.AnyThree7,
            CombinationPayType.Three2Bar, CombinationPayType.One2xOne5x, CombinationPayType.One2xOne4x, CombinationPayType.One2xOne3x,
            CombinationPayType.Three1Bar, CombinationPayType.Two2x, CombinationPayType.AnyThreeBar, CombinationPayType.AnyOne5x,
            CombinationPayType.AnyOne4x, CombinationPayType.AnyOne3x, CombinationPayType.AnyOne2x
        ];

        foreach (var combinationPayType in combinationPayTypeOrder)
        {
            var amount = statsService.GetBaseGameTotalPayWinAmount(combinationPayType);
            var frequency = amount / (double)totalSpinCount;
            Console.WriteLine($"{combinationPayType,10} {amount,10:N0} {frequency,10:F5}");
        }

        Console.WriteLine("--------------------------------");

        Console.WriteLine($"Total spins: {totalSpinCount}");
        Console.WriteLine($"Total win pay: {totalWinPay}");
        Console.WriteLine($"Total bonus pay: {totalBonusPay}");
        Console.WriteLine($"RTP: {totalWinPay / (double)totalSpinCount:F5}");
    }
}