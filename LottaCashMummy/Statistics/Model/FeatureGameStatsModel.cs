namespace LottaCashMummy.Statistics.Model;

public class FeatureGameStatsModel
{
    private readonly Dictionary<(int, int, int), int> levelCount = new();
    public Dictionary<(int, int, int), int> LevelCount => levelCount;

    private readonly Dictionary<(int, int, int), int> spinCount = new();
    public Dictionary<(int, int, int), int> SpinCount => spinCount;

    private readonly Dictionary<(int, int, int), int> gemCount = new();
    public Dictionary<(int, int, int), int> GemCount => gemCount;

    private readonly Dictionary<(int, int, int), double> gemValue = new();
    public Dictionary<(int, int, int), double> GemValue => gemValue;

    private readonly Dictionary<(int, int, int), int> coinCount = new();
    public Dictionary<(int, int, int), int> CoinCount => coinCount;

    private readonly Dictionary<(int, int, int), double> coinValue = new();
    public Dictionary<(int, int, int), double> CoinValue => coinValue;

    public void AddLevel(int bonusType, int initGem, int level)
    {
        var key = (bonusType, initGem, level);
        this.levelCount[key] = levelCount.GetValueOrDefault(key, 0) + 1;
    }

    public void AddSpin(int bonusType, int initGem, int level)
    {
        var key = (bonusType, initGem, level);
        this.spinCount[key] = spinCount.GetValueOrDefault(key, 0) + 1;
    }

    public void AddGemCount(int bonusType, int initGem, int level)
    {
        var key = (bonusType, initGem, level);
        this.gemCount[key] = gemCount.GetValueOrDefault(key, 0) + 1;
    }

    public void AddGemValue(int bonusType, int initGem, int level, double value)
    {
        var key = (bonusType, initGem, level);
        this.gemValue[key] = gemValue.GetValueOrDefault(key, 0) + value;
    }

    public void AddCoinCount(int bonusType, int initGem, int level)
    {
        var key = (bonusType, initGem, level);
        this.coinCount[key] = coinCount.GetValueOrDefault(key, 0) + 1;
    }

    public void AddCoinValue(int bonusType, int initGem, int level, double value)
    {
        var key = (bonusType, initGem, level);
        this.coinValue[key] = coinValue.GetValueOrDefault(key, 0) + value;
    }
}

