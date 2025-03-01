using LottaCashMummy.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Runtime.InteropServices;

namespace LottaCashMummy.Buffer;

// 배열 기반 SlotStats 구현
//public class ArraySlotStats
//{
//    private static readonly int[] LevelsRange = [1, 2, 3, 4];
//    private static readonly int[] InitGemCountsRange = [1, 2, 3, 4, 5];
    
//    // 레벨과 젬 수에 대한 인덱스 매핑 함수
//    public static int GetIndex(int level, int gems)
//    {
//        // 레벨은 1부터 시작, 젬은 1부터 시작
//        int levelIndex = level - 1;
//        int gemsIndex = gems - 1;
//        return levelIndex * InitGemCountsRange.Length + gemsIndex;
//    }
    
//    // 배열 크기 계산 (레벨 수 * 젬 수)
//    private static readonly int ArraySize = LevelsRange.Length * InitGemCountsRange.Length;
    
//    // 각 통계를 위한 배열들
//    private readonly double[] spinCounts = new double[ArraySize];
//    private readonly double[] respinCounts = new double[ArraySize];
//    private readonly double[] levelUpCounts = new double[ArraySize];
//    private readonly double[] createGemCount = new double[ArraySize];
//    private readonly double[] obtainGemValue = new double[ArraySize];
//    private readonly double[] createCoinCountA = new double[ArraySize];
//    private readonly double[] createCoinCountB = new double[ArraySize];
//    private readonly double[] obtainCoinValueA = new double[ArraySize];
//    private readonly double[] obtainCoinValueB = new double[ArraySize];
//    private readonly double[] redCoinCount = new double[ArraySize];
//    private readonly double[] freeSpinCoinCount = new double[ArraySize];
    
//    // 모든 배열을 한 번에 접근할 수 있는 배열
//    private readonly double[][] allArrays;
    
//    public ArraySlotStats()
//    {
//        // 모든 배열을 배열의 배열에 저장
//        allArrays = new double[][]
//        {
//            spinCounts,
//            respinCounts,
//            levelUpCounts,
//            createGemCount,
//            obtainGemValue,
//            createCoinCountA,
//            createCoinCountB,
//            obtainCoinValueA,
//            obtainCoinValueB,
//            redCoinCount,
//            freeSpinCoinCount
//        };
        
//        // 모든 배열 초기화
//        Clear();
//    }
    
//    public void Clear()
//    {
//        // 모든 배열을 한 번에 초기화
//        foreach (var array in allArrays)
//        {
//            Array.Clear(array, 0, array.Length);
//        }
//    }
    
//    // 각 배열에 대한 접근자 속성
//    public double[] SpinCounts => spinCounts;
//    public double[] RespinCounts => respinCounts;
//    public double[] LevelUpCounts => levelUpCounts;
//    public double[] CreateGemCount => createGemCount;
//    public double[] ObtainGemValue => obtainGemValue;
//    public double[] CreateCoinCountA => createCoinCountA;
//    public double[] CreateCoinCountB => createCoinCountB;
//    public double[] ObtainCoinValueA => obtainCoinValueA;
//    public double[] ObtainCoinValueB => obtainCoinValueB;
//    public double[] RedCoinCount => redCoinCount;
//    public double[] FreeSpinCoinCount => freeSpinCoinCount;
    
//    // 인덱서를 통해 배열 배열에 접근
//    public double[] this[int index]
//    {
//        get
//        {
//            if (index < 0 || index >= allArrays.Length)
//                throw new IndexOutOfRangeException("Array index out of range");
//            return allArrays[index];
//        }
//    }
    
//    // 값 증가 메서드들
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementSpinCount(int level, int gems)
//    {
//        spinCounts[GetIndex(level, gems)] += 1;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementRespinCount(int level, int gems)
//    {
//        respinCounts[GetIndex(level, gems)] += 1;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementLevelUpCount(int level, int gems)
//    {
//        levelUpCounts[GetIndex(level, gems)] += 1;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementCreateCoinCount(FeatureBonusType type, int level, int gems)
//    {
//        var withRedCoin = (type & FeatureBonusType.Collect) == FeatureBonusType.Collect;
//        int index = GetIndex(level, gems);
        
//        if (withRedCoin)
//        {
//            createCoinCountA[index] += 1;
//        }
//        else
//        {
//            createCoinCountB[index] += 1;
//        }
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementObtainCoinValue(FeatureBonusType type, int level, int gems, double value)
//    {
//        var withRedCoin = (type & FeatureBonusType.Collect) == FeatureBonusType.Collect;
//        int index = GetIndex(level, gems);
        
