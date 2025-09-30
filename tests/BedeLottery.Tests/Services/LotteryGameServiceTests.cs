using BedeLottery.Models;
using BedeLottery.Services;
using BedeLottery.Interfaces;
using BedeLottery.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BedeLottery.Tests.Services;

public class LotteryGameServiceTests
{
    private readonly Mock<IPlayerService> _mockPlayerService;
    private readonly Mock<IPrizeCalculationService> _mockPrizeService;
    private readonly LotteryGameService _gameService;
    private readonly GameConfiguration _gameConfig;

    public LotteryGameServiceTests()
    {
        _mockPlayerService = new Mock<IPlayerService>();
        _mockPrizeService = new Mock<IPrizeCalculationService>();
        _gameConfig = CreateDefaultGameConfiguration();
        _gameService = new LotteryGameService(_mockPlayerService.Object, _mockPrizeService.Object, Options.Create(_gameConfig));
    }

    [Fact]
    public void Constructor_NullPlayerService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LotteryGameService(null!, _mockPrizeService.Object, Options.Create(_gameConfig)));
    }

    [Fact]
    public void Constructor_NullPrizeService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LotteryGameService(_mockPlayerService.Object, null!, Options.Create(_gameConfig)));
    }

    [Fact]
    public void InitializeGame_ValidPlayerCount_ShouldCreateGameWithPlayers()
    {
        // Arrange
        var expectedPlayers = new List<Player>
        {
            new Player(1, isHuman: true),
            new Player(2, isHuman: false),
            new Player(3, isHuman: false)
        };
        _mockPlayerService.Setup(s => s.CreatePlayers(12)).Returns(expectedPlayers);

        // Act
        var game = _gameService.InitializeGame(12);

        // Assert
        Assert.NotNull(game);
        Assert.Equal(expectedPlayers, game.Players);
        Assert.Equal(GameState.Initialized, game.State);
        _mockPlayerService.Verify(s => s.CreatePlayers(12), Times.Once);
    }

    [Fact]
    public void ProcessHumanTicketPurchase_ValidPurchase_ShouldReturnTrue()
    {
        // Arrange
        var humanPlayer = new Player(1, isHuman: true);
        var game = new LotteryGame
        {
            Players = new List<Player> { humanPlayer },
            State = GameState.Initialized
        };

        _mockPlayerService.Setup(s => s.ValidateHumanTicketPurchase(humanPlayer, 5)).Returns(true);

        // Act
        var result = _gameService.ProcessHumanTicketPurchase(game, 5);

        // Assert
        Assert.True(result);
        Assert.Equal(5, humanPlayer.Tickets.Count);
        _mockPlayerService.Verify(s => s.ValidateHumanTicketPurchase(humanPlayer, 5), Times.Once);
    }

    [Fact]
    public void ProcessHumanTicketPurchase_InvalidState_ShouldReturnFalse()
    {
        // Arrange
        var game = new LotteryGame { State = GameState.NotStarted };

        // Act
        var result = _gameService.ProcessHumanTicketPurchase(game, 5);

        // Assert
        Assert.False(result);
        _mockPlayerService.Verify(s => s.ValidateHumanTicketPurchase(It.IsAny<Player>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void ProcessHumanTicketPurchase_NoHumanPlayer_ShouldReturnFalse()
    {
        // Arrange
        var game = new LotteryGame
        {
            Players = new List<Player> { new Player(2, isHuman: false) },
            State = GameState.Initialized
        };

        // Act
        var result = _gameService.ProcessHumanTicketPurchase(game, 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessCpuTicketPurchases_ValidState_ShouldProcessPurchases()
    {
        // Arrange
        var players = new List<Player>
        {
            new Player(1, isHuman: true),
            new Player(2, isHuman: false)
        };
        var game = new LotteryGame
        {
            Players = players,
            State = GameState.Initialized
        };

        // Act
        _gameService.ProcessCpuTicketPurchases(game);

        // Assert
        Assert.Equal(GameState.TicketsPurchased, game.State);
        _mockPlayerService.Verify(s => s.ProcessCpuTicketPurchases(players), Times.Once);
    }

    [Fact]
    public void ProcessCpuTicketPurchases_InvalidState_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var game = new LotteryGame { State = GameState.NotStarted };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _gameService.ProcessCpuTicketPurchases(game));
    }

    [Fact]
    public void ConductDraw_ValidState_ShouldReturnResults()
    {
        // Arrange
        var players = new List<Player> { new Player(1) };
        players[0].PurchaseTickets(5);
        
        var game = new LotteryGame
        {
            Players = players,
            State = GameState.TicketsPurchased,
            TotalRevenue = 5m
        };
        game.AllTickets = players[0].Tickets;

        var expectedDistribution = new PrizeDistribution();
        var expectedResults = new LotteryResults { HouseProfit = 1m };

        _mockPrizeService.Setup(s => s.CalculatePrizeDistribution(5m, 5)).Returns(expectedDistribution);
        _mockPrizeService.Setup(s => s.ConductDraw(game.AllTickets, expectedDistribution)).Returns(expectedResults);

        // Act
        var results = _gameService.ConductDraw(game);

        // Assert
        Assert.Equal(expectedResults, results);
        Assert.Equal(GameState.DrawCompleted, game.State);
        Assert.Equal(1m, game.HouseProfit);
        _mockPrizeService.Verify(s => s.CalculatePrizeDistribution(5m, 5), Times.Once);
        _mockPrizeService.Verify(s => s.ConductDraw(game.AllTickets, expectedDistribution), Times.Once);
    }

    [Fact]
    public void ConductDraw_InvalidState_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var game = new LotteryGame { State = GameState.Initialized };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _gameService.ConductDraw(game));
    }

    [Fact]
    public void ConductDraw_NoTickets_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var game = new LotteryGame
        {
            State = GameState.TicketsPurchased,
            AllTickets = new List<Ticket>()
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _gameService.ConductDraw(game));
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
