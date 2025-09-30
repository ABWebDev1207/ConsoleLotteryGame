using BedeLottery.Configuration;
using BedeLottery.Interfaces;
using BedeLottery.Models;
using BedeLottery.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BedeLottery.Tests.BehaviorDriven;

/// <summary>
/// Behavior-driven tests for complete game flow scenarios
/// </summary>
public class GameFlowBehaviorTests
{
    private readonly Mock<IRandomNumberGenerator> _mockRandom;
    private readonly Mock<IConsoleService> _mockConsole;
    private readonly GameConfiguration _gameConfig;
    private readonly IOptions<GameConfiguration> _gameConfigOptions;

    public GameFlowBehaviorTests()
    {
        _mockRandom = new Mock<IRandomNumberGenerator>();
        _mockConsole = new Mock<IConsoleService>();
        _gameConfig = CreateDefaultGameConfiguration();
        _gameConfigOptions = Options.Create(_gameConfig);
    }

    [Fact]
    public void Given_MinimumPlayers_When_GameIsPlayed_Then_AllPlayersCanParticipateAndWinnersAreSelected()
    {
        // Given - Minimum number of players (10)
        var playerService = new PlayerService(_mockRandom.Object, _gameConfigOptions);
        var prizeService = new PrizeCalculationService(_mockRandom.Object, _gameConfigOptions);
        var gameService = new LotteryGameService(playerService, prizeService, _gameConfigOptions);

        SetupCpuPlayerPurchases(9); // 9 CPU players
        SetupDrawSelections();

        // When - Game is played with minimum players
        var game = gameService.InitializeGame(10);
        var humanPurchaseSuccess = gameService.ProcessHumanTicketPurchase(game, 5);
        gameService.ProcessCpuTicketPurchases(game);
        var results = gameService.ConductDraw(game);
        var winSummaries = gameService.GetPlayerWinSummaries(game);

        // Then - Game completes successfully with proper prize distribution
        Assert.True(humanPurchaseSuccess);
        Assert.Equal(10, game.Players.Count);
        Assert.True(game.AllTickets.Count >= 10); // At least 1 ticket per player
        Assert.Single(results.GrandPrizeWinners);
        Assert.NotEmpty(results.SecondTierWinners);
        Assert.NotEmpty(results.ThirdTierWinners);
        Assert.True(game.HouseProfit > 0);
    }

    [Fact]
    public void Given_MaximumPlayers_When_GameIsPlayed_Then_AllPlayersCanParticipateAndRevenueIsMaximized()
    {
        // Given - Maximum number of players (15)
        var playerService = new PlayerService(_mockRandom.Object, _gameConfigOptions);
        var prizeService = new PrizeCalculationService(_mockRandom.Object, _gameConfigOptions);
        var gameService = new LotteryGameService(playerService, prizeService, _gameConfigOptions);

        SetupCpuPlayerPurchases(14); // 14 CPU players
        SetupDrawSelections();

        // When - Game is played with maximum players
        var game = gameService.InitializeGame(15);
        var humanPurchaseSuccess = gameService.ProcessHumanTicketPurchase(game, 10); // Max tickets
        gameService.ProcessCpuTicketPurchases(game);
        var results = gameService.ConductDraw(game);

        // Then - Game handles maximum capacity correctly
        Assert.True(humanPurchaseSuccess);
        Assert.Equal(15, game.Players.Count);
        Assert.True(game.AllTickets.Count >= 15);
        Assert.True(game.TotalRevenue >= 15m); // At least $15 revenue
        Assert.Single(results.GrandPrizeWinners);
        Assert.True(game.HouseProfit > 0);
    }

    [Fact]
    public void Given_PlayerWithInsufficientFunds_When_AttemptingToPurchaseTickets_Then_PurchaseFails()
    {
        // Given - Player with insufficient funds
        var player = new Player(1, 2.50m, isHuman: true); // Only $2.50
        var playerService = new PlayerService(_mockRandom.Object, _gameConfigOptions);

        // When - Attempting to purchase more tickets than affordable
        var canPurchase = playerService.ValidateHumanTicketPurchase(player, 5); // Needs $5

        // Then - Purchase validation fails
        Assert.False(canPurchase);
        Assert.Equal(2, player.GetMaxPurchasableTickets(_gameConfig.Tickets.Price));
    }

    [Fact]
    public void Given_SingleTicketSold_When_DrawIsConducted_Then_OnlyGrandPrizeIsAwarded()
    {
        // Given - Only one ticket in the entire game
        var player = new Player(1, 10m);
        player.PurchaseTickets(1, _gameConfig.Tickets.Price);
        var tickets = player.Tickets;

        var prizeService = new PrizeCalculationService(_mockRandom.Object, _gameConfigOptions);
        _mockRandom.Setup(r => r.Next(It.IsAny<int>())).Returns(0);

        // When - Draw is conducted with single ticket
        var distribution = prizeService.CalculatePrizeDistribution(1m, 1);
        var results = prizeService.ConductDraw(tickets, distribution);

        // Then - Only grand prize is awarded
        Assert.Single(results.GrandPrizeWinners);
        Assert.Empty(results.SecondTierWinners);
        Assert.Empty(results.ThirdTierWinners);
        Assert.Equal(0.5m, results.GrandPrizeWinners[0].WinAmount);
    }

