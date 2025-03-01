using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Database;
using LottaCashMummy.Game;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LottaCashMummy;

public class LottaCashMummy
{
    private readonly IBaseData baseData;
    private readonly IFeatureData featureData;
    private readonly IJackpotData jackpotData;

    private readonly BaseService baseService;

    private readonly ThreadLocal<ThreadLocalStorage> tls;

    private readonly IDbRepository baseRepository;

    private long progress;

    public LottaCashMummy(IBaseData baseData, IFeatureData featureData, IJackpotData jackpotData, IDbRepository baseRepository)
    {
        this.baseData = baseData;
        this.featureData = featureData;
        this.jackpotData = jackpotData;
        this.baseRepository = baseRepository;

        baseService = new BaseService(baseData, featureData, jackpotData);
        tls = new ThreadLocal<ThreadLocalStorage>(() => new ThreadLocalStorage(baseRepository));
    }

    public ref long Progress => ref progress;  // 진행 상황을 외부에서 참조할 수 있도록

    public void Run(long totalSpins, int threadCount = 0)
    {
        const int BATCH_SIZE = 30000;

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
    }

    private static long CalculateBatchSpins(long totalSpins, int batchIndex, int batchSize)
    {
        return Math.Min(batchSize, totalSpins - batchIndex * (long)batchSize);
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

        long totalCount = 0;

        // 5개, 4개, 3개 매칭 순서로 출력
        for (int hits = 5; hits >= 3; hits--)
        {
            foreach (var symbolType in symbolOrder)
            {
                byte symbol = (byte)symbolType;
                var count = baseRepository.GetTotalPayWinAmount(symbol, hits);
                var probability = (count / (double)totalSpins) * 100; // 확률 계산
                Console.WriteLine($"{symbolType} {hits,-8} {count,10:N0} {probability,10:F4}%");
                totalCount += count; // 카운트 업데이트
            }
        }

        var totalProbability = (totalCount / (double)totalSpins) * 100;
        Console.WriteLine(new string('-', 40));
        Console.WriteLine($"Total        {totalCount,10:N0} {totalProbability,10:F4}%");
    }

    private static void PrintFeatureEnterStats(SimulationStats state)
    {
        // Console.WriteLine("\nFeature Enter Statistics:");
        // Console.WriteLine(new string('-', 150));

        // var header = String.Format(
        //     "{0,-20} {1,10} {2,10} {3,15} {4,15} {5,15} {6,15} {7,15} {8,15} {9,15} {10,15} {11, 15} {12, 15} {13, 15}",
        //     "Type", "Gems", "Level", "EnterCnt", "SpinCnt", "RedCoinCnt", "RespinCnt", "PlusSpinCnt", 
        //     "GemCnt", "GemValue", "CoinCntWRedCoin", "CoinCntNoRedCoin", "CoinValueWRedCoin", "CoinValueNoRedCoin");

        // Console.WriteLine(header);
        // Console.WriteLine(new string('-', 150));

        // // 배열 기반 접근으로 변경
        // double totalEnterCount = 0;
        // foreach (var featureStat in state.FeatureStats)
        // {
        //     // 모든 레벨과 젬 수에 대해 스핀 카운트 합산
        //     for (int level = 1; level <= 4; level++)
        //     {
        //         for (int gem = 1; gem <= 5; gem++)
        //         {
        //             int index = ArraySlotStats.GetIndex(level, gem);
        //             totalEnterCount += featureStat.SpinCounts[index];
        //         }
        //     }
        // }

        // foreach (FeatureBonusType type in BonusTypeConverter.CombiTypeOrder.Keys)
        // {
        //     var featureStats = state.GetFeatureStats(type);
        //     for (int gem = 1; gem <= 5; gem++)
        //     {
        //         for (byte level = 1; level <= 4; level++)
        //         {
        //             int index = ArraySlotStats.GetIndex(level, gem);
                    
        //             // 배열에서 직접 값 가져오기
        //             var spinCount = featureStats.SpinCounts[index];
        //             var levelUpCount = featureStats.LevelUpCounts[index];
        //             var redCoinCount = featureStats.RedCoinCount[index];
        //             var respinCount = featureStats.RespinCounts[index];
        //             var plusSpinCount = featureStats.FreeSpinCoinCount[index];
        //             var createGemCount = featureStats.CreateGemCount[index];
        //             var gemAmount = featureStats.ObtainGemValue[index];
        //             var createCoinCountA = featureStats.CreateCoinCountA[index];
        //             var createCoinCountB = featureStats.CreateCoinCountB[index];
        //             var coinAmountA = featureStats.ObtainCoinValueA[index];
        //             var coinAmountB = featureStats.ObtainCoinValueB[index];

        //             // 값이 0인 경우 출력하지 않음 - 주석 처리하여 모든 데이터 출력
        //             // if (spinCount == 0) continue;

        //             var enterProbability = (spinCount / totalEnterCount) * 100;

        //             var line = String.Format(
        //                 "{0,-20} {1,10} {2,10} {3,15:N0} {4,15:N0} {5,15:N0} {6,15:N0} {7,15:N0} {8,15:N0} {9,15:N2} {10,15:N0} {11,15:N0} {12,15:N2} {13,15:N2}",
        //                 type, gem, level, levelUpCount, spinCount, redCoinCount, respinCount, plusSpinCount,
        //                 createGemCount, gemAmount, createCoinCountA, createCoinCountB, coinAmountA, coinAmountB);

        //             Console.WriteLine(line);
        //         }
        //     }
        //     Console.WriteLine(header);
        // }

        // Console.WriteLine(new string('-', 150));
        // Console.WriteLine(header);
        // Console.WriteLine(new string('-', 150));

        // // win amount output.
        // var baseWinAmount = state.BaseWinAmount;
        // var featureWinAmount = state.FeatureWinAmount;

        // Console.WriteLine($"BaseWinAmount: {baseWinAmount,15:N0}");
        // Console.WriteLine($"FeatureWinAmount: {featureWinAmount,15:N0}");
    }
}

