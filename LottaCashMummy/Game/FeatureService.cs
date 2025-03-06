using LottaCashMummy.Buffer;
using LottaCashMummy.Common;

namespace LottaCashMummy.Game;

public class FeatureService
{
    private readonly FeatureSetup setup;
    private readonly FeatureSymbolCreate symbol;
    private readonly FeatureSymbolCollect symbolCollect;

    private readonly IFeatureData fd;

    public FeatureService(IFeatureData featureData)
    {
        symbol = new FeatureSymbolCreate(featureData);

        var mummy = new FeatureMummy(featureData);
        setup = new FeatureSetup(featureData, mummy);
        symbolCollect = new FeatureSymbolCollect(mummy);
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

        // while (fs.UseSpinCount())
        // {
        //     for (int idx = 0; idx < SlotConst.SCREEN_AREA; idx++)
        //     {
        //         if (fs.IsActiveMummyArea(idx))  // Mummy Area
        //         {
        //             if (fd.Symbol.GetRollSymbolInScreenArea(fs.Mummy.Level, fs.SymbolRng) == FeatureSymbolType.Gem)
        //             {
        //                 fs.AddSymbol(idx, FeatureSymbolType.Gem, 0, 1);
        //             }

        //             // var n = fs.SymbolRng.Next(1, 101);
        //             // if (n <= 5)
        //             //     fs.AddSymbol(i, FeatureSymbolType.Gem, 0, 1);
                    
        //             break;
        //         }
        //     }

        //     fs.ClearAllSymbols();
        // }

        // while (fs.UseSpinCount())
        // {
        //     int splitSymbolIndex = symbol.CreateSymbol(fs);
        //     // symbolCollect.CollectCoin(fs, splitSymbolIndex, isRespin: false, out bool hasRedCoin);
        //     // if (hasRedCoin)
        //     // {
        //     //     Respin(fs, splitSymbolIndex);
        //     // }

        //     // symbolCollect.CollectGem(fs, splitSymbolIndex, isRespin: false);

        //     fs.ClearAllSymbols();
        // }

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
