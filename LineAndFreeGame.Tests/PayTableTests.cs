using System.Text.Json;
using LineAndFree.Shared;
using LineAndFree.Table;

namespace LineAndFree.Tests;

public class PayTableTests
{
    private GameDataLoader GetTestGameDataLoader()
    {
        var payTableData = new Dictionary<string, object>
        {
            { "Symbol", new[] { "WW", "AA", "BB", "CC", "DD", "EE", "FF", "GG", "HH", "II", "JJ", "SS" } },
            { "5 of a Kind", new[] { "", "300", "250", "100", "100", "80", "50", "25", "25", "25", "25", "20" } },
            { "4 of a Kind", new[] { "", "100", "80", "50", "50", "30", "15", "10", "10", "10", "10", "15" } },
            { "3 of a Kind", new[] { "", "20", "20", "20", "20", "15", "5", "5", "5", "5", "5", "10" } }
        };

        var gameDataLoader = new GameDataLoader
        {
            { "PayTable", JsonSerializer.Serialize(payTableData) }
        };

        return gameDataLoader;
    }

    [Fact]
    public void Constructor_ShouldInitializePayTable_WhenValidDataProvided()
    {
        // Arrange
        var gameDataLoader = GetTestGameDataLoader();

        // Act
        var payTable = new PayTable(gameDataLoader);

        // Assert
        Assert.NotNull(payTable);
    }

    [Theory]
    [InlineData(new[] { SymbolType.BB, SymbolType.BB, SymbolType.BB, SymbolType.CC, SymbolType.WW }, SymbolType.BB, 3, 20)]
    [InlineData(new[] { SymbolType.WW, SymbolType.BB, SymbolType.BB, SymbolType.CC, SymbolType.CC }, SymbolType.BB, 3, 20)]
    [InlineData(new[] { SymbolType.BB, SymbolType.WW, SymbolType.BB, SymbolType.CC, SymbolType.WW }, SymbolType.BB, 3, 20)]
    [InlineData(new[] { SymbolType.BB, SymbolType.BB, SymbolType.WW, SymbolType.CC, SymbolType.CC }, SymbolType.BB, 3, 20)]
    [InlineData(new[] { SymbolType.WW, SymbolType.WW, SymbolType.BB, SymbolType.CC, SymbolType.CC }, SymbolType.BB, 3, 20)] // 와일드 2개 + BB 1개 (앞쪽)
    [InlineData(new[] { SymbolType.WW, SymbolType.BB, SymbolType.WW, SymbolType.CC, SymbolType.CC }, SymbolType.BB, 3, 20)] // 와일드 2개 + BB 1개 (중간)
    [InlineData(new[] { SymbolType.BB, SymbolType.WW, SymbolType.WW, SymbolType.CC, SymbolType.CC }, SymbolType.BB, 3, 20)] // BB 1개 + 와일드 2개 (뒤쪽)
    public void CalculatePay_ShouldReturnCorrectPay_WhenValidSymbolsProvided(SymbolType[] symbols, SymbolType expectedSymbol, int expectedCount, int expectedPay)
    {
        // Arrange
        var gameDataLoader = GetTestGameDataLoader();
        var payTable = new PayTable(gameDataLoader);
        var middleSymbols = new Span<SymbolType>(symbols);

        // Act
        var result = payTable.CalculatePay(middleSymbols);

        // Assert
        Assert.Equal((expectedSymbol, expectedCount, expectedPay), result);
    }
}