//        if (withRedCoin)
//        {
//            obtainCoinValueA[index] += value;
//        }
//        else
//        {
//            obtainCoinValueB[index] += value;
//        }
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementCreateGemCount(int level, int gems)
//    {
//        createGemCount[GetIndex(level, gems)] += 1;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementObtainGemValue(int level, int gems, double value)
//    {
//        obtainGemValue[GetIndex(level, gems)] += value;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementRedCoinCount(int level, int gems)
//    {
//        redCoinCount[GetIndex(level, gems)] += 1;
//    }
    
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void IncrementFreeSpinCoinCount(int level, int gems)
//    {
//        freeSpinCoinCount[GetIndex(level, gems)] += 1;
//    }
    
//    // 스레드 안전한 배열 병합 메서드 추가
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void MergeFrom(ArraySlotStats source)
//    {
//        // 모든 배열에 대해 병합 수행
//        for (int i = 0; i < allArrays.Length; i++)
//        {
//            MergeArray(this[i], source[i]);
//        }
//    }
    
//    // 단일 배열 병합을 위한 스레드 안전 메서드
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    private static void MergeArray(double[] target, double[] source)
//    {
//        for (int i = 0; i < target.Length; i++)
//        {
//            double sourceValue = source[i];
//            // 소스 값이 0인 경우에도 병합 (디버깅을 위해)
//            // if (sourceValue == 0) continue; // 소스 값이 0이면 건너뛰기
            
//            // lock을 사용한 스레드 안전 업데이트
//            lock (target)
//            {
//                target[i] += sourceValue;
//            }
//        }
//    }
    
//    //// 특정 인덱스의 값을 스레드 안전하게 증가시키는 메서드
//    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
//    //private static void ThreadSafeAdd(double[] array, int index, double value)
//    //{
//    //    if (value == 0) return; // 값이 0이면 건너뛰기
        
//    //    // lock을 사용한 스레드 안전 업데이트
//    //    lock (array)
//    //    {
//    //        array[index] += value;
//    //    }
//    //}
//}

public class SpinStatistics
{
    private readonly StatsMatrix<int> baseWinStats;
    public IStatsMatrix<int> BaseWinStats => baseWinStats;
    
    // 배열 기반 통계만 사용
    //private readonly List<ArraySlotStats> arrayFeatureCounts;
    //public List<ArraySlotStats> ArrayFeatureCounts => arrayFeatureCounts;

    private double baseWinAmount;
    public double BaseWinAmount => baseWinAmount;

    private double featureWinAmount;
    public double FeatureWinAmount => featureWinAmount;

    public SpinStatistics()
    {
        baseWinStats = new StatsMatrix<int>(SlotConst.PAYTABLE_SYMBOL, SlotConst.MAX_HITS);
        
        // 배열 기반 통계 초기화
        //arrayFeatureCounts = new List<ArraySlotStats>(BonusTypeConverter.CombiTypeOrder.Count);
        
        //for (int i = 0; i < BonusTypeConverter.CombiTypeOrder.Count; i++)
        //{
        //    arrayFeatureCounts.Add(new ArraySlotStats());
        //}

        baseWinAmount = 0.0;
        featureWinAmount = 0.0;
    }
    
    // 배열 기반 통계 접근자만 사용
    // public ArraySlotStats GetFeatureStats(FeatureBonusType featureBonusType)
    // {
    //     var idx = BonusTypeConverter.GetCombiTypeOrder(featureBonusType);
    //     return arrayFeatureCounts[idx - 1];
    // }

    public void Clear()
    {
        baseWinStats.Clear();
        
        // 배열 기반 통계 초기화
        //foreach (var arrayFeatureCount in arrayFeatureCounts)
        //{
        //    arrayFeatureCount.Clear();
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseWinCount(byte symbol, int hits) => baseWinStats.Update(symbol, hits, 1, (a, b) => a + b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBaseWinAmount(long amount) => baseWinAmount += amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFeatureWinAmount(double amount) => featureWinAmount += amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetBaseWinAmount() => baseWinAmount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetFeatureWinAmount() => featureWinAmount;
    
    // 배열 기반 통계 병합 메서드 추가
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public void MergeArrayStats(SpinStatistics source)
    // {
    //     // baseWinStats 병합
    //     // StatsMatrix에 MergeFrom 메서드가 없으므로 직접 병합
    //     foreach (var (row, col, value) in source.BaseWinStats.GetItems())
    //     {
    //         if (value > 0)
    //         {
    //             baseWinStats.Update(row, col, value, (a, b) => a + b);
    //         }
    //     }
        
    //     // 배열 기반 통계 병합
    //     // for (int i = 0; i < arrayFeatureCounts.Count; i++)
    //     // {
    //     //     arrayFeatureCounts[i].MergeFrom(source.ArrayFeatureCounts[i]);
    //     // }
        
    //     // lock을 사용한 스레드 안전 업데이트
    //     lock (this)
    //     {
    //         baseWinAmount += source.BaseWinAmount;
    //         featureWinAmount += source.FeatureWinAmount;
    //     }
    // }
}

