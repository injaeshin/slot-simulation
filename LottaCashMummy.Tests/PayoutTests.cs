using Xunit;
using LottaCashMummy.Table;
using LottaCashMummy.Common;
using LottaCashMummy.Game;
using LottaCashMummy.Buffer;

namespace LottaCashMummy.Tests;

public class TestPayTable : IPayTable
{
    private readonly Dictionary<byte, int[]> payTable;

    public TestPayTable()
    {
        payTable = new Dictionary<byte, int[]>
        {
            { (byte)SymbolType.Wild, new[] { 0, 0, 20, 80, 200 } },
            { (byte)SymbolType.M1, new[] { 0, 0, 15, 50, 150 } },
            { (byte)SymbolType.M2, new[] { 0, 0, 15, 50, 150 } },
            { (byte)SymbolType.M3, new[] { 0, 0, 15, 50, 150 } },
            { (byte)SymbolType.M4, new[] { 0, 0, 15, 50, 150 } },
            { (byte)SymbolType.M5, new[] { 0, 0, 15, 50, 150 } },
            { (byte)SymbolType.L1, new[] { 0, 0, 10, 30, 100 } },
            { (byte)SymbolType.L2, new[] { 0, 0, 10, 30, 100 } },
            { (byte)SymbolType.L3, new[] { 0, 0, 10, 30, 100 } },
            { (byte)SymbolType.L4, new[] { 0, 0, 10, 30, 100 } },
            { (byte)SymbolType.Gem, new[] { 0, 0, 0, 0, 0 } },
            { (byte)SymbolType.Mummy, new[] { 0, 0, 0, 0, 0 } }
        };
    }

    public int GetPayout(byte symbolType, int hitCount)
    {
        if (hitCount < 1 || hitCount > 5) 
            return 0;

        return payTable.TryGetValue(symbolType, out var pays) 
            ? pays[hitCount - 1] 
            : 0;
    }
}

public class PayoutTests
{
    private readonly BasePayout payout;
    private readonly SlotStats spinStats;

    public PayoutTests()
    {
        spinStats = new SlotStats();
        payout = new BasePayout(new TestPayTable());
    }

    [Theory]
    [InlineData(new byte[] { 1, 1, 2, 2, 2 }, 2, true, 2, 5)]  // Wild Wild M1 M1 M1 -> M1 5개
    [InlineData(new byte[] { 1, 1, 1, 2, 2 }, 3, true, 2, 5)]  // Wild Wild Wild M1 M1 -> M1 5개
    [InlineData(new byte[] { 1, 1, 2, 3, 3 }, 2, true, 2, 4)]  // Wild Wild M1 M2 M2 -> M1 4개
    [InlineData(new byte[] { 1, 1, 12, 2, 2 }, 2, false, 0, 0)]  // Wild Wild MM M1 M1 -> 페이 없음
    [InlineData(new byte[] { 1, 1, 11, 2, 2 }, 2, false, 0, 0)]  // Wild Wild GEM M1 M1 -> 페이 없음
    [InlineData(new byte[] { 1, 1, 1, 12, 2 }, 3, false, 0, 0)]  // Wild Wild Wild MM M1 -> 페이 없음
    public void CheckNormalCombinationWithWildSubstitution_ReturnsExpectedResult(
        byte[] symbols, 
        int wildCount, 
        bool expectedShouldPay, 
        byte expectedSymbol, 
        int expectedCount)
    {
        // Act
        var result = payout.CheckNormalCombinationWithWildSubstitution(symbols.AsSpan()[wildCount..], wildCount);

        // Assert
        Assert.Equal(expectedShouldPay, result.ShouldPay);
        if (expectedShouldPay)
        {
            Assert.Equal(expectedSymbol, result.Symbol);
            Assert.Equal(expectedCount, result.Count);
        }
    }
} 