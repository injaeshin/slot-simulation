using LineAndFree.Shared;

namespace LineAndFree.Statistics;

/// <summary>
/// 게임 타입(베이스/프리)별 승리 통계를 관리하는 클래스
/// </summary>
public class GameWinStatistics
{
    #region 기본 스핀 통계

    // 총 스핀 횟수
    public long TotalSpinCount { get; private set; } = 0;

    // 스캐터 개수별 스핀 횟수
    private readonly Dictionary<int, int> spinCountByScatter = [];
    public Dictionary<int, int> SpinCountByScatter => spinCountByScatter;

    /// <summary>
    /// 총 스핀 횟수 증가
    /// </summary>
    public void IncrementSpinCount()
    {
        TotalSpinCount++;
    }

    /// <summary>
    /// 특정 스캐터 개수의 스핀 횟수 증가
    /// </summary>
    /// <param name="scatterCount">스캐터 개수</param>
    public void IncrementSpinCountByScatter(int scatterCount)
    {
        TotalSpinCount++;
        spinCountByScatter[scatterCount] = spinCountByScatter.GetValueOrDefault(scatterCount, 0) + 1;
    }

    #endregion

    #region 심볼 승리 통계

    // 심볼 유형과 개수별 승리 금액
    public Dictionary<(SymbolType, int), long> SymbolPayAmount { get; private set; } = [];

    // 심볼 유형과 개수별 승리 횟수
    public Dictionary<(SymbolType, int), long> SymbolHitCount { get; private set; } = [];

    // 총 심볼 승리 횟수
    public long SymbolWinCount { get; private set; } = 0;

    /// <summary>
    /// 심볼 승리 기록
    /// </summary>
    /// <param name="symbol">심볼 유형</param>
    /// <param name="count">심볼 개수</param>
    /// <param name="winPay">승리 금액</param>
    public void RecordSymbolWin(SymbolType symbol, int count, int winPay)
    {
        IncrementSymbolWinCount();
        var key = (symbol, count);
        SymbolPayAmount[key] = SymbolPayAmount.GetValueOrDefault(key) + winPay;
        SymbolHitCount[key] = SymbolHitCount.GetValueOrDefault(key) + 1;
    }

    private void IncrementSymbolWinCount()
    {
        SymbolWinCount++;
    }

    #endregion

    #region 스캐터 승리 통계

    // 스캐터 개수별 승리 횟수
    private readonly Dictionary<int, int> scatterWinCount = [];
    public Dictionary<int, int> ScatterWinCount => scatterWinCount;

    /// <summary>
    /// 스캐터 승리 기록
    /// </summary>
    /// <param name="scatterCount">스캐터 개수</param>
    public void RecordScatterWin(int scatterCount)
    {
        scatterWinCount[scatterCount] = scatterWinCount.GetValueOrDefault(scatterCount, 0) + 1;
    }

    #endregion

    #region 프리스핀 리트리거 통계

    // 리트리거 세부 통계 - (초기 진입 스캐터 수, 리트리거 스캐터 수) => 발생 횟수
    private Dictionary<(int InitialScatter, int RetriggerScatter), int> retriggersDetailedCount = new();
    public Dictionary<(int InitialScatter, int RetriggerScatter), int> RetriggersDetailedCount => retriggersDetailedCount;

    // 총 리트리거 횟수
    private int totalRetriggerCount;
    public int TotalRetriggerCount => totalRetriggerCount;

    // 세션당 리트리거 발생 횟수 분포 (예: 1회 리트리거된 세션 수, 2회 리트리거된 세션 수...)
    private readonly Dictionary<int, int> retriggersPerSessionCount = new();
    public Dictionary<int, int> RetriggersPerSessionCount => retriggersPerSessionCount;


    /// <summary>
    /// 개별 리트리거 발생 기록
    /// </summary>
    /// <param name="initialScatterCount">프리스핀 초기 진입 시 스캐터 개수</param>
    /// <param name="retriggeredScatterCount">리트리거 발생 시 스캐터 개수</param>
    public void RecordRetrigger(int initialScatterCount, int retriggeredScatterCount)
    {
        // 초기 진입 스캐터 개수와 리트리거 스캐터 개수 조합을 기록
        var key = (initialScatterCount, retriggeredScatterCount);
        retriggersDetailedCount[key] = retriggersDetailedCount.GetValueOrDefault(key, 0) + 1;

        // 총 리트리거 횟수 증가
        totalRetriggerCount++;
    }

