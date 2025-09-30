using BedeLottery.Models;
using BedeLottery.Services;
using BedeLottery.Interfaces;
using BedeLottery.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BedeLottery.Tests.Services;

public class PlayerServiceTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom;
    private readonly PlayerService _playerService;
    private readonly GameConfiguration _gameConfig;

    public PlayerServiceTests()
    {
        _mockRandom = new Mock<IRandomNumberGenerator>();
        _gameConfig = CreateDefaultGameConfiguration();
        _playerService = new PlayerService(_mockRandom.Object, Options.Create(_gameConfig));
    }

    [Fact]
    public void Constructor_NullRandomGenerator_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PlayerService(null!, Options.Create(_gameConfig)));
    }

    [Fact]
    public void Constructor_NullGameConfig_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PlayerService(_mockRandom.Object, null!));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(15)]
    public void CreatePlayers_ValidCount_ShouldCreateCorrectPlayers(int totalPlayers)
    {
        // Act
        var players = _playerService.CreatePlayers(totalPlayers);

        // Assert
        Assert.Equal(totalPlayers, players.Count);
        
        // First player should be human
        Assert.True(players[0].IsHuman);
        Assert.Equal(1, players[0].Id);
        Assert.Equal("Player 1", players[0].Name);

        // Remaining players should be CPU
        for (int i = 1; i < totalPlayers; i++)
        {
            Assert.False(players[i].IsHuman);
            Assert.Equal(i + 1, players[i].Id);
            Assert.Equal($"Player {i + 1}", players[i].Name);
        }

        // All players should have starting balance
        Assert.All(players, p => Assert.Equal(10.00m, p.Balance));
    }

    [Theory]
    [InlineData(9)]
    [InlineData(16)]
    [InlineData(0)]
    public void CreatePlayers_InvalidCount_ShouldThrowArgumentException(int totalPlayers)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _playerService.CreatePlayers(totalPlayers));
    }

    [Fact]
    public void ProcessCpuTicketPurchases_ShouldPurchaseRandomTicketsForCpuPlayers()
    {
        // Arrange
        var players = new List<Player>
        {
            new Player(1, isHuman: true),
            new Player(2, isHuman: false),
            new Player(3, isHuman: false)
        };

        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(3) // Player 2 buys 3 tickets
            .Returns(7); // Player 3 buys 7 tickets

        // Act
        _playerService.ProcessCpuTicketPurchases(players);

        // Assert
        Assert.Empty(players[0].Tickets); // Human player should not be affected
        Assert.Equal(3, players[1].Tickets.Count);
        Assert.Equal(7, players[2].Tickets.Count);
        
        Assert.Equal(10.00m, players[0].Balance); // Human player balance unchanged
        Assert.Equal(7.00m, players[1].Balance); // Player 2: 10 - 3 = 7
        Assert.Equal(3.00m, players[2].Balance); // Player 3: 10 - 7 = 3
    }

    [Fact]
    public void ProcessCpuTicketPurchases_OnlyHumanPlayer_ShouldNotPurchaseAnyTickets()
    {
        // Arrange
        var players = new List<Player>
        {
            new Player(1, isHuman: true)
        };

        // Act
        _playerService.ProcessCpuTicketPurchases(players);

        // Assert
        Assert.Empty(players[0].Tickets);
        Assert.Equal(10.00m, players[0].Balance);
        _mockRandom.Verify(r => r.Next(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void ValidateHumanTicketPurchase_ValidScenarios_ShouldReturnTrue(int ticketCount, bool expected)
    {
        // Arrange
        var humanPlayer = new Player(1, isHuman: true);

        // Act
        var result = _playerService.ValidateHumanTicketPurchase(humanPlayer, ticketCount);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(11, false)]
    [InlineData(15, false)]
    public void ValidateHumanTicketPurchase_InvalidTicketCount_ShouldReturnFalse(int ticketCount, bool expected)
    {
        // Arrange
        var humanPlayer = new Player(1, isHuman: true);

        // Act
        var result = _playerService.ValidateHumanTicketPurchase(humanPlayer, ticketCount);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidateHumanTicketPurchase_CpuPlayer_ShouldReturnFalse()
    {
        // Arrange
        var cpuPlayer = new Player(2, isHuman: false);

        // Act
        var result = _playerService.ValidateHumanTicketPurchase(cpuPlayer, 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateHumanTicketPurchase_NullPlayer_ShouldReturnFalse()
    {
        // Act
        var result = _playerService.ValidateHumanTicketPurchase(null!, 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateHumanTicketPurchase_InsufficientBalance_ShouldReturnFalse()
    {
        // Arrange
        var humanPlayer = new Player(1, isHuman: true);
        humanPlayer.PurchaseTickets(10); // Use all balance

        // Act
        var result = _playerService.ValidateHumanTicketPurchase(humanPlayer, 1);

        // Assert
        Assert.False(result);
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
