
using LottaCashMummy.Buffer;

namespace LottaCashMummy.Game;

public class FeatureService
{
    private readonly FeatureSetup setup;
    private readonly FeatureSymbol symbol;
    private readonly FeatureSymbolCollect symbolCollect;
    public FeatureService(IFeatureData featureData)
    {
        symbol = new FeatureSymbol(featureData);

        var mummy = new FeatureMummy(featureData);
        setup = new FeatureSetup(featureData, mummy);
        symbolCollect = new FeatureSymbolCollect(mummy);
    }

    public void Execute(ThreadLocalStorage tls)
    {
        Init(tls.BaseStorage, tls.FeatureStorage, tls.Random);
        Spin(tls.FeatureStorage);
    }

    private void Init(BaseStorage bs, FeatureStorage fs, Random random)
    {
        setup.Init(bs, fs, random);
        fs.Enter();
    }

    // private static byte[] requiredGemToNextLevel = [0, 5, 4, 3, 255];
    // private static byte[] bonusSpinCount = [0, 1, 2, 3, 4];
    // private static byte[] ignoreSlot = [0, 25 - 4, 25 - 9, 25 - 16, 25];
    // private static byte[] levelByProbability = [0, 3, 3, 2, 0];

    private void Spin(FeatureStorage fs)
    {
        if (fs.GemCount > 0)
        {
            symbolCollect.ForceRemoveGem(fs);
            fs.ClearSymbolInScreenArea();
        }

        while (fs.UseSpinCount())
        {
            (int gemCount, int coinCount) = symbol.CreateSymbol(fs);

            if (gemCount >0)
            {
                symbolCollect.RemoveGem(fs);
            }

            if (coinCount > 0)
            {
                symbolCollect.RemoveCoin(fs);
            }

            // 심볼 생성
            //symbol.CreateWithMummyArea(fs, random, isRespin: false);
            //symbol.CreateGem(fs, random);

            // 머미 영역 심볼 획득
            //var hasRedCoin = symbolCollect.CollectCoinInMummyArea(fs, isRespin: false);
            //if (hasRedCoin)
            //{
            //    Respin(fs, random);
            //}

            //symbolCollect.CollectGem(fs);

            // 초기화
            fs.ClearSymbolInScreenArea();
        }

        //var value = new double[] { 500, 100, 80, 60, 50, 0, 40, 30, 20, 15, 12.5, 10, 8, 5, 4, 3, 2, 1, 0.5 };
        //var weight = new int[] { 1, 4, 10, 10, 15, 15, 15, 15, 15, 15, 15, 15, 20, 20, 20, 20, 30, 300, 500 };
        //var cumulativeWeight = new int[value.Length];
        //cumulativeWeight[0] = weight[0];
        //for (int i = 1; i < value.Length; i++)
        //{
        //    cumulativeWeight[i] = cumulativeWeight[i - 1] + weight[i];
        //}

        ////int level = 1;
        //var spinCount = 0;
        ////var gemCount = 0;

        ////var coinCount = 0;
        ////var coinValue = 0.0;

        //var screenLength = 21;
        ////var ignoreSlot = new byte[] { 0, 25 - 4, 25 - 9, 25 - 16, 25 };
        ////var levelByProbability = new byte[] { 0, 3, 3, 2, 0 };

        //while (fs.UseSpinCount())
        //{
        //    spinCount++;
        //    fs.AddSpinCount();

        //    for (byte idx = 0; idx < screenLength; idx++)
        //    {
        //        // if (ignoreSlot[level] < idx)
        //        //     continue;

        //        var n = random.Next(0, 100);

        //        if (n < 5)//levelByProbability[level])
        //        {
        //            //gemCount++;
        //            fs.TestAddGemCount();
        //            var cv = random.Next(0, cumulativeWeight[cumulativeWeight.Length - 1]);

        //            int selectedIndex = -1;
        //            for (int i = 0; i < cumulativeWeight.Length; i++)
        //            {
        //                if (cv < cumulativeWeight[i])
        //                {
        //                    selectedIndex = i;
        //                    break;
        //                }
        //            }

        //            if (selectedIndex == -1)
        //            {
        //                throw new Exception($"Invalid cumulative weight {cv}");
        //            }

        //            var selectedValue = value[selectedIndex];
        //            fs.TestAddGemValue(selectedValue);
        //        }
        //        // coin은 5+17 = 22
        //        else if (n < 22)
        //        {
        //            //coinCount++;
        //            fs.TestAddCoinCount();
        //            var cv = random.Next(0, cumulativeWeight[cumulativeWeight.Length - 1]);

        //            int selectedIndex = -1;
        //            for (int i = 0; i < cumulativeWeight.Length; i++)
        //            {
        //                if (cv < cumulativeWeight[i])
        //                {
        //                    selectedIndex = i;
        //                    break;
        //                }
        //            }

        //            if (selectedIndex == -1)
        //            {
        //                throw new Exception($"Invalid cumulative weight {cv}");
        //            }

        //            var selectedValue = value[selectedIndex];
        //            fs.TestAddCoinValue(selectedValue);
        //            //coinValue += selectedValue;
        //        }
        //    }
        //}
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

            var hasRedCoin = symbolCollect.CollectCoinInMummyArea(fs, isRespin: true);
            if (hasRedCoin)
            {
                throw new Exception("Red coin found");
            }
        } while (true);
    }
}