    #endregion

    #region 총계 및 초기화

    // 총 승리 금액
    public long TotalPayAmount { get; private set; } = 0;

    public void IncrementTotalPayAmount(long amount)
    {
        TotalPayAmount += amount;
    }

    // 총 승리 횟수
    public long TotalHitCount { get; private set; } = 0;

    public void IncrementTotalHitCount()
    {
        TotalHitCount++;
    }

    #endregion
}

/// <summary>
/// 베이스 게임과 프리 게임의 통계를 관리하는 클래스
/// </summary>
public class SpinStatistics
{
    // 베이스 게임과 프리 게임의 통계 객체
    private readonly GameWinStatistics baseWinStatistics = new();
    private readonly GameWinStatistics freeWinStatistics = new();

    public GameWinStatistics BaseWinStatistics => baseWinStatistics;
    public GameWinStatistics FreeWinStatistics => freeWinStatistics;

    #region 스핀 카운트 통계

    /// <summary>
    /// 베이스 게임 스핀 횟수 증가
    /// </summary>
    public void IncrementBaseGameSpinCount()
    {
        baseWinStatistics.IncrementSpinCount();
    }

    #endregion

    #region 심볼 승리 통계

    /// <summary>
    /// 베이스 게임 심볼 승리 기록
    /// </summary>
    /// <param name="symbol">심볼 유형</param>
    /// <param name="count">심볼 개수</param>
    /// <param name="pay">승리 금액</param>
    public void RecordBaseGameSymbolWin(SymbolType symbol, int count, int pay)
    {
        var key = (symbol, count);
        baseWinStatistics.SymbolPayAmount[key] = baseWinStatistics.SymbolPayAmount.GetValueOrDefault(key) + pay;
        baseWinStatistics.SymbolHitCount[key] = baseWinStatistics.SymbolHitCount.GetValueOrDefault(key) + 1;
        baseWinStatistics.IncrementTotalPayAmount(pay);
        baseWinStatistics.IncrementTotalHitCount();
    }

    /// <summary>
    /// 프리 게임 심볼 승리 기록
    /// </summary>
    /// <param name="symbol">심볼 유형</param>
    /// <param name="count">심볼 개수</param>
    /// <param name="pay">승리 금액</param>
    public void RecordFreeGameSymbolWin(SymbolType symbol, int count, int pay)
    {
        freeWinStatistics.RecordSymbolWin(symbol, count, pay);
        freeWinStatistics.IncrementTotalPayAmount(pay);
        freeWinStatistics.IncrementTotalHitCount();
    }

    #endregion

    #region 스캐터 승리 통계

    /// <summary>
    /// 베이스 게임 스캐터 승리 기록
    /// </summary>
    /// <param name="scatterCount">스캐터 개수</param>
    public void RecordBaseGameScatterWin(int scatterCount)
    {
        baseWinStatistics.RecordScatterWin(scatterCount);
    }

    /// <summary>
    /// 프리 게임 스캐터 승리 기록
    /// </summary>
    /// <param name="scatterCount">스캐터 개수</param>
    public void RecordFreeGameScatterWin(int scatterCount)
    {
        freeWinStatistics.RecordScatterWin(scatterCount);
    }

    /// <summary>
    /// 특정 스캐터 개수에 따른 스핀 횟수 증가
    /// </summary>
    /// <param name="scatterCount">스캐터 개수</param>
    public void IncrementSpinCountByScatter(int scatterCount)
    {
        freeWinStatistics.IncrementSpinCountByScatter(scatterCount);
    }

    #endregion

    #region 프리스핀 리트리거 통계

    /// <summary>
    /// 프리 게임에서 리트리거 발생 기록
    /// </summary>
    /// <param name="initialScatterCount">프리스핀 초기 진입 시 스캐터 개수</param>
    /// <param name="retriggeredScatterCount">리트리거 발생 시 스캐터 개수</param>
    public void RecordFreeGameRetrigger(int initialScatterCount, int retriggeredScatterCount)
    {
        freeWinStatistics.RecordRetrigger(initialScatterCount, retriggeredScatterCount);
    }

    #endregion
}