using WheelOfFortune.Table;
using WheelOfFortune.Shared;

namespace WheelOfFortune.Tests;

public class PayTableTests
{
    [Fact]
    public void GetPay_ReturnsCorrectPay_ForValidCombination()
    {
        // Arrange
        var payTable = new PayTable();
        var symbols = new SymbolType[] { SymbolType.Wild2x, SymbolType.Wild5x, SymbolType.Wild2x };

        // Act
        var pay = payTable.GetPay(symbols);

        // Assert
        Assert.Equal((int)CombinationPayType.Wild2x5x2x, pay.pay);
        Assert.Equal(CombinationPayType.Wild2x5x2x, pay.combinationPayType);
    }

    [Fact]
    public void GetPay_ReturnsZero_ForInvalidCombination()
    {
        // Arrange
        var payTable = new PayTable();
        var symbols = new SymbolType[] { SymbolType.Blank, SymbolType.Blank, SymbolType.Blank };

        // Act
        var pay = payTable.GetPay(symbols);

        // Assert
        Assert.Equal(0, pay.pay);
        Assert.Equal(CombinationPayType.None, pay.combinationPayType);
    }

    [Fact]
    public void GetPay_ReturnsCorrectPay_WithMultiplier()
    {
        // Arrange
        var payTable = new PayTable();
        var symbols = new SymbolType[] { SymbolType.Seven, SymbolType.Wild2x, SymbolType.Seven };

        // Act
        var pay = payTable.GetPay(symbols);

        // Assert
        Assert.Equal((int)CombinationPayType.Three7 * 2, pay.pay); // Assuming the multiplier is 2 for Wild2x
        Assert.Equal(CombinationPayType.Three7, pay.combinationPayType);
    }

    [Theory]
    [InlineData(new SymbolType[] { SymbolType.Wild2x, SymbolType.Wild5x, SymbolType.Wild2x }, (int)CombinationPayType.Wild2x5x2x, CombinationPayType.Wild2x5x2x)]
    [InlineData(new SymbolType[] { SymbolType.Blank, SymbolType.Blank, SymbolType.Blank }, 0, CombinationPayType.None)]
    [InlineData(new SymbolType[] { SymbolType.Seven, SymbolType.Wild2x, SymbolType.Seven }, (int)CombinationPayType.Three7 * 2, CombinationPayType.Three7)]
    [InlineData(new SymbolType[] { SymbolType.SevenBar, SymbolType.Wild2x, SymbolType.SevenBar }, (int)CombinationPayType.Three7Bar, CombinationPayType.Three7Bar)]
    [InlineData(new SymbolType[] { SymbolType.ThreeBar, SymbolType.Wild2x, SymbolType.ThreeBar }, (int)CombinationPayType.Three3Bar * 2, CombinationPayType.Three3Bar)]
    public void GetPay_ReturnsExpectedPay(SymbolType[] symbols, int expectedPay, CombinationPayType expectedCombinationPayType)
    {
        // Arrange
        var payTable = new PayTable();

        // Act
        var pay = payTable.GetPay(symbols);

        // Assert
        Assert.Equal(expectedPay, pay.pay);
        Assert.Equal(expectedCombinationPayType, pay.combinationPayType);
    }
}
