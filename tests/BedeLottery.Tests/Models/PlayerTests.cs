using BedeLottery.Models;
using Xunit;

namespace BedeLottery.Tests.Models;

public class PlayerTests
{
    [Fact]
    public void Constructor_ShouldInitializePlayerCorrectly()
    {
        // Arrange & Act
        var humanPlayer = new Player(1, isHuman: true);
        var cpuPlayer = new Player(2, isHuman: false);

        // Assert
        Assert.Equal(1, humanPlayer.Id);
        Assert.Equal("Player 1", humanPlayer.Name);
        Assert.Equal(10.00m, humanPlayer.Balance);
        Assert.True(humanPlayer.IsHuman);
        Assert.Empty(humanPlayer.Tickets);

        Assert.Equal(2, cpuPlayer.Id);
        Assert.Equal("Player 2", cpuPlayer.Name);
        Assert.Equal(10.00m, cpuPlayer.Balance);
        Assert.False(cpuPlayer.IsHuman);
        Assert.Empty(cpuPlayer.Tickets);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    public void PurchaseTickets_ValidCount_ShouldSucceed(int ticketCount, bool expectedSuccess)
    {
        // Arrange
        var player = new Player(1);

        // Act
        var tickets = player.PurchaseTickets(ticketCount);

        // Assert
        if (expectedSuccess)
        {
            Assert.Equal(ticketCount, tickets.Count);
            Assert.Equal(ticketCount, player.Tickets.Count);
            Assert.Equal(10.00m - ticketCount, player.Balance);
            Assert.All(tickets, ticket => Assert.Equal(player, ticket.Owner));
        }
        else
        {
            Assert.Empty(tickets);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(15)]
    public void PurchaseTickets_InvalidCount_ShouldFail(int ticketCount)
    {
        // Arrange
        var player = new Player(1);

        // Act
        var tickets = player.PurchaseTickets(ticketCount);

        // Assert
        Assert.Empty(tickets);
        Assert.Empty(player.Tickets);
        Assert.Equal(10.00m, player.Balance);
    }

    [Fact]
    public void PurchaseTickets_InsufficientBalance_ShouldFail()
    {
        // Arrange
        var player = new Player(1);
        player.PurchaseTickets(10); // Use all balance

        // Act
        var tickets = player.PurchaseTickets(1);

        // Assert
        Assert.Empty(tickets);
        Assert.Equal(10, player.Tickets.Count);
        Assert.Equal(0.00m, player.Balance);
    }

    [Fact]
    public void CanPurchaseTickets_ValidScenarios_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player(1);

        // Act & Assert
        Assert.True(player.CanPurchaseTickets(1));
        Assert.True(player.CanPurchaseTickets(5));
        Assert.True(player.CanPurchaseTickets(10));
    }

    [Fact]
    public void CanPurchaseTickets_InvalidScenarios_ShouldReturnFalse()
    {
        // Arrange
        var player = new Player(1);

        // Act & Assert
        Assert.False(player.CanPurchaseTickets(0));
        Assert.False(player.CanPurchaseTickets(11));
        Assert.False(player.CanPurchaseTickets(15));
    }

    [Fact]
    public void GetMaxPurchasableTickets_ShouldReturnCorrectValue()
    {
        // Arrange
        var player = new Player(1);

        // Act & Assert
        Assert.Equal(10, player.GetMaxPurchasableTickets());

        // Purchase some tickets
        player.PurchaseTickets(3);
        Assert.Equal(7, player.GetMaxPurchasableTickets());

        // Purchase more tickets
        player.PurchaseTickets(7);
        Assert.Equal(0, player.GetMaxPurchasableTickets());
    }

    [Fact]
    public void AddWinnings_ShouldIncreaseBalance()
    {
        // Arrange
        var player = new Player(1);
        var initialBalance = player.Balance;

        // Act
        player.AddWinnings(50.00m);

        // Assert
        Assert.Equal(initialBalance + 50.00m, player.Balance);
    }

    [Fact]
    public void AddWinnings_NegativeAmount_ShouldNotChangeBalance()
    {
        // Arrange
        var player = new Player(1);
        var initialBalance = player.Balance;

        // Act
        player.AddWinnings(-10.00m);

        // Assert
        Assert.Equal(initialBalance, player.Balance);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var player = new Player(1);
        player.PurchaseTickets(3);

        // Act
        var result = player.ToString();

        // Assert
        Assert.Equal("Player 1 - Balance: $7.00, Tickets: 3", result);
    }
}
