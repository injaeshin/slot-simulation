
using SpinOfFortune.Shared;
using SpinOfFortune.Statistics;

namespace SpinOfFortune.ThreadBuffer;

public class BaseStorage(Random random)
{
    private readonly Random random = random;
    public Random Random => random;

    private BaseStatsModel baseStatsModel = new();
    public BaseStatsModel Statistics => baseStatsModel;

    private readonly SymbolType[] symbols = new SymbolType[3];
    public SymbolType[] Symbols => symbols;

    public bool HasBonus()
    {
        return false;
    }



    public void StatsClear()
    {
        baseStatsModel = new();
    }

    public void AddWinPay(CombinationPayType combiType, int pay)
    {
        baseStatsModel.AddWinPay(combiType, pay);
    }

    public void AddSpinCount()
    {
        baseStatsModel.AddSpinCount();
    }
}
