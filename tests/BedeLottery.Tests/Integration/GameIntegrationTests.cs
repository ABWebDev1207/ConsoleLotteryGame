using BedeLottery.Models;
using BedeLottery.Services;
using BedeLottery.Interfaces;
using BedeLottery.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BedeLottery.Tests.Integration;

public class GameIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly Mock<IRandomNumberGenerator> _mockRandom;
    private readonly Mock<IConsoleService> _mockConsole;

    public GameIntegrationTests()
    {
        _mockRandom = new Mock<IRandomNumberGenerator>();
        _mockConsole = new Mock<IConsoleService>();

        var services = new ServiceCollection();

        // Add configuration
        var gameConfig = CreateDefaultGameConfiguration();
        services.Configure<GameConfiguration>(config =>
        {
            config.Players = gameConfig.Players;
            config.Tickets = gameConfig.Tickets;
            config.Prizes = gameConfig.Prizes;
            config.Display = gameConfig.Display;
        });

        services.AddSingleton(_mockRandom.Object);
        services.AddSingleton(_mockConsole.Object);
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IPrizeCalculationService, PrizeCalculationService>();
        services.AddScoped<ILotteryGameService, LotteryGameService>();
        services.AddScoped<IGameUIService, GameUIService>();
        services.AddScoped<IGameOrchestrator, GameOrchestrator>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CompleteGameFlow_ShouldExecuteSuccessfully()
    {
        // Arrange
        var gameOrchestrator = _serviceProvider.GetRequiredService<IGameOrchestrator>();
        
        // Setup console interactions (no longer need player count input)
        _mockConsole.SetupSequence(c => c.ReadLine())
            .Returns("5"); // Human ticket count

        _mockConsole.Setup(c => c.ReadKey(It.IsAny<bool>())).Returns(new ConsoleKeyInfo());

        // Setup random number generation for CPU purchases and draw
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(3) // Player 2 buys 3 tickets
            .Returns(2) // Player 3 buys 2 tickets
            .Returns(4) // Player 4 buys 4 tickets
            .Returns(1) // Player 5 buys 1 ticket
            .Returns(6) // Player 6 buys 6 tickets
            .Returns(2) // Player 7 buys 2 tickets
            .Returns(3) // Player 8 buys 3 tickets
            .Returns(5) // Player 9 buys 5 tickets
            .Returns(1) // Player 10 buys 1 ticket
            .Returns(4) // Player 11 buys 4 tickets
            .Returns(2); // Player 12 buys 2 tickets

        // Setup draw selections
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0) // Grand prize winner
            .Returns(1) // Second tier winner 1
            .Returns(2) // Second tier winner 2
            .Returns(3) // Second tier winner 3
            .Returns(0) // Third tier winner 1
            .Returns(1) // Third tier winner 2
            .Returns(2) // Third tier winner 3
            .Returns(3) // Third tier winner 4
            .Returns(4) // Third tier winner 5
            .Returns(5); // Third tier winner 6

        // Act
        await gameOrchestrator.RunGameAsync();

        // Assert
        // Verify welcome message was displayed
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("BEDE LOTTERY GAME"))), Times.AtLeastOnce);

        // Verify player count is displayed (auto-generated, not prompted)
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("Randomly selected") && s.Contains("players"))), Times.Once);

        // Verify ticket count prompt
        _mockConsole.Verify(c => c.Write(It.Is<string>(s => s.Contains("How many tickets would you like to purchase"))), Times.AtLeastOnce);
        
        // Verify results were displayed
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("LOTTERY RESULTS"))), Times.Once);
        _mockConsole.Verify(c => c.WriteLine(It.Is<string>(s => s.Contains("House profit"))), Times.Once);
    }

    [Fact]
    public void EndToEndGameLogic_ShouldProduceValidResults()
    {
        // Arrange
        var gameService = _serviceProvider.GetRequiredService<ILotteryGameService>();
        var playerService = _serviceProvider.GetRequiredService<IPlayerService>();
        
        // Setup predictable random numbers for CPU players (9 players: 2-10)
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(2) // Player 2: 2 tickets
            .Returns(3) // Player 3: 3 tickets
            .Returns(1) // Player 4: 1 ticket
            .Returns(2) // Player 5: 2 tickets
            .Returns(1) // Player 6: 1 ticket
            .Returns(3) // Player 7: 3 tickets
            .Returns(2) // Player 8: 2 tickets
            .Returns(1) // Player 9: 1 ticket
            .Returns(2); // Player 10: 2 tickets

        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0) // Grand prize: first ticket
            .Returns(0) // Second tier: first available ticket
            .Returns(0); // Third tier: first available ticket

        // Act
        var game = gameService.InitializeGame(10); // 10 players total (minimum allowed)
        
        // Human player buys 3 tickets
        var humanPurchaseSuccess = gameService.ProcessHumanTicketPurchase(game, 3);
        
        // CPU players buy tickets
        gameService.ProcessCpuTicketPurchases(game);
        
        // Conduct draw
        var results = gameService.ConductDraw(game);
        var winSummaries = gameService.GetPlayerWinSummaries(game);

        // Assert
        Assert.True(humanPurchaseSuccess);
        Assert.Equal(10, game.Players.Count);
        
        // Total tickets: 3 (human) + 2 + 3 + 1 + 2 + 1 + 3 + 2 + 1 + 2 = 20 tickets
        Assert.Equal(20, game.AllTickets.Count);
        Assert.Equal(20.0m, game.TotalRevenue);
        
        // Prize distribution should be correct
        Assert.Single(results.GrandPrizeWinners);
        Assert.Equal(10.0m, results.GrandPrizeWinners[0].WinAmount); // 50% of 20

        // Second tier: 10% of 20 tickets = 2 winners, gets 30% of revenue = 6.0 total, 3.0 each
        Assert.Equal(2, results.SecondTierWinners.Count);
        Assert.All(results.SecondTierWinners, t => Assert.Equal(3.0m, t.WinAmount));

        // Third tier: 20% of 20 tickets = 4 winners, share 10% of revenue = 2.0 total, 0.5 each
        Assert.Equal(4, results.ThirdTierWinners.Count);
        Assert.All(results.ThirdTierWinners, t => Assert.Equal(0.5m, t.WinAmount));

        // House profit: 20 - 10 - 6 - 2 = 2.0
        Assert.Equal(2.0m, game.HouseProfit);
        
        // Win summaries should be ordered by total winnings
        Assert.NotEmpty(winSummaries);
        Assert.True(winSummaries[0].TotalWinnings >= winSummaries.Last().TotalWinnings);
    }

    [Fact]
    public void GameWithMinimumPlayers_ShouldWorkCorrectly()
    {
        // Arrange
        var gameService = _serviceProvider.GetRequiredService<ILotteryGameService>();
        
        // Setup random for 9 CPU players (minimum is 10 total)
        var randomSequence = _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()));
        for (int i = 0; i < 9; i++)
        {
            randomSequence.Returns(1); // Each CPU player buys 1 ticket
        }

        // Setup draw random selections
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0) // Grand prize
            .Returns(0); // Second and third tier winners

        // Act
        var game = gameService.InitializeGame(10);
        gameService.ProcessHumanTicketPurchase(game, 1);
        gameService.ProcessCpuTicketPurchases(game);
        var results = gameService.ConductDraw(game);

        // Assert
        Assert.Equal(10, game.Players.Count);
        Assert.Equal(10, game.AllTickets.Count); // 1 human + 9 CPU * 1 each
        Assert.Equal(10.0m, game.TotalRevenue);
        Assert.NotNull(results);
        Assert.Single(results.GrandPrizeWinners);
    }

    [Fact]
    public void GameWithMaximumPlayers_ShouldWorkCorrectly()
    {
        // Arrange
        var gameService = _serviceProvider.GetRequiredService<ILotteryGameService>();
        
        // Setup random for 14 CPU players (maximum is 15 total)
        var randomSequence = _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>(), It.IsAny<int>()));
        for (int i = 0; i < 14; i++)
        {
            randomSequence.Returns(2); // Each CPU player buys 2 tickets
        }

        // Setup draw random selections
        _mockRandom.SetupSequence(r => r.Next(It.IsAny<int>()))
            .Returns(0); // Grand prize and other winners

        // Act
        var game = gameService.InitializeGame(15);
        gameService.ProcessHumanTicketPurchase(game, 5);
        gameService.ProcessCpuTicketPurchases(game);
        var results = gameService.ConductDraw(game);

        // Assert
        Assert.Equal(15, game.Players.Count);
        Assert.Equal(33, game.AllTickets.Count); // 5 human + 14 CPU * 2 each
        Assert.Equal(33.0m, game.TotalRevenue);
        Assert.NotNull(results);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
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
