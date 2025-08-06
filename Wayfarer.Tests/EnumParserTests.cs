using System;
using Xunit;
/// <summary>
/// Tests for the EnumParser utility to ensure it works correctly.
/// </summary>
public class EnumParserTests
{
    [Fact]
    public void TryParse_Should_Parse_Valid_Enum_Values()
    {
        // Arrange & Act & Assert
        Assert.True(EnumParser.TryParse<ItemCategory>("Trade_Goods", out ItemCategory category));
        Assert.Equal(ItemCategory.Trade_Goods, category);

        Assert.True(EnumParser.TryParse<ItemCategory>("TRADE_GOODS", out ItemCategory category2, ignoreCase: true));
        Assert.Equal(ItemCategory.Trade_Goods, category2);
    }

    [Fact]
    public void TryParse_Should_Handle_Spaces_In_Enum_Names()
    {
        // Arrange & Act & Assert
        Assert.True(EnumParser.TryParse<ItemCategory>("Trade Goods", out ItemCategory category));
        Assert.Equal(ItemCategory.Trade_Goods, category);
    }

    [Fact]
    public void TryParse_Should_Return_False_For_Invalid_Values()
    {
        // Arrange & Act & Assert
        Assert.False(EnumParser.TryParse<ItemCategory>("InvalidCategory", out _));
        Assert.False(EnumParser.TryParse<ItemCategory>("", out _));
        Assert.False(EnumParser.TryParse<ItemCategory>(null, out _));
    }

    [Fact]
    public void Parse_Should_Throw_For_Invalid_Values()
    {
        // Arrange & Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            EnumParser.Parse<ItemCategory>("InvalidCategory", "TestField"));

        Assert.Contains("Invalid value 'InvalidCategory' for TestField", ex.Message);
        Assert.Contains("Trade_Goods", ex.Message); // Should list valid values
    }

    [Fact]
    public void ParseList_Should_Parse_Multiple_Values()
    {
        // Arrange
        string[] values = new[] { "Trade_Goods", "Tools", "Food" };

        // Act
        List<ItemCategory> result = EnumParser.ParseList<ItemCategory>(values, "categories");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(ItemCategory.Trade_Goods, result);
        Assert.Contains(ItemCategory.Tools, result);
        Assert.Contains(ItemCategory.Food, result);
    }

    [Fact]
    public void ParseList_Should_Throw_For_Any_Invalid_Values()
    {
        // Arrange
        string[] values = new[] { "Trade_Goods", "InvalidCategory", "Tools" };

        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            EnumParser.ParseList<ItemCategory>(values, "categories"));

        Assert.Contains("Invalid values for categories: InvalidCategory", ex.Message);
    }

    [Fact]
    public void TryParseList_Should_Skip_Invalid_Values()
    {
        // Arrange
        string?[] values = new[] { "Trade_Goods", "InvalidCategory", "Tools", "", null };

        // Act
        List<ItemCategory> result = EnumParser.TryParseList<ItemCategory>(values);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(ItemCategory.Trade_Goods, result);
        Assert.Contains(ItemCategory.Tools, result);
    }
}