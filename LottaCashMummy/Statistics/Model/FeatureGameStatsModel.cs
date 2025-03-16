
namespace LottaCashMummy.Statistics.Model;

public class FeatureGameStatsModel
{
    private readonly Dictionary<(int, int, int), int> levelCount = new();
    public Dictionary<(int, int, int), int> LevelCount => levelCount;

    private readonly Dictionary<(int, int, int), long> gemSpinCount = new();
    public Dictionary<(int, int, int), long> GemSpinCount => gemSpinCount;

    private readonly Dictionary<(int, int, int), long> coinSpinCount = new();
    public Dictionary<(int, int, int), long> CoinSpinCount => coinSpinCount;

    private readonly Dictionary<(int, int, int), long> gemCount = new();
    public Dictionary<(int, int, int), long> GemCount => gemCount;

    private readonly Dictionary<(int, int, int), double> gemValue = new();
    public Dictionary<(int, int, int), double> GemValue => gemValue;

    private readonly Dictionary<(int, int, int), long> coinCount = new();
    public Dictionary<(int, int, int), long> CoinCount => coinCount;

    private readonly Dictionary<(int, int, int), double> coinValue = new();
    public Dictionary<(int, int, int), double> CoinValue => coinValue;

    private readonly Dictionary<(int, int, int), long> redCoinCount = new();
    public Dictionary<(int, int, int), long> RedCoinCount => redCoinCount;

    public void AddLevel(int bonusType, int initGem, int level)
    {
        var key = (bonusType, initGem, level);
        this.levelCount[key] = levelCount.GetValueOrDefault(key, 0) + 1;
    }

    public void AddGemSpinCount(int bonusType, int initGem, int level, long value)
    {
        var key = (bonusType, initGem, level);
        this.gemSpinCount[key] = gemSpinCount.GetValueOrDefault(key, 0) + value;
    }

    public void AddCoinSpinCount(int bonusType, int initGem, int level, long value)
    {
        var key = (bonusType, initGem, level);
        this.coinSpinCount[key] = coinSpinCount.GetValueOrDefault(key, 0) + value;
    }

    public void AddGemCount(int bonusType, int initGem, int level, long value)
    {
        var key = (bonusType, initGem, level);
        this.gemCount[key] = gemCount.GetValueOrDefault(key, 0) + value;
    }

    public void AddGemValue(int bonusType, int initGem, int level, double value)
    {
        var key = (bonusType, initGem, level);
        this.gemValue[key] = gemValue.GetValueOrDefault(key, 0) + value;
    }

    public void AddCoinCount(int bonusType, int initGem, int level, long value)
    {
        var key = (bonusType, initGem, level);
        this.coinCount[key] = coinCount.GetValueOrDefault(key, 0) + value;
    }

    public void AddCoinValue(int bonusType, int initGem, int level, double value)
    {
        var key = (bonusType, initGem, level);
        this.coinValue[key] = coinValue.GetValueOrDefault(key, 0) + value;
    }

    public void AddRedCoinCount(int bonusTypeOrder, int initGemCount, byte level)
    {
        var key = (bonusTypeOrder, initGemCount, level);
        this.redCoinCount[key] = redCoinCount.GetValueOrDefault(key, 0) + 1;
    }
}