using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Game;

namespace LottaCashMummy;

public class LottaCashMummy
{
    private readonly IBaseData baseData;
    private readonly IFeatureData featureData;
    private readonly IJackpotData jackpotData;

    private readonly BaseService baseService;

    private readonly ThreadLocal<ThreadLocalStorage> tls;

    private long progress;

    public LottaCashMummy(IBaseData baseData, IFeatureData featureData, IJackpotData jackpotData)
    {
        this.baseData = baseData;
        this.featureData = featureData;
        this.jackpotData = jackpotData;

        baseService = new BaseService(baseData, featureData, jackpotData);
        tls = new ThreadLocal<ThreadLocalStorage>(() => new ThreadLocalStorage());
    }

    public ref long Progress => ref progress;  // 진행 상황을 외부에서 참조할 수 있도록

    public void Run(long totalSpins, int threadCount = 0)
    {
        const int BATCH_SIZE = 20000;

        var config = new SimulationStats();
        var totalBatches = (int)Math.Ceiling(totalSpins / (double)BATCH_SIZE);
        if (threadCount <= 0) threadCount = Environment.ProcessorCount;

        var options = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

        Parallel.For(0, totalBatches, options, batchIndex =>
        {
            var buffer = this.tls.Value!;
            var currentBatchSpins = CalculateBatchSpins(totalSpins, batchIndex, BATCH_SIZE);

            for (long i = 0; i < currentBatchSpins; i++)
            {
                baseService.SimulateSingleSpin(buffer);
            }

            Interlocked.Add(ref progress, currentBatchSpins);
            config.MergeFromBuffer(buffer);
        });

        //PrintPayWinResult(totalSpins, config);
        PrintFeatureEnterStats(config);
        //CalculateAndPrintGemProbabilities(config);

        //PrintFeatureTestStats(config);
    }

    private static long CalculateBatchSpins(long totalSpins, int batchIndex, int batchSize)
    {
        return Math.Min(batchSize, totalSpins - batchIndex * (long)batchSize);
    }

    // private static void PrintPayWinResult(long totalSpins, SimulationStats state)
    // {
    //     Console.WriteLine();
    //     Console.WriteLine("Base Game Symbol Win Results:");
    //     Console.WriteLine($"Total Spins: {totalSpins:N0}\n");

    //     Console.WriteLine("Symbol    Count      Probability");
    //     Console.WriteLine(new string('-', 40));

    //     var symbolOrder = new[] {
    //         SymbolType.Wild,
    //         SymbolType.M1, SymbolType.M2, SymbolType.M3, SymbolType.M4, SymbolType.M5,
    //         SymbolType.L1, SymbolType.L2, SymbolType.L3, SymbolType.L4
    //     };

    //     long totalCount = 0;

    //     // 5개, 4개, 3개 매칭 순서로 출력
    //     for (int hits = 5; hits >= 3; hits--)
    //     {
    //         foreach (var symbolType in symbolOrder)
    //         {
    //             int symbol = (int)symbolType;
    //             var count = state.WinPayTable[symbol, hits];
    //             var probability = (count / (double)totalSpins) * 100; // 확률 계산
    //             Console.WriteLine($"{symbolType} {hits,-8} {count,10:N0} {probability,10:F4}%");
    //             totalCount += count; // 카운트 업데이트
    //         }
    //     }

    //     var totalProbability = (totalCount / (double)totalSpins) * 100;
    //     Console.WriteLine(new string('-', 40));
    //     Console.WriteLine($"Total        {totalCount,10:N0} {totalProbability,10:F4}%");
    // }

