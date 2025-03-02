
using LottaCashMummy.Buffer;
using System;

namespace LottaCashMummy.Game;

public class FeatureService
{
    private readonly FeatureSetup setup;
    private readonly FeatureSymbolCreate symbol;
    private readonly FeatureSymbolCollect symbolCollect;

    public FeatureService(IFeatureData featureData)
    {
        symbol = new FeatureSymbolCreate(featureData);

        var mummy = new FeatureMummy(featureData);
        setup = new FeatureSetup(featureData, mummy);
        symbolCollect = new FeatureSymbolCollect(mummy);
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
            int splitSymbolIndex = symbol.CreateSymbolToScreenArea(fs);
            symbolCollect.CollectCoin(fs, splitSymbolIndex, isRespin: false, out bool hasRedCoin);
            if (hasRedCoin)
            {
                Respin(fs, splitSymbolIndex);
            }

            symbolCollect.CollectGem(fs, splitSymbolIndex, isRespin: false);

            // 초기화
            fs.ClearSymbolInScreenArea();
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
            fs.ClearSymbolInMummyArea();

            splitSymbolIdx = symbol.CreateSymbolToMummyArea(fs, splitSymbolIdx);
            if (fs.CoinCount == 0)
            {
                break;
            }

            symbolCollect.CollectCoin(fs, splitSymbolIdx, isRespin: true, out bool hasRedCoin);
            if (hasRedCoin)
            {
                throw new Exception("Red coin found");
            }
        } while (true);
    }
}
