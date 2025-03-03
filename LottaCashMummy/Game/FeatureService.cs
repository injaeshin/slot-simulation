
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using System;
using System.Security.Cryptography.X509Certificates;

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
        var featureType = fs.FeatureBonusType;
        if ((featureType & FeatureBonusType.Collect) != FeatureBonusType.Collect)
        {
            return;
        }

        if (fs.InitGemCount != 1)
            return;

        fs.Enter();

        while (fs.UseSpinCount())
        {
            int splitSymbolIndex = symbol.CreateSymbol(fs);
            symbolCollect.CollectCoin(fs, splitSymbolIndex, isRespin: false, out bool hasRedCoin);
            if (hasRedCoin)
            {
                Respin(fs, splitSymbolIndex);
            }

            symbolCollect.CollectGem(fs, splitSymbolIndex, isRespin: false);

            fs.ClearAllSymbols();
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
            fs.ClearSymbolsInMummyArea();

            splitSymbolIdx = symbol.CreateSymbolRespin(fs, splitSymbolIdx);
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
