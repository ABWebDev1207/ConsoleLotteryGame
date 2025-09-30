using BedeLottery.Models;
using System.Reflection;
using Xunit;

namespace BedeLottery.Tests.Models;

public class TicketTests
{
    [Fact]
    public void Constructor_ShouldInitializeTicketCorrectly()
    {
        // Arrange
        var player = new Player(1);

        // Act
        var ticket = new Ticket(player);

        // Assert
        Assert.True(ticket.Id > 0);
        Assert.Equal(player, ticket.Owner);
        Assert.False(ticket.IsWinner);
        Assert.Null(ticket.PrizeType);
        Assert.Equal(0, ticket.WinAmount);
        Assert.True(ticket.PurchaseTime <= DateTime.Now);
    }

    [Fact]
    public void Constructor_NullOwner_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Ticket(null!));
    }

    [Fact]
    public void MarkAsWinner_ShouldSetWinnerProperties()
    {
        // Arrange
        var player = new Player(1);
        var ticket = new Ticket(player);
        var prizeType = PrizeType.Grand;
        var winAmount = 100.00m;

        // Act
        ticket.MarkAsWinner(prizeType, winAmount);

        // Assert
        Assert.True(ticket.IsWinner);
        Assert.Equal(prizeType, ticket.PrizeType);
        Assert.Equal(winAmount, ticket.WinAmount);
    }

    [Fact]
    public void UniqueIds_ShouldBeAssignedToEachTicket()
    {
        // Arrange
        var player = new Player(1);

        // Act
        var ticket1 = new Ticket(player);
        var ticket2 = new Ticket(player);
        var ticket3 = new Ticket(player);

        // Assert
        Assert.NotEqual(ticket1.Id, ticket2.Id);
        Assert.NotEqual(ticket2.Id, ticket3.Id);
        Assert.NotEqual(ticket1.Id, ticket3.Id);
    }

    [Fact]
    public void ToString_NotWinner_ShouldReturnCorrectFormat()
    {
        // Arrange
        var player = new Player(1);
        var ticket = new Ticket(player);

        // Act
        var result = ticket.ToString();

        // Assert
        Assert.Contains($"Ticket #{ticket.Id}", result);
        Assert.Contains("Owner: Player 1", result);
        Assert.Contains("Not a winner", result);
    }

    [Fact]
    public void ToString_Winner_ShouldReturnCorrectFormat()
    {
        // Arrange
        var player = new Player(1);
        var ticket = new Ticket(player);
        ticket.MarkAsWinner(PrizeType.Grand, 50.00m);

        // Act
        var result = ticket.ToString();

        // Assert
        Assert.Contains($"Ticket #{ticket.Id}", result);
        Assert.Contains("Owner: Player 1", result);
        Assert.Contains("WINNER - Grand ($50.00)", result);
    }

    [Fact]
    public void Equals_SameId_ShouldReturnTrue()
    {
        // Arrange
        var player = new Player(1);
        var ticket1 = new Ticket(player);

        // Act & Assert - A ticket should equal itself
        Assert.Equal(ticket1, ticket1);
        Assert.Equal(ticket1.GetHashCode(), ticket1.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentId_ShouldReturnFalse()
    {
        // Arrange
        var player = new Player(1);
        var ticket1 = new Ticket(player);
        var ticket2 = new Ticket(player);

        // Act & Assert
        Assert.NotEqual(ticket1, ticket2);
    }

    [Theory]
    [InlineData(PrizeType.Grand)]
    [InlineData(PrizeType.SecondTier)]
    [InlineData(PrizeType.ThirdTier)]
    public void PrizeType_AllValues_ShouldBeSupported(PrizeType prizeType)
    {
        // Arrange
        var player = new Player(1);
        var ticket = new Ticket(player);

        // Act
        ticket.MarkAsWinner(prizeType, 10.00m);

        // Assert
        Assert.Equal(prizeType, ticket.PrizeType);
    }
}
