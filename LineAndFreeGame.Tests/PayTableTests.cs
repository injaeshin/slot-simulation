using System.Text.Json;
using LineAndFreeGame.Common;
using LineAndFreeGame.Table;

namespace LineAndFreeGame.Tests;

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
    [InlineData(new[] { SymbolType.AA, SymbolType.AA, SymbolType.AA, SymbolType.AA, SymbolType.AA }, SymbolType.AA, 5, 300)]
    [InlineData(new[] { SymbolType.BB, SymbolType.BB, SymbolType.BB, SymbolType.BB, SymbolType.BB }, SymbolType.BB, 5, 250)]
    [InlineData(new[] { SymbolType.CC, SymbolType.CC, SymbolType.CC, SymbolType.CC, SymbolType.CC }, SymbolType.CC, 5, 100)]
    [InlineData(new[] { SymbolType.DD, SymbolType.DD, SymbolType.DD, SymbolType.DD, SymbolType.DD }, SymbolType.DD, 5, 100)]
    [InlineData(new[] { SymbolType.EE, SymbolType.EE, SymbolType.EE, SymbolType.EE, SymbolType.EE }, SymbolType.EE, 5, 80)]
    [InlineData(new[] { SymbolType.FF, SymbolType.FF, SymbolType.FF, SymbolType.FF, SymbolType.FF }, SymbolType.FF, 5, 50)]
    [InlineData(new[] { SymbolType.GG, SymbolType.GG, SymbolType.GG, SymbolType.GG, SymbolType.GG }, SymbolType.GG, 5, 25)]
    [InlineData(new[] { SymbolType.HH, SymbolType.HH, SymbolType.HH, SymbolType.HH, SymbolType.HH }, SymbolType.HH, 5, 25)]
    [InlineData(new[] { SymbolType.II, SymbolType.II, SymbolType.II, SymbolType.II, SymbolType.II }, SymbolType.II, 5, 25)]
    [InlineData(new[] { SymbolType.JJ, SymbolType.JJ, SymbolType.JJ, SymbolType.JJ, SymbolType.JJ }, SymbolType.JJ, 5, 25)]
    [InlineData(new[] { SymbolType.SS, SymbolType.SS, SymbolType.SS, SymbolType.SS, SymbolType.SS }, SymbolType.SS, 5, 20)]
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
