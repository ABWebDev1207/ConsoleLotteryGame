using BedeLottery.Configuration;

namespace BedeLottery.Tests.TestHelpers;

/// <summary>
/// Helper class for creating test configurations to eliminate duplication
/// </summary>
public static class ConfigurationTestHelper
{
    /// <summary>
    /// Creates a default game configuration for testing
    /// </summary>
    public static GameConfiguration CreateDefaultGameConfiguration()
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

    /// <summary>
    /// Creates a custom game configuration for testing edge cases
    /// </summary>
    public static GameConfiguration CreateCustomGameConfiguration(
        int minPlayers = 5,
        int maxPlayers = 20,
        decimal startingBalance = 15.00m,
        decimal ticketPrice = 1.00m,
        decimal grandPrizePercentage = 0.60m,
        decimal secondTierPercentage = 0.25m,
        decimal thirdTierPercentage = 0.10m,
        decimal houseProfitPercentage = 0.05m)
    {
        return new GameConfiguration
        {
            Players = new PlayerConfiguration
            {
                MinimumCount = minPlayers,
                MaximumCount = maxPlayers,
                StartingBalance = startingBalance,
                MinTicketsPerPlayer = 1,
                MaxTicketsPerPlayer = 15
            },
            Tickets = new TicketConfiguration 
            { 
                Price = ticketPrice 
            },
            Prizes = new PrizeConfiguration
            {
                GrandPrizePercentage = grandPrizePercentage,
                SecondTierPercentage = secondTierPercentage,
                ThirdTierPercentage = thirdTierPercentage,
                HouseProfitPercentage = houseProfitPercentage,
                SecondTierWinnerPercentage = 0.15m,
                ThirdTierWinnerPercentage = 0.25m
            },
            Display = new DisplayConfiguration
            {
                ShowDetailedResults = true,
                ShowPrizeBreakdown = true,
                ClearScreenBetweenPhases = false
            }
        };
    }

    /// <summary>
    /// Creates an invalid configuration for testing validation
    /// </summary>
    public static GameConfiguration CreateInvalidGameConfiguration()
    {
        return new GameConfiguration
        {
            Players = new PlayerConfiguration
            {
                MinimumCount = 15, // Invalid: min > max
                MaximumCount = 10,
                StartingBalance = -5.00m, // Invalid: negative balance
                MinTicketsPerPlayer = 5, // Invalid: min > max
                MaxTicketsPerPlayer = 3
            },
            Tickets = new TicketConfiguration 
            { 
                Price = 0m // Invalid: zero price
            },
            Prizes = new PrizeConfiguration
            {
                GrandPrizePercentage = 0.40m,
                SecondTierPercentage = 0.30m,
                ThirdTierPercentage = 0.10m,
                HouseProfitPercentage = 0.10m, // Invalid: doesn't sum to 1.0
                SecondTierWinnerPercentage = 1.5m, // Invalid: > 1.0
                ThirdTierWinnerPercentage = -0.1m // Invalid: negative
            },
            Display = new DisplayConfiguration()
        };
    }
}