    [Fact]
    public void Given_ConfiguredPrizePercentages_When_DrawIsConducted_Then_PrizesMatchConfiguration()
    {
        // Given - Custom prize configuration
        var customConfig = CreateCustomGameConfiguration();
        var customOptions = Options.Create(customConfig);
        
        var players = CreatePlayersWithTickets(5, 2); // 5 players, 2 tickets each = 10 tickets, $10 revenue
        var prizeService = new PrizeCalculationService(_mockRandom.Object, customOptions);
        
        SetupDrawSelections();

        // When - Draw is conducted with custom configuration
        var distribution = prizeService.CalculatePrizeDistribution(10m, 10);
        var results = prizeService.ConductDraw(players.SelectMany(p => p.Tickets).ToList(), distribution);

        // Then - Prizes match custom configuration (60% grand, 25% second, 10% third, 5% house)
        Assert.Equal(6.0m, distribution.GrandPrizeAmount); // 60% of $10
        Assert.Equal(2.5m, distribution.SecondTierTotalAmount); // 25% of $10
        Assert.Equal(1.0m, distribution.ThirdTierTotalAmount); // 10% of $10
        Assert.Equal(0.5m, distribution.ExpectedHouseProfit); // 5% of $10
    }

    [Theory]
    [InlineData(10, 1)] // Minimum players, minimum tickets each
    [InlineData(15, 10)] // Maximum players, maximum tickets each
    [InlineData(12, 5)] // Average scenario
    public void Given_VariousPlayerAndTicketCombinations_When_GameIsPlayed_Then_GameCompletesSuccessfully(int playerCount, int ticketsPerPlayer)
    {
        // Given - Various player and ticket combinations
        var playerService = new PlayerService(_mockRandom.Object, _gameConfigOptions);
        var prizeService = new PrizeCalculationService(_mockRandom.Object, _gameConfigOptions);
        var gameService = new LotteryGameService(playerService, prizeService, _gameConfigOptions);

        SetupCpuPlayerPurchases(playerCount - 1, ticketsPerPlayer);
        SetupDrawSelections();

        // When - Game is played with these combinations
        var game = gameService.InitializeGame(playerCount);
        var humanPurchaseSuccess = gameService.ProcessHumanTicketPurchase(game, ticketsPerPlayer);
        gameService.ProcessCpuTicketPurchases(game);
        var results = gameService.ConductDraw(game);

        // Then - Game completes successfully regardless of combination
        Assert.True(humanPurchaseSuccess);
        Assert.Equal(playerCount, game.Players.Count);
        Assert.Equal(playerCount * ticketsPerPlayer, game.AllTickets.Count);
        Assert.Equal(playerCount * ticketsPerPlayer * _gameConfig.Tickets.Price, game.TotalRevenue);
        Assert.Single(results.GrandPrizeWinners);
        Assert.True(results.TotalRevenue > 0);
        Assert.True(game.HouseProfit >= 0);
    }

    private void SetupCpuPlayerPurchases(int cpuPlayerCount, int ticketsPerPlayer = 2)
    {
        var sequence = _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()));
        for (int i = 0; i < cpuPlayerCount; i++)
        {
            sequence.Returns(ticketsPerPlayer);
        }
    }

    private void SetupDrawSelections()
    {
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0) // Grand prize winner
            .Returns(0) // Second tier winners
            .Returns(1)
            .Returns(0) // Third tier winners
            .Returns(1)
            .Returns(2);
    }

    private List<Player> CreatePlayersWithTickets(int playerCount, int ticketsPerPlayer)
    {
        var players = new List<Player>();
        for (int i = 1; i <= playerCount; i++)
        {
            var player = new Player(i, _gameConfig.Players.StartingBalance);
            player.PurchaseTickets(ticketsPerPlayer, _gameConfig.Tickets.Price);
            players.Add(player);
        }
        return players;
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

    private GameConfiguration CreateCustomGameConfiguration()
    {
        return new GameConfiguration
        {
            Players = new PlayerConfiguration
            {
                MinimumCount = 5,
                MaximumCount = 20,
                StartingBalance = 15.00m,
                MinTicketsPerPlayer = 1,
                MaxTicketsPerPlayer = 15
            },
            Tickets = new TicketConfiguration { Price = 1.00m },
            Prizes = new PrizeConfiguration
            {
                GrandPrizePercentage = 0.60m,
                SecondTierPercentage = 0.25m,
                ThirdTierPercentage = 0.10m,
                HouseProfitPercentage = 0.05m,
                SecondTierWinnerPercentage = 0.15m,
                ThirdTierWinnerPercentage = 0.25m
            },
            Display = new DisplayConfiguration()
        };
    }
}
