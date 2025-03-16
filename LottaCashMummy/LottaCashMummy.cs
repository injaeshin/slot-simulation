using System.Diagnostics;
using System.IO.Pipelines;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Game;
using LottaCashMummy.Statistics;
using LottaCashMummy.Statistics.Model;

namespace LottaCashMummy;

public class LottaCashMummy
{
    private readonly IBaseData baseData;
    private readonly IFeatureData featureData;
    private readonly IJackpotData jackpotData;

    private readonly BaseService baseService;
    private readonly StatsService statsService;

    private readonly ThreadLocal<ThreadLocalStorage> tls;

    private long progress;

    public LottaCashMummy(IBaseData baseData, IFeatureData featureData, IJackpotData jackpotData)
    {
        this.baseData = baseData;
        this.featureData = featureData;
        this.jackpotData = jackpotData;
        baseService = new BaseService(baseData, featureData, jackpotData);
        statsService = new StatsService();
        tls = new ThreadLocal<ThreadLocalStorage>(() => new ThreadLocalStorage());
    }

    public ref long Progress => ref progress;  // 진행 상황을 외부에서 참조할 수 있도록

    public void Run(long totalSpins, int batchSize, int threadCount)
    {
        var totalBatches = (int)Math.Ceiling(totalSpins / (double)batchSize);
        var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };
        statsService.SetStatsResults(new StatsResult[totalBatches]);

        Parallel.For(0, totalBatches, options, batchIndex =>
        {
            var buffer = this.tls.Value!;
            var currentBatchSpins = Math.Min(batchSize, totalSpins - batchIndex * (long)batchSize);

            for (long i = 0; i < currentBatchSpins; i++)
            {
                baseService.SimulateSingleSpin(buffer);
            }

            statsService.SetStatsResults(batchIndex, buffer.StatsResult);
            buffer.StatsClear();

            Interlocked.Add(ref progress, currentBatchSpins);
        });
    }

    public void PrintPayWinResult(long totalSpins)
    {
        Console.WriteLine();
        Console.WriteLine("Base Game Symbol Win Results:");
        Console.WriteLine($"Total Spins: {totalSpins:N0}\n");

        Console.WriteLine("Symbol    Count      Probability");
        Console.WriteLine(new string('-', 40));

        var symbolOrder = new[] {
            SymbolType.Wild,
            SymbolType.M1, SymbolType.M2, SymbolType.M3, SymbolType.M4, SymbolType.M5,
            SymbolType.L1, SymbolType.L2, SymbolType.L3, SymbolType.L4
        };

        long totalSpin = statsService.GetBaseGameTotalSpinCount();

        long totalCount = 0;
        // 5개, 4개, 3개 매칭 순서로 출력
        for (int hits = 5; hits >= 3; hits--)
        {
            foreach (var symbolType in symbolOrder)
            {
                byte symbol = (byte)symbolType;
                var count = statsService.GetBaseGameTotalPayWinAmount(symbol, hits);
                var probability = (count / (double)totalSpins) * 100; // 확률 계산
                Console.WriteLine($"{symbolType} {hits,-8} {count,10:N0} {probability,10:F4}%");
                totalCount += count; // 카운트 업데이트
            }
        }

        var totalProbability = (totalCount / (double)totalSpins) * 100;
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"Total    [iteration: {totalSpins,10:N0}  - exec: {totalSpin,10:N0}] {totalCount,10:N0} {totalProbability,10:F4}%");
    }

    public void PrintFeatureLevelStats(long totalSpins)
    {
        Console.WriteLine();
        Console.WriteLine("Feature Level Statistics:");
        Console.WriteLine($"Total Spins: {totalSpins:N0}\n");

        var header = String.Format(
            "{0,-20} {1,10} {2,10} {3,15} {4,15} {5,15} {6,15} {7,15}",
            "Type", "Gems", "Level", "EnterCnt", "SpinCnt", "GemCnt", "GemRate", "GemValue", "GemExpVal");

        Console.WriteLine(header);
        Console.WriteLine(new string('-', 150));

        foreach (FeatureBonusType type in BonusTypeConverter.BonusTypeOrder.Keys)
        {
            var bonusType = BonusTypeConverter.BonusTypeOrder[type];
            for (int gem = 1; gem <= 5; gem++)
            {
                for (byte level = 1; level <= 4; level++)
                {
                    var levelCount = statsService.GetBonusGameTotalLevelCount(bonusType, gem, level);
                    var spinCount = statsService.GetBonusGameTotalGemSpinCount(bonusType, gem, level);
                    var gemCount = statsService.GetBonusGameTotalGemCount(bonusType, gem, level);
                    var gemRate = (double)gemCount / spinCount;
                    var gemValue = statsService.GetBonusGameTotalGemValue(bonusType, gem, level);
                    var gemExpectedValue = gemValue / gemCount;

                    Console.WriteLine($"{type,-20} {gem,10} {level,10} {levelCount,15:N0} {spinCount,15:N0} {gemCount,15:N0} {gemRate,15:F4} {gemValue,15:F4} {gemExpectedValue,15:F4}");
                }
            }
            Console.WriteLine(new string('-', 150));
        }
    }
}