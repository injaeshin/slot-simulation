using System.Runtime.InteropServices;
using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using Microsoft.VisualBasic;

namespace LottaCashMummy.Game;

public class FeatureService
{
    private readonly FeatureSetup setup;
    private readonly FeatureSymbol symbol;
    private readonly FeatureSymbolCollect symbolCollect;

    public FeatureService(IFeatureData featureData, IJackpotData jackpotData)
    {
        symbol = new FeatureSymbol(featureData);

        var mummy = new FeatureMummy(featureData);
        setup = new FeatureSetup(featureData, mummy);
        symbolCollect = new FeatureSymbolCollect(mummy);
    }

    public void Execute(ThreadLocalStorage tls)
    {
        Init(tls.BaseStorage, tls.FeatureStorage, tls.Random);
        Spin(tls.FeatureStorage, tls.Random);
    }

    private void Init(BaseStorage bs, FeatureStorage fs, Random random)
    {
        setup.Init(bs, fs, random);
        fs.FeatureEnter();
    }

    // private static byte[] requiredGemToNextLevel = [0, 5, 4, 3, 255];
    // private static byte[] bonusSpinCount = [0, 1, 2, 3, 4];
    // private static byte[] ignoreSlot = [0, 25 - 4, 25 - 9, 25 - 16, 25];
    // private static byte[] levelByProbability = [0, 3, 3, 2, 0];

    private void Spin(FeatureStorage fs, Random random)
    {
        if (fs.GemCount > 0)
        {
            symbolCollect.CollectGemScreenArea(fs);
        }

        while (fs.UseSpinCount())
        {
            // 심볼 생성
            symbol.CreateWithMummyArea(fs, random, isRespin: false);
            symbol.CreateWithoutMummyArea(fs, random);

            // 머미 영역 심볼 획득
            var hasRedCoin = symbolCollect.CollectCoinInMummyArea(fs);
            if (hasRedCoin)
            {
                Respin(fs, random);
            }

            symbolCollect.CollectGemScreenArea(fs);

            // 초기화
            fs.ClearSymbolInScreenArea();
        }

        // int level = 1;
        // var gemCount = 0;
        // var spinCount = 0;

        // fs.SpinStats.Feature.AddTestEnterCount();

        // while (fs.UseSpinCount())
        // {
        //     spinCount++;
        //     fs.SpinStats.Feature.AddTestTotalSpinCount(level);

        //     for (byte idx = 0; idx < screenLength; idx++)
        //     {
        //         if (ignoreSlot[level] < idx)
        //             continue;

        //         if (level < 4 && random.Next(1, 101) <= levelByProbability[level])
        //         {
        //             gemCount++;
        //             fs.SpinStats.Feature.AddTestTotalCreateGemCount(level);
        //         }

        //         if (gemCount == requiredGemToNextLevel[level])
        //         {
        //             fs.SpinStats.Feature.AddTestLevelEnterCount(level);
        //             fs.SpinStats.Feature.AddTestTotalLevelUpSuccessSpinCount(level);
        //             fs.AddBonusSpinCount(bonusSpinCount[level]);
        //             spinCount = 0;
        //             gemCount = 0;
        //             level++;
        //         }
        //     }
        // }
    }

    private void Respin(FeatureStorage fs, Random random)
    {
        do
        {
            fs.AddRespinCount();
            fs.ClearSymbolInMummyArea();

            symbol.CreateWithMummyArea(fs, random, isRespin: true);
            if (fs.CoinCount == 0)
            {
                break;
            }

            var hasRedCoin = symbolCollect.CollectCoinInMummyArea(fs);
            if (hasRedCoin)
            {
                throw new Exception("Red coin found");
            }
        } while (true);
    }
}
