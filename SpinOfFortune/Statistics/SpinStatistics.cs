using SpinOfFortune.Shared;

namespace SpinOfFortune.Statistics;

public class SpinStatistics
{
    private long totalSpinCount;
    public long TotalSpinCount => totalSpinCount;

    private readonly Dictionary<CombinationPayType, int> totalWinPays = [];
    public Dictionary<CombinationPayType, int> TotalWinPays => totalWinPays;

    private double totalBonusPay;
    public double TotalBonusPay => totalBonusPay;

    public void AddSpinCount()
    {
        totalSpinCount++;
    }

    public void AddWinPay(CombinationPayType combiType, int winPay)
    {
        totalWinPays[combiType] = totalWinPays.GetValueOrDefault(combiType, 0) + winPay;
    }

    public void AddBonusPay(double bonusPay)
    {
        totalBonusPay += bonusPay;
    }
}