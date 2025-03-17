using SpinOfFortune.Game;
using SpinOfFortune.Shared;
using SpinOfFortune.Table;
using SpinOfFortune.ThreadBuffer;

namespace SpinOfFortune.Service;

public class GameService
{
    private readonly BaseGame baseGame;
    private readonly BonusGame bonusGame;

    private readonly IReelSet reelSet;

    public GameService(ReelSet reelSet)
    {
        this.reelSet = reelSet;
        this.baseGame = new BaseGame(reelSet);
        this.bonusGame = new BonusGame();
    }

    public List<SymbolType[]> GetRawReelStrip()
    {
        return reelSet.ReelStrips;
    }

    public void SimulateSingleSpin(ThreadStorage ts)
    {
        var isWin = baseGame.Spin(ts);
        if (!isWin)
        {
            return;
        }

        var bs = ts.BaseStorage;
        var symbol = bs.Symbols;

        // bonus 확인
        if (bs.HasBonus())
        {
            bonusGame.Spin(ts);
        }
    }
}
