using LineAndFreeGame.Common;

namespace LineAndFreeGame.Statistics;

public class SpinStatistics
{
    private long totalSpinCount;
    public long TotalSpinCount => totalSpinCount;

    private readonly Dictionary<(SymbolType, int), int> totalWinPays = [];
    public Dictionary<(SymbolType, int), int> TotalWinPays => totalWinPays;

    private double totalBonusPay;
    public double TotalBonusPay => totalBonusPay;

    public void AddSpinCount()
    {
        totalSpinCount++;
    }

    public void AddWinPay(SymbolType symbol, int count, int winPay)
    {
        totalWinPays[(symbol, count)] = totalWinPays.GetValueOrDefault((symbol, count), 0) + winPay;
    }

    public void AddBonusPay(double bonusPay)
    {
        totalBonusPay += bonusPay;
    }
}