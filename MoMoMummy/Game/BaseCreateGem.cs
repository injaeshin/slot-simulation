
using MoMoMummy.ThreadStorage;
using MoMoMummy.Shared;
using MoMoMummy.Table;

namespace MoMoMummy.Game;

public class BaseCreateGem
{
    private readonly IBaseData baseData;
    private readonly IJackpotData jackpotData;

    public BaseCreateGem(IBaseData baseData, IJackpotData jackpotData)
    {
        this.baseData = baseData;
        this.jackpotData = jackpotData;
    }

    public void CreateGem(BaseStorage bs, Random random, FeatureBonusTrigger featureTrigger)
    {
        if (featureTrigger.Type > FeatureBonusType.None)
        {
            // 1. 일반 심볼을 Gem으로 변환
            ConvertNormalToGemSymbols(bs, random, featureTrigger.Quantity);

            // 2. Gem 속성 설정
            SetGemAttributes(bs, random, featureTrigger.Type);

            // 3. Gem 값 설정
            SetGemValues(bs, random);
        }
        else if (bs.HasMummySymbol)
        {
            // 1. Gem 속성 설정 (무작위)
            SetRandomGemAttributes(bs, random);

            // 2. Gem 값 설정
            SetGemValues(bs, random);
        }
    }

        private void ConvertNormalToGemSymbols(BaseStorage bs, Random random, int targetCount)
    {
        var remainingCount = targetCount - bs.GemCount;
        while (remainingCount > 0)
        {
            var randomNormalIdx = random.Next(bs.NormalCount);
            bs.SwapNormalToGemSymbol((byte)randomNormalIdx);
            remainingCount--;
        }
    }

    private void SetGemAttributes(BaseStorage bs, Random random, FeatureBonusType featureType)
    {
        Span<GemBonusType> bonusTypes = stackalloc GemBonusType[bs.GemCount];
        var bonusTypeCount = featureType.GetGemAttributeType(bonusTypes);

        // 먼저 정해진 속성 수만큼 설정
        for (int i = 0; i < bonusTypeCount; i++)
        {
            var index = bs.GetGemIndex((byte)i);
            bs.SetGemAttribute(index, bonusTypes[i]);
        }

        // 남은 Gem들에 대해 임의의 속성 부여
        for (int i = bonusTypeCount; i < bs.GemCount; i++)
        {
            var index = bs.GetGemIndex((byte)i);
            bs.SetGemAttribute(index, baseData.BaseSymbol.GetRollGemBonusType(random));
        }
    }

    private void SetRandomGemAttributes(BaseStorage bs, Random random)
    {
        for (int i = 0; i < bs.GemCount; i++)
        {
            var index = bs.GetGemIndex((byte)i);
            bs.SetGemAttribute(index, baseData.BaseSymbol.GetRollGemBonusType(random));
        }
    }

    private void SetGemValues(BaseStorage bs, Random random)
    {
        for (int i = 0; i < bs.GemCount; i++)
        {
            var index = bs.GetGemIndex((byte)i);
            var value = baseData.BaseSymbol.GetRollGemCredit(random);
            var jackpotType = JackpotType.None;
            
            if (jackpotData.Jackpot.TryGetJackpotType((int)value, out var jType))
            {
                jackpotType = jType;
            }
            
            bs.SetGemValue(index, value, jackpotType);
        }
    }
}   
