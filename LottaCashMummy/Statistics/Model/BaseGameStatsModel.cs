
namespace LottaCashMummy.Statistics.Model;

public class BaseGameStatsModel
{
    private long _spinCount { get; set; }
    private Dictionary<(int, int), long> winPay { get; set; }

    public BaseGameStatsModel()
    {
        winPay = new Dictionary<(int, int), long>(12);
    }

    public void AddSpinCount()
    {
        _spinCount++;
    }

    public long GetSpinCount()
    {
        return _spinCount;
    }

    public void AddWinPay(int symbol, int hit, long amount)
    {
        var key = (symbol, hit);
        winPay[key] = winPay.GetValueOrDefault(key, 0) + amount;
    }

    public long GetWinPay(int symbol, int hit)
    {
        var key = (symbol, hit);
        return winPay.GetValueOrDefault(key, 0);
    }
}