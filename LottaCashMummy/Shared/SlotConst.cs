namespace LottaCashMummy.Shared;

public class SlotConst
{
    public const byte INVALID_INDEX = 0xFF;

    public const int BASE_ROWS = 3;
    public const int BASE_COLS = 5;
    public const int BASE_TOTAL_POSITION = BASE_ROWS * BASE_COLS;

    public const int FEATURE_ROWS = 5;
    public const int FEATURE_COLS = 5;
    public const int FEATURE_TOTAL_POSITION = FEATURE_ROWS * FEATURE_COLS;

    public const int BET = 80;
    public const int FEATURE_SPIN_COUNT = 5;
    public const int TOTAL_POSITIONS = BASE_ROWS * BASE_COLS;
    public const int MAX_SYMBOL_TYPE = (int)SymbolType.Max;
    public const int MAX_MUMMY_SYMBOL = 1;
    public const int MAX_WIN_GEM_SYMBOL = BASE_ROWS * BASE_COLS;

    public const byte WILD_SYMBOL = (byte)SymbolType.Wild;
    public const byte MUMMY_SYMBOL = (byte)SymbolType.Mummy;
    public const byte GEM_SYMBOL = (byte)SymbolType.Gem;

    public const byte BLANK_FEATURE_SYMBOL = (byte)FeatureSymbolType.Blank;
    public const byte GEM_FEATURE_SYMBOL = (byte)FeatureSymbolType.Gem;
    public const byte COIN_FEATURE_SYMBOL = (byte)FeatureSymbolType.Coin;

    // SlotStats
    public const int PAYTABLE_SYMBOL = 12; // 페이테이블 타입 수 (W, M1, M2, M3, M4, M5, L1, L2, L3, L4, G, M)
    public const int MAX_HITS = 6; // 최대 히트 수 (0, 1, 2, 3, 4, 5)
    public const int MAX_FEATURE_LEVEL = 4; // (1, 2, 3, 4)

    public static int SCREEN_AREA = FEATURE_ROWS * FEATURE_COLS;
}