    private static void PrintFeatureEnterStats(SimulationStats state)
    {
        Console.WriteLine("\nFeature Enter Statistics:");
        Console.WriteLine(new string('-', 150));

        //                    Console.WriteLine($"{type,-20} {gem,10} {level,10}" +
                        // $" {levelUpCount,15:N0} {spinCount,15:N0} {redCoinCount,15:N0} {respinCount,15:N0} {plusSpinCount,15:N0}" +
                        // $" {createGemCount,15:N0} {gemAmount,15:N0} {createCoinCountA,15:N0} {createCoinCountB,15:N0} {coinAmountA,15:N0} {coinAmountB,15:N0}");

        var header = String.Format(
            "{0,-20} {1,10} {2,10} {3,15} {4,15} {5,15} {6,15} {7,15} {8,15} {9,15} {10,15} {11, 15} {12, 15} {13, 15}",
            "Type", "Gems", "Level", "EnterCnt", "SpinCnt", "RedCoinCnt", "RespinCnt", "PlusSpinCnt", 
            "GemCnt", "GemValue", "CoinCntWRedCoin", "CoinCntNoRedCoin", "CoinValueWRedCoin", "CoinValueNoRedCoin");

        Console.WriteLine(header);
        Console.WriteLine(new string('-', 150));

        var totalEnterCount = state.FeatureStats.Sum(x => x.SpinCounts.GetItems().Sum((KeyValuePair<(int, int), double> y) => y.Value));

        foreach (FeatureBonusType type in BonusTypeConverter.CombiTypeOrder.Keys)
        {
            var featureStats = state.GetFeatureStats(type);
            for (int gem = 1; gem <= 5; gem++)
            {
                for (byte level = 1; level <= 4; level++)
                {
                    var key = (level, gem);
                    //var enterCount = featureStats.SpinCounts.GetValueOrDefault(key);
                    var spinCount = featureStats.SpinCounts.GetValueOrDefault(key);
                    var levelUpCount = featureStats.LevelUpCounts.GetValueOrDefault(key);
                    var redCoinCount = featureStats.RedCoinCount.GetValueOrDefault(key);
                    var respinCount = featureStats.RespinCounts.GetValueOrDefault(key);
                    var plusSpinCount = featureStats.FreeSpinCoinCount.GetValueOrDefault(key);

                    var createCoinCountA = featureStats.CreateCoinCountA.GetValueOrDefault(key);            // with RedCoin
                    var createCoinCountB = featureStats.CreateCoinCountB.GetValueOrDefault(key);            // without RedCoin
                    var coinValueA = featureStats.ObtainCoinValueA.GetValueOrDefault(key);
                    var coinValueB = featureStats.ObtainCoinValueB.GetValueOrDefault(key);

                    var gemCount = featureStats.CreateGemCount.GetValueOrDefault(key);
                    var gemValue = featureStats.ObtainGemValue.GetValueOrDefault(key);

                    //var levelEnterRate = 0.0;
                    // var levelEnterCount = 0.0;
                    // if (level == 1)
                    // {
                    //     levelEnterCount = enterCount;
                    //     //levelEnterRate = enterCount > 0 ? (double)levelUpCount / enterCount * 100 : 0;
                    // }
                    // else
                    // {
                    //     levelEnterCount = featureStats.LevelUpCounts.GetValueOrDefault((level, gem));
                    //     //var prevLevelUpCount = featureStats.LevelUpCounts.GetValueOrDefault((level - 1, gem));
                    //     //levelEnterRate = prevLevelUpCount > 0 ? (double)levelUpCount / prevLevelUpCount * 100 : 0;
                    // }

                    Console.WriteLine($"{type,-20} {gem,10} {level,10}" +
                        $" {levelUpCount,15:N0} {spinCount,15:N0} {redCoinCount,15:N0} {respinCount,15:N0} {plusSpinCount,15:N0}" +
                        $" {gemCount,15:N0} {gemValue,15:N0} {createCoinCountA,15:N0} {createCoinCountB,15:N0} {coinValueA,15:N0} {coinValueB,15:N0}");
                }
            }

            Console.WriteLine(new string('-', 150));
            Console.WriteLine(header);
            Console.WriteLine(new string('-', 150));
        }
    }

    //public void CalculateAndPrintGemProbabilities(SimulationState state)
    //{
    //    Console.WriteLine("\nGem Creation Probability by Level:");
    //    Console.WriteLine(new string('-', 150));
    //    Console.WriteLine(String.Format("{0,-20} {1,10} {2,10} {3,15} {4,15} {5,15}", "Type", "InitGems", "Level", "SpinCount", "GemCount", "AvgGem"));
    //    Console.WriteLine(new string('-', 150));

    //    foreach (FeatureBonusType type in typeOrderWithValue.Keys)
    //    {
    //        for (int gem = 1; gem <= 4; gem++)
    //        {
    //            for (byte level = 1; level <= 4; level++)
    //            {
    //                var key = (type, level, gem);
    //                var spins = state.FeatureSpinCount.GetValueOrDefault(key);
    //                var gems = state.FeatureCreateGemCount.GetValueOrDefault(key);

    //                double probability = (spins > 0) ? (double)gems / spins : 0;

    //                Console.WriteLine($"{type,-20} {gem,10} {level,10} {spins,15:N0} {gems,15:N0} {probability,15:F2}");
    //            }

    //            Console.WriteLine(new string('-', 150));
    //        }

    //        // 타입별 소계 출력
    //        var totalSpinsWithType = state.FeatureSpinCount.Where(x => x.Key.Type == type).Sum(x => x.Value);
    //        var totalGemsWithType = state.FeatureCreateGemCount.Where(x => x.Key.Type == type).Sum(x => x.Value);
    //        double totalProbability = (totalSpinsWithType > 0) ? (double)totalGemsWithType / totalSpinsWithType * 100 : 0;

    //        Console.WriteLine(new string('-', 150));
    //        Console.WriteLine($"SubTotal {type,-35} {totalSpinsWithType,15:N0} {totalGemsWithType,15:N0} {totalProbability,15:F2}");
    //        Console.WriteLine(new string('-', 150));
    //    }

    //    // 전체 확률 계산
    //    var totalSpins = state.FeatureSpinCount.Values.Sum();
    //    var totalGems = state.FeatureCreateGemCount.Values.Sum();
    //    double overallProbability = (totalSpins > 0) ? (double)totalGems / totalSpins * 100 : 0;

    //    Console.WriteLine(new string('-', 110));
    //    Console.WriteLine($"Total {totalSpins,15:N0} {totalGems,15:N0} {overallProbability,15:F2}");
    //    Console.WriteLine(new string('-', 110));
    //}
}

