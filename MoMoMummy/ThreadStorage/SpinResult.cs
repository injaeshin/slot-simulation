
using MoMoMummy.Shared;

namespace MoMoMummy.ThreadStorage;

// 피처 게임의 현재 진행 상황을 보관함

public class FeatureSingleSpinResult
{
    // feature
    public long GemSpinCount { get; private set; }
    public long GemCount { get; private set; }
    public double GemValue { get; private set; }
    public long CoinSpinCount { get; private set; }
    public long CoinCount { get; private set; }
    public double CoinValue { get; private set; }
    public bool HasRedCoin { get; private set; }


    // respin
    public long RespinCoinSpinCount { get; private set; }
    public long RespinCoinCount { get; private set; }
    public double RespinCoinValue { get; private set; }


    // 레벨업 후 남아 있는 젬
    public long RemainGemCount { get; private set; }
    public void SetRemainGemCount(long count) => RemainGemCount = count;

    public void Clear()
    {
        GemSpinCount = 0;
        GemCount = 0;
        GemValue = 0;
        CoinSpinCount = 0;
        CoinCount = 0;
        CoinValue = 0;
        HasRedCoin = false;

        RespinCoinSpinCount = 0;
        RespinCoinCount = 0;
        RespinCoinValue = 0;

        RemainGemCount = 0;
    }

    public void AddSymbolCount(FeatureSymbolType symbolType)
    {
        if (symbolType == FeatureSymbolType.Coin)
        {
            GemCount++;
        }
        else if (symbolType == FeatureSymbolType.Gem)
        {
            CoinCount++;
        }
        else
        {
            throw new Exception($"Invalid symbol type: {symbolType}");
        }
    }

    public void AddRespinSymbolCount(FeatureSymbolType symbolType)
    {
        if (symbolType == FeatureSymbolType.Coin)
        {
            RespinCoinCount++;
        }
        else if (symbolType == FeatureSymbolType.Gem)
        {
            throw new Exception("Gem is not allowed to be added to respin");
        }
        else
        {
            throw new Exception($"Invalid symbol type: {symbolType}");
        }
    }

    public void AddSymbolValue(FeatureSymbolType symbolType, double value)
    {
        if (symbolType == FeatureSymbolType.Coin)
        {
            CoinValue += value;
        }
        else if (symbolType == FeatureSymbolType.Gem)
        {
            GemValue += value;
        }
        else
        {
            throw new Exception($"Invalid symbol type: {symbolType}");
        }
    }

    public void AddRespinSymbolValue(FeatureSymbolType symbolType, double value)
    {
        if (symbolType == FeatureSymbolType.Coin)
        {
            RespinCoinValue += value;
        }
        else if (symbolType == FeatureSymbolType.Gem)
        {
            throw new Exception("Gem is not allowed to be added to respin");
        }
        else
        {
            throw new Exception($"Invalid symbol type: {symbolType}");
        }
    }

    public void AddGemSpinCount()
    {
        GemSpinCount++;
    }

    public void AddCoinSpinCount()
    {
        CoinSpinCount++;
    }

    public void AddCoinSpinCountRespin()
    {
        RespinCoinSpinCount++;
    }

    public void SetHasRedCoin(bool hasRedCoin)
    {
        HasRedCoin = hasRedCoin;
    }
}