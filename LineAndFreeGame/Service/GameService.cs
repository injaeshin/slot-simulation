using Microsoft.Extensions.Logging;

using LineAndFreeGame.Common;
using LineAndFreeGame.Game;
using LineAndFreeGame.Table;
using LineAndFreeGame.ThreadStorage;
using Microsoft.Extensions.Configuration;

namespace LineAndFreeGame.Service;

public interface IGameService
{
    void PrintResults();
    void PrintSymbolDistribution();
    void SimulateSingleSpin(ThreadBuffer buffer);
}

public class GameService : IGameService
{
    private readonly ILogger<GameService> logger;

    private readonly PayTable payTable;
    private readonly LineGame lineGame;
    private readonly GameDataLoader dataLoader;

    public GameService(IConfiguration conf, ILoggerFactory logFactory)
    {
        var filePath = conf.GetSection("file").Value ?? throw new Exception("Reel strip path not found in configuration");
        this.dataLoader = GameDataLoader.Read(filePath) ?? throw new Exception("Failed to load reel strip");
        if (this.dataLoader == null)
        {
            throw new Exception("Failed to load reel strip");
        }

        this.payTable = new PayTable(this.dataLoader);
        this.logger = logFactory.CreateLogger<GameService>();
        this.lineGame = new LineGame(new ReelStrip(this.dataLoader, "Base"), this.payTable, logFactory.CreateLogger<LineGame>());
    }

    public void SimulateSingleSpin(ThreadBuffer buffer)
    {
        this.lineGame.SimulateSingleSpin(buffer);
    }

    public void PrintResults()
    {
        throw new NotImplementedException();
    }

    public void PrintSymbolDistribution()
    {
        throw new NotImplementedException();
    }

}