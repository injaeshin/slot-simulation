
using LottaCashMummy.Common;

namespace LottaCashMummy.Model;

public class BaseSpinModel
{
    public int SpinCount { get; set; }
}

public class BasePayWinModel
{
    public int SymbolType { get; set; }

    public int Hit { get; set; }

    public long Amount { get; set; }
}

