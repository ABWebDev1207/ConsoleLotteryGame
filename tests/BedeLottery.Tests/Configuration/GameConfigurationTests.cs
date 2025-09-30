using BedeLottery.Configuration;
using Xunit;

namespace BedeLottery.Tests.Configuration;

/// <summary>
/// Tests for game configuration validation and behavior
/// </summary>
public class GameConfigurationTests
{
    [Fact]
    public void Given_ValidConfiguration_When_Validated_Then_NoExceptionIsThrown()
    {
        // Given - Valid configuration
        var config = CreateValidConfiguration();

        // When & Then - Validation should pass
        var exception = Record.Exception(() => config.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Given_InvalidPlayerCount_When_Validated_Then_ThrowsInvalidOperationException()
    {
        // Given - Invalid player count configuration
        var config = CreateValidConfiguration();
        config.Players.MinimumCount = 15;
        config.Players.MaximumCount = 10; // Max less than min

        // When & Then - Should throw exception
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Given_NegativeStartingBalance_When_Validated_Then_ThrowsInvalidOperationException()
    {
        // Given - Negative starting balance
        var config = CreateValidConfiguration();
        config.Players.StartingBalance = -5.00m;

        // When & Then - Should throw exception
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Given_InvalidTicketPrice_When_Validated_Then_ThrowsInvalidOperationException()
    {
        // Given - Invalid ticket price
        var config = CreateValidConfiguration();
        config.Tickets.Price = 0m;

        // When & Then - Should throw exception
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Theory]
    [InlineData(0.40, 0.30, 0.10, 0.10)] // Doesn't sum to 1.0
    [InlineData(0.50, 0.30, 0.10, 0.20)] // Sums to 1.1
    [InlineData(0.30, 0.20, 0.10, 0.10)] // Sums to 0.7
    public void Given_InvalidPrizePercentages_When_Validated_Then_ThrowsInvalidOperationException(
        decimal grand, decimal second, decimal third, decimal house)
    {
        // Given - Invalid prize percentages
        var config = CreateValidConfiguration();
        config.Prizes.GrandPrizePercentage = grand;
        config.Prizes.SecondTierPercentage = second;
        config.Prizes.ThirdTierPercentage = third;
        config.Prizes.HouseProfitPercentage = house;

        // When & Then - Should throw exception
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Theory]
    [InlineData(0.0)] // Zero percentage
    [InlineData(1.1)] // Over 100%
    [InlineData(-0.1)] // Negative percentage
    public void Given_InvalidWinnerPercentages_When_Validated_Then_ThrowsInvalidOperationException(decimal percentage)
    {
        // Given - Invalid winner percentages
        var config = CreateValidConfiguration();
        config.Prizes.SecondTierWinnerPercentage = percentage;

        // When & Then - Should throw exception
        Assert.Throws<InvalidOperationException>(() => config.Validate());
    }

    [Fact]
    public void Given_ValidPrizePercentages_When_Validated_Then_ValidationPasses()
    {
        // Given - Valid prize percentages that sum to 1.0
        var config = CreateValidConfiguration();
        config.Prizes.GrandPrizePercentage = 0.45m;
        config.Prizes.SecondTierPercentage = 0.35m;
        config.Prizes.ThirdTierPercentage = 0.15m;
        config.Prizes.HouseProfitPercentage = 0.05m;

        // When & Then - Should not throw exception
        var exception = Record.Exception(() => config.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Given_ConfigurationSectionName_When_Accessed_Then_ReturnsCorrectValue()
    {
        // Given & When - Accessing section name
        var sectionName = GameConfiguration.SectionName;

        // Then - Should return expected value
        Assert.Equal("GameConfiguration", sectionName);
    }

    [Fact]
    public void Given_DefaultConfiguration_When_Created_Then_HasExpectedDefaults()
    {
        // Given & When - Creating default configuration
        var config = new GameConfiguration();

        // Then - Should have expected default values
        Assert.Equal(10, config.Players.MinimumCount);
        Assert.Equal(15, config.Players.MaximumCount);
        Assert.Equal(10.00m, config.Players.StartingBalance);
        Assert.Equal(1, config.Players.MinTicketsPerPlayer);
        Assert.Equal(10, config.Players.MaxTicketsPerPlayer);
        
        Assert.Equal(1.00m, config.Tickets.Price);
        
        Assert.Equal(0.50m, config.Prizes.GrandPrizePercentage);
        Assert.Equal(0.30m, config.Prizes.SecondTierPercentage);
        Assert.Equal(0.10m, config.Prizes.ThirdTierPercentage);
        Assert.Equal(0.10m, config.Prizes.HouseProfitPercentage);
        Assert.Equal(0.10m, config.Prizes.SecondTierWinnerPercentage);
        Assert.Equal(0.20m, config.Prizes.ThirdTierWinnerPercentage);
        
        Assert.True(config.Display.ShowDetailedResults);
        Assert.True(config.Display.ShowPrizeBreakdown);
        Assert.True(config.Display.ClearScreenBetweenPhases);
    }

    [Theory]
    [InlineData(5, 20, 15.00, 1, 15)] // Custom player settings
    [InlineData(8, 12, 5.00, 2, 8)] // Different custom settings
    public void Given_CustomPlayerConfiguration_When_Validated_Then_AcceptsValidValues(
        int minPlayers, int maxPlayers, decimal startingBalance, int minTickets, int maxTickets)
    {
        // Given - Custom player configuration
        var config = CreateValidConfiguration();
        config.Players.MinimumCount = minPlayers;
        config.Players.MaximumCount = maxPlayers;
        config.Players.StartingBalance = startingBalance;
        config.Players.MinTicketsPerPlayer = minTickets;
        config.Players.MaxTicketsPerPlayer = maxTickets;

        // When & Then - Should validate successfully
        var exception = Record.Exception(() => config.Validate());
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0.5)] // 50 cents
    [InlineData(2.0)] // $2
    [InlineData(0.25)] // 25 cents
    public void Given_CustomTicketPrice_When_Validated_Then_AcceptsValidPrices(decimal price)
    {
        // Given - Custom ticket price
        var config = CreateValidConfiguration();
        config.Tickets.Price = price;

        // When & Then - Should validate successfully
        var exception = Record.Exception(() => config.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Given_DisplayConfiguration_When_Modified_Then_AcceptsAllBooleanValues()
    {
        // Given - Display configuration
        var config = CreateValidConfiguration();

        // When - Modifying display settings
        config.Display.ShowDetailedResults = false;
        config.Display.ShowPrizeBreakdown = false;
        config.Display.ClearScreenBetweenPhases = false;

        // Then - Should validate successfully
        var exception = Record.Exception(() => config.Validate());
        Assert.Null(exception);
        
        Assert.False(config.Display.ShowDetailedResults);
        Assert.False(config.Display.ShowPrizeBreakdown);
        Assert.False(config.Display.ClearScreenBetweenPhases);
    }

    private GameConfiguration CreateValidConfiguration()
    {
        return new GameConfiguration
        {
            Players = new PlayerConfiguration
            {
                MinimumCount = 10,
                MaximumCount = 15,
                StartingBalance = 10.00m,
                MinTicketsPerPlayer = 1,
                MaxTicketsPerPlayer = 10
            },
            Tickets = new TicketConfiguration
            {
                Price = 1.00m
            },
            Prizes = new PrizeConfiguration
            {
                GrandPrizePercentage = 0.50m,
                SecondTierPercentage = 0.30m,
                ThirdTierPercentage = 0.10m,
                HouseProfitPercentage = 0.10m,
                SecondTierWinnerPercentage = 0.10m,
                ThirdTierWinnerPercentage = 0.20m
            },
            Display = new DisplayConfiguration
            {
                ShowDetailedResults = true,
                ShowPrizeBreakdown = true,
                ClearScreenBetweenPhases = true
            }
        };
    }
}
