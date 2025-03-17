
namespace SpinOfFortune.Shared;

public enum SymbolType
{
    Blank,
    Wild5x,
    Wild4x,
    Wild3x,
    Wild2x,
    Seven,
    SevenBar,
    OneBar,
    TwoBar,
    ThreeBar,
    Bonus,
    Max
}

public enum CombinationPayType
{
    None,
    Wild2x5x2x = 3000,
    Wild2x4x2x = 2000,
    Wild2x3x2x = 1500,
    Wild2x2x2x = 1000,
    Three7 = 50,
    Three7Bar = 40,
    Three3Bar = 30,
    AnyThree7 = 25,
    Three2Bar = 20,
    One2xOne5x = 20,
    One2xOne4x = 16,
    One2xOne3x = 12,
    Three1Bar = 10,
    Two2x = 8,
    AnyThreeBar = 5,
    AnyOne5x = 5,
    AnyOne4x = 4,
    AnyOne3x = 3,
    AnyOne2x = 2,
    Max
}