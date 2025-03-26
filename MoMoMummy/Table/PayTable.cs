using System.Runtime.CompilerServices;
using MoMoMummy.Shared;

namespace MoMoMummy.Table;

public interface IPayTable
{
    int GetPayout(byte symbolType, int hitCount);
}

public class PayTable : IPayTable
{
    private readonly Dictionary<byte, int[]> payTable;

    public PayTable(GameDataLoader kv)
    {
        var parser = new PayTableModelParser();
        payTable = parser.ReadPayTable(kv);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetPayout(byte symbolType, int hitCount)
    {
        if (hitCount < 1 || hitCount > 5) 
            return 0;

        return payTable.TryGetValue(symbolType, out var pays) 
            ? pays[hitCount - 1] 
            : 0;
    }
}
