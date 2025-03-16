using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy.Game;

public class FeatureService
{
    private readonly FeatureSetup setup;
    private readonly FeatureSymbolCreate symbol;
    private readonly FeatureMummy mummy;

    private readonly IFeatureData fd;

    public FeatureService(IFeatureData featureData)
    {
        symbol = new FeatureSymbolCreate(featureData);
        mummy = new FeatureMummy(featureData);
        setup = new FeatureSetup(featureData, mummy);
        fd = featureData;
    }

    public void Execute(ThreadLocalStorage tls)
    {
        setup.Init(tls);
        Spin(tls.FeatureStorage);
    }

    private void Spin(FeatureStorage fs)
    {
        fs.Enter();

        while (fs.UseSpinCount())
        {
            (int splitSymbolIndex, bool hasRedCoin) = symbol.CreateSymbol(fs);
            if (hasRedCoin)
            {
                Respin(fs, splitSymbolIndex);
            }

            CalculateResult(fs);
        }
    }

    private void Respin(FeatureStorage fs, int splitSymbolIdx)
    {
        var respinCount = 0;

        // If splitSymbolIdx isn't in mummy area. it is already created outside.
        // so, do not create inside.
        if (!fs.IsActiveMummyArea(splitSymbolIdx))
        {
            splitSymbolIdx = -1;
        }

        do
        {
            respinCount++;
            if (respinCount > 100)
            {
                throw new Exception("Respin count is too high");
            }

            fs.UseRespin();

            (splitSymbolIdx, var hasCoin) = symbol.CreateSymbolRespin(fs, splitSymbolIdx);
            if (!hasCoin)
            {
                break;
            }
        } while (true);
    }

    private void CalculateResult(FeatureStorage fs)
    {
        var spinResult = fs.SpinResult;

        fs.AddStatsAddGemCountAndValue();
        fs.AddStatsAddCoinCountAndValue();
        fs.AddStatsAddRedCoinCount();

        if (mummy.LevelUp(fs))
        {
            fs.Mummy.CenterIndex = FeatureMummy.CalculateCenterIndex(fs.Mummy.CenterIndex, fs.Mummy.Area);
            FeatureMummy.CalculateArea(fs, fs.Mummy.CenterIndex, fs.Mummy.Area);
        }
    }
}
