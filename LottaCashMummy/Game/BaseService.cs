using LottaCashMummy.Buffer;
using LottaCashMummy.Common;
using LottaCashMummy.Table;
using System.Runtime.CompilerServices;

namespace LottaCashMummy.Game;

public class BaseService
{
    private readonly IBaseData baseData;
    private readonly IJackpotData jackpotData;

    private readonly BaseSpin baseSpin;
    private readonly BasePayout basePayout;
    private readonly BaseCreateGem baseTrigger;
    private readonly FeatureService featureService;

    FeatureBonusTrigger featureTrigger = new FeatureBonusTrigger(FeatureBonusType.None, 0, 0);

    public BaseService(IBaseData baseData, IFeatureData featureData, IJackpotData jackpotData)
    {
        this.baseData = baseData;
        this.jackpotData = jackpotData;

        baseSpin = new BaseSpin(baseData);
        basePayout = new BasePayout(baseData.PayTable);
        baseTrigger = new BaseCreateGem(baseData, jackpotData);
        featureService = new FeatureService(featureData);
    }

    public void SimulateSingleSpin(ThreadLocalStorage buffer)
    {
        buffer.BaseStorage.Clear();
        buffer.BaseStorage.AddSpinCount();

        //var featureTrigger = baseData.BaseSymbol.GetRollBonusTrigger(buffer.Random);

        if (!baseSpin.Spin(buffer.BaseStorage, buffer.Random, featureTrigger))
        {
            throw new Exception("Spin failed");
        }

        baseTrigger.CreateGem(buffer.BaseStorage, buffer.Random, featureTrigger);

        if (ValidateBonusTrigger(buffer.BaseStorage, featureTrigger))
        {
            featureService.Execute(buffer);
        }

        basePayout.CalculatePayout(buffer.BaseStorage);
    }

    public static bool ValidateBonusTrigger(BaseStorage bs, FeatureBonusTrigger trigger)
    {
        if (trigger.Type <= FeatureBonusType.None && (!bs.HasMummySymbol || bs.GemCount == 0))
        {
            return false;
        }

        byte combinedAttribute = 0;
        for (int i = 0; i < bs.GemCount; i++)
        {
            var index = bs.GetGemIndex((byte)i);
            bs.AddWinGemSymbol(index);

            if (trigger.Type <= FeatureBonusType.None)
            {
                combinedAttribute |= bs.GetAttribute(index);
            }
        }

        var tp = trigger.Type > FeatureBonusType.None ? (byte)trigger.Type : combinedAttribute;
        bs.SetFeatureBonusType(tp);

        return true;
    }
}
