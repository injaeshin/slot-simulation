using Common;
using LottaCashMummy.Table;

namespace LottaCashMummy;

public interface IBaseData
{
    IBaseReelSet BaseReelSet { get; }
    IBaseSymbol BaseSymbol { get; }
    IPayTable PayTable { get; }
}

public class BaseData : IBaseData
{
    public IBaseReelSet BaseReelSet { get; }
    public IBaseSymbol BaseSymbol { get; }
    public IPayTable PayTable { get; }

    public BaseData(GameDataLoader kv)
    {
        BaseReelSet = new BaseReelSet(kv);
        BaseSymbol = new BaseSymbol(kv);
        PayTable = new PayTable(kv);
    }
}

public interface IFeatureData
{
    IFeatureMummy Mummy { get; }
    IFeatureSymbol Symbol { get; }
}

public class FeatureData : IFeatureData
{
    public IFeatureMummy Mummy { get; }
    public IFeatureSymbol Symbol { get; }

    public FeatureData(GameDataLoader kv)
    {
        Mummy = new FeatureMummy(kv);
        Symbol = new FeatureSymbol(kv);
    }
}

public interface IJackpotData
{
    IJackpot Jackpot { get; }
}

public class JackpotData : IJackpotData
{
    public IJackpot Jackpot { get; }

    public JackpotData(GameDataLoader kv)
    {
        Jackpot = new Jackpot(kv);
    }
}
