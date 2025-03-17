
using SpinOfFortune.Shared;

namespace SpinOfFortune.Statistics;

public class BaseStatsModel
{
    private long totalSpinCount;
    public long TotalSpinCount => totalSpinCount;

    private readonly Dictionary<CombinationPayType, int> totalWinPays = [];
    public Dictionary<CombinationPayType, int> TotalWinPays => totalWinPays;

    public void AddSpinCount()
    {
        totalSpinCount++;
    }

    public void AddWinPay(CombinationPayType combiType, int winPay)
    {
        totalWinPays[combiType] = totalWinPays.GetValueOrDefault(combiType, 0) + winPay;
    }
}
