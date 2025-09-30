using BedeLottery.Models;
using BedeLottery.Services;
using BedeLottery.Interfaces;
using BedeLottery.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BedeLottery.Tests.Services;

public class PrizeCalculationServiceTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom;
    private readonly PrizeCalculationService _prizeService;
    private readonly GameConfiguration _gameConfig;

    public PrizeCalculationServiceTests()
    {
        _mockRandom = new Mock<IRandomNumberGenerator>();
        _gameConfig = CreateDefaultGameConfiguration();
        _prizeService = new PrizeCalculationService(_mockRandom.Object, Options.Create(_gameConfig));
    }

    [Fact]
    public void Constructor_NullRandomGenerator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PrizeCalculationService(null!, Options.Create(_gameConfig)));
    }

    [Theory]
    [InlineData(100, 50)]
    [InlineData(200, 100)]
    [InlineData(75, 30)]
    public void CalculatePrizeDistribution_ValidInputs_ShouldCalculateCorrectly(decimal totalRevenue, int totalTickets)
    {
        // Act
        var distribution = _prizeService.CalculatePrizeDistribution(totalRevenue, totalTickets);

        // Assert
        Assert.Equal(totalRevenue * 0.50m, distribution.GrandPrizeAmount);
        Assert.Equal(totalRevenue * 0.30m, distribution.SecondTierTotalAmount);
        Assert.Equal(totalRevenue * 0.10m, distribution.ThirdTierTotalAmount);
        
        var expectedSecondTierCount = Math.Max(1, (int)Math.Round(totalTickets * 0.10));
        var expectedThirdTierCount = Math.Max(1, (int)Math.Round(totalTickets * 0.20));
        
        Assert.Equal(expectedSecondTierCount, distribution.SecondTierWinnerCount);
        Assert.Equal(expectedThirdTierCount, distribution.ThirdTierWinnerCount);
        
        Assert.Equal(distribution.SecondTierTotalAmount / expectedSecondTierCount, distribution.SecondTierPrizePerWinner);
        Assert.Equal(distribution.ThirdTierTotalAmount / expectedThirdTierCount, distribution.ThirdTierPrizePerWinner);
        
        var expectedHouseProfit = totalRevenue * 0.10m;
        Assert.Equal(expectedHouseProfit, distribution.ExpectedHouseProfit);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-100, 10)]
    [InlineData(100, 0)]
    [InlineData(100, -5)]
    public void CalculatePrizeDistribution_InvalidInputs_ShouldThrowArgumentException(decimal totalRevenue, int totalTickets)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _prizeService.CalculatePrizeDistribution(totalRevenue, totalTickets));
    }

    [Fact]
    public void ConductDraw_ValidInputs_ShouldDistributePrizesCorrectly()
    {
        // Arrange
        var players = CreateTestPlayers(3);
        var tickets = CreateTestTickets(players, new[] { 2, 3, 5 }); // 10 total tickets
        var distribution = _prizeService.CalculatePrizeDistribution(10m, 10);

        // Setup random selections
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0) // Grand prize winner (first ticket)
            .Returns(0) // Second tier winner (first available ticket)
            .Returns(1) // Third tier winner (second available ticket)
            .Returns(0); // Third tier winner (first available ticket)

        // Act
        var results = _prizeService.ConductDraw(tickets, distribution);

        // Assert
        Assert.Single(results.GrandPrizeWinners);
        Assert.Equal(1, results.SecondTierWinners.Count);
        Assert.Equal(2, results.ThirdTierWinners.Count);
        
        Assert.Equal(5.0m, results.GrandPrizeWinners[0].WinAmount);
        Assert.Equal(3.0m, results.SecondTierWinners[0].WinAmount);
        Assert.All(results.ThirdTierWinners, t => Assert.Equal(0.5m, t.WinAmount));
        
        Assert.Equal(10m, results.TotalRevenue);
        Assert.Equal(1.0m, results.HouseProfit); // 10 - 5 - 3 - 1 = 1
    }

    [Fact]
    public void ConductDraw_NullTickets_ShouldThrowArgumentException()
    {
        // Arrange
        var distribution = new PrizeDistribution();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _prizeService.ConductDraw(null!, distribution));
    }

    [Fact]
    public void ConductDraw_EmptyTickets_ShouldThrowArgumentException()
    {
        // Arrange
        var tickets = new List<Ticket>();
        var distribution = new PrizeDistribution();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _prizeService.ConductDraw(tickets, distribution));
    }

    [Fact]
    public void ConductDraw_NullDistribution_ShouldThrowArgumentNullException()
    {
        // Arrange
        var players = CreateTestPlayers(1);
        var tickets = CreateTestTickets(players, new[] { 1 });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _prizeService.ConductDraw(tickets, null!));
    }

    [Fact]
    public void ConductDraw_SingleTicket_ShouldOnlyAwardGrandPrize()
    {
        // Arrange
        var players = CreateTestPlayers(1);
        var tickets = CreateTestTickets(players, new[] { 1 });
        var distribution = _prizeService.CalculatePrizeDistribution(1m, 1);

        _mockRandom.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

        // Act
        var results = _prizeService.ConductDraw(tickets, distribution);

        // Assert
        Assert.Single(results.GrandPrizeWinners);
        Assert.Empty(results.SecondTierWinners);
        Assert.Empty(results.ThirdTierWinners);
        Assert.Equal(0.5m, results.GrandPrizeWinners[0].WinAmount);
    }

    private List<Player> CreateTestPlayers(int count)
    {
        var players = new List<Player>();
        for (int i = 1; i <= count; i++)
        {
            players.Add(new Player(i));
        }
        return players;
    }

    private List<Ticket> CreateTestTickets(List<Player> players, int[] ticketCounts)
    {
        var tickets = new List<Ticket>();
        for (int i = 0; i < players.Count && i < ticketCounts.Length; i++)
        {
            for (int j = 0; j < ticketCounts[i]; j++)
            {
                tickets.Add(new Ticket(players[i]));
            }
        }
        return tickets;
    }

    private GameConfiguration CreateDefaultGameConfiguration()
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
            Tickets = new TicketConfiguration { Price = 1.00m },
            Prizes = new PrizeConfiguration
            {
                GrandPrizePercentage = 0.50m,
                SecondTierPercentage = 0.30m,
                ThirdTierPercentage = 0.10m,
                HouseProfitPercentage = 0.10m,
                SecondTierWinnerPercentage = 0.10m,
                ThirdTierWinnerPercentage = 0.20m
            },
            Display = new DisplayConfiguration()
        };
    }
}
