using LineAndFree.Game;
using LineAndFree.Shared;
using LineAndFree.Table;
using LineAndFree.ThreadStorage;
using Microsoft.Extensions.Configuration;

namespace LineAndFree.Service;

public interface IGameService
{
    Task SimulateSingleSpin(ThreadBuffer buffer);
    void PrintSymbolDistribution();
    void PrintReelStrip();
}

public class GameService : IGameService
{
    private readonly PayTable payTable;
    private readonly BaseGame baseGame;
    private readonly FreeGame freeGame;

    public GameService(IConfiguration conf)
    {
        var filePath = conf.GetSection("file").Value ?? throw new Exception("Reel strip path not found in configuration");
        var dataLoader = GameDataLoader.Read(filePath) ?? throw new Exception("Failed to load reel strip");

        this.payTable = new PayTable(dataLoader);
        this.baseGame = new BaseGame(dataLoader, this.payTable);
        this.freeGame = new FreeGame(dataLoader, this.payTable);
    }

    public async Task SimulateSingleSpin(ThreadBuffer buffer)
    {
        await this.baseGame.SimulateSingleSpin(buffer);
    }

    public void PrintReelStrip()
    {
        baseGame.PrintReelStrip();
        freeGame.PrintReelStrip();
    }

    public void PrintSymbolDistribution()
    {
        baseGame.PrintSymbolDistribution();
        freeGame.PrintSymbolDistribution();
    }
}