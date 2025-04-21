using LineAndFree.Shared;
using LineAndFree.Statistics;

namespace LineAndFree.Service;

public class StatsService
{
    private readonly List<SpinStatistics> spinStats = [];

    public void AddSpinStats(SpinStatistics spinStats)
    {
        this.spinStats.Add(spinStats);
    }

    public long GetBaseGameTotalSpinCount()
    {
        return spinStats.Sum(model => model.BaseWinStatistics.TotalSpinCount);
    }

    public long GetBaseGameTotalWinAmount()
    {
        return spinStats.Sum(model => model.BaseWinStatistics.TotalPayAmount);
    }

    public long GetBaseGameTotalWinCount()
    {
        return spinStats.Sum(model => model.BaseWinStatistics.TotalHitCount);
    }

    public long GetBaseGameTotalWinPayAmount(SymbolType symbol, int count)
    {
        long total = 0;
        foreach (var model in spinStats)
        {
            total += model.BaseWinStatistics.SymbolPayAmount.GetValueOrDefault((symbol, count));
        }
        return total;
    }

    public long GetBaseGameTotalWinCountWithScatter(int scatterCount)
    {
        long total = 0;
        foreach (var model in spinStats)
        {
            foreach (var kvp in model.BaseWinStatistics.ScatterWinCount)
            {
                if (kvp.Key == scatterCount)
                {
                    total += kvp.Value;
                }
            }
        }
        return total;
    }

    public long GetBaseGameTotalWinCountWithScatter()
    {
        long total = 0;
        foreach (var model in spinStats)
        {
            foreach (var kvp in model.BaseWinStatistics.ScatterWinCount)
            {
                total += kvp.Value;
            }
        }
        return total;
    }

    public long GetFreeGameTotalSpinCount()
    {
        return spinStats.Sum(model => model.FreeWinStatistics.TotalSpinCount);
    }

    public long GetFreeGameTotalWinAmount()
    {
        return spinStats.Sum(model => model.FreeWinStatistics.TotalPayAmount);
    }

    public long GetFreeGameWinPayAmount(SymbolType symbol, int count)
    {
        long total = 0;
        foreach (var model in spinStats)
        {
            total += model.FreeWinStatistics.SymbolPayAmount.GetValueOrDefault((symbol, count));
        }
        return total;
    }

    public long GetFreeGameWinCountWithScatter(int scatterCount)
    {
        long total = 0;
        foreach (var model in spinStats)
        {
            foreach (var kvp in model.FreeWinStatistics.ScatterWinCount)
            {
                if (kvp.Key == scatterCount)
                {
                    total += kvp.Value;
                }
            }
        }
        return total;
    }

    /// <summary>
    /// 모든 초기 스캐터 개수별 리트리거 통계 조회
    /// </summary>
    /// <returns>초기 스캐터 개수 -> 리트리거 스캐터 개수 -> 발생 횟수의 중첩 Dictionary</returns>
    public Dictionary<int, Dictionary<int, long>> GetAllFreeGameRetriggerStats()
    {
        var result = new Dictionary<int, Dictionary<int, long>>();

        // 일반적인 초기 스캐터 개수에 대한 Dictionary 초기화
        result[3] = new Dictionary<int, long> { [3] = 0, [4] = 0, [5] = 0 };
        result[4] = new Dictionary<int, long> { [3] = 0, [4] = 0, [5] = 0 };
        result[5] = new Dictionary<int, long> { [3] = 0, [4] = 0, [5] = 0 };

        foreach (var model in spinStats)
        {
            foreach (var kvp in model.FreeWinStatistics.RetriggersDetailedCount)
            {
                int initialScatter = kvp.Key.InitialScatter;
                int retriggerScatter = kvp.Key.RetriggerScatter;

                // 해당 초기 스캐터 개수에 대한 Dictionary가 없으면 생성
                if (!result.ContainsKey(initialScatter))
                {
                    result[initialScatter] = new Dictionary<int, long>();
                }

                // 리트리거 스캐터 개수에 대한 카운트 증가
                result[initialScatter][retriggerScatter] =
                    result[initialScatter].GetValueOrDefault(retriggerScatter, 0) + kvp.Value;
            }
        }

        return result;
    }
}
