namespace LottaCashMummy.Common;

public enum SymbolType : byte
{
    None = 0,
    Wild = 1,
    M1 = 2,
    M2 = 3,
    M3 = 4,
    M4 = 5,
    M5 = 6,
    L1 = 7,
    L2 = 8,
    L3 = 9,
    L4 = 10,
    Gem = 11,
    Mummy = 12,
    Max,
}

public enum JackpotType : byte
{
    None = 0,
    Mini = 1,
    Minor = 2,
    Major = 3,
    Mega = 4,
    Grand = 5
}

public enum GemBonusType : byte
{
    None = 0,
    Collect = 1,
    Spins = 2,
    Symbols = 4,
}

public enum FeatureSymbolType : byte
{
    None = 0,
    Coin = 1,
    Blank = 2,
    Gem = 3,
    Max,
}

public enum FeatureBonusValueType : byte
{
    None = 0,
    Jackpot = 1,
    Spin = 2,
    RedCoin = 3,
    Coin = 4,
    Max,
}

public enum FeatureBonusType : byte
{
    None = 0,        // 0   1개
    Collect = 1 << 0, // 1   1개
    Spins = 1 << 1,  // 2   1개
    Symbols = 1 << 2, // 4   1개
    CollectSpins = Collect | Spins, // 3   2개
    CollectSymbols = Collect | Symbols, // 5   2개
    SpinsSymbols = Spins | Symbols, // 6   2개
    CollectSpinsSymbols = Collect | Spins | Symbols, // 7   3개
}

public enum FeatureBonusCombiType : byte
{
    None,
    // 단일 기능
    CollectWithRedCoin,        // Collect(+RedCoin)
    CollectNoRedCoin,          // Collect(-RedCoin)
    Spins,                     // Spins
    Symbols,                   // Symbols

    // 2개 조합
    CollectSpinsWithRedCoin,   // Collect+Spins(+RedCoin)
    CollectSpinsNoRedCoin,     // Collect+Spins(-RedCoin)
    CollectSymbolsWithRedCoin, // Collect+Symbols(+RedCoin)
    CollectSymbolsNoRedCoin,   // Collect+Symbols(-RedCoin)
    SpinsSymbols,              // Spins+Symbols

    // 3개 조합
    AllFeaturesWithRedCoin,    // Collect+Spins+Symbols(+RedCoin)
    AllFeaturesNoRedCoin,       // Collect+Spins+Symbols(-RedCoin)
    Max
}

//public enum MummyCollectBoxType : byte
//{
//    None = 0,
//    TwoByTwo = 4,
//    ThreeByThree = 9,
//    FourByFour = 16,
//    FiveByFive = 25,
//}



