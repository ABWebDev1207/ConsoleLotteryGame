using BedeLottery.Models;
using BedeLottery.Configuration;

namespace BedeLottery.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating test data to improve test readability and reduce duplication
/// </summary>
public class TestDataBuilder
{
    /// <summary>
    /// Builder for creating players with specific configurations
    /// </summary>
    public class PlayerBuilder
    {
        private int _id = 1;
        private decimal _startingBalance = 10.00m;
        private bool _isHuman = false;
        private int _ticketCount = 0;
        private decimal _ticketPrice = 1.00m;

        public PlayerBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public PlayerBuilder WithStartingBalance(decimal balance)
        {
            _startingBalance = balance;
            return this;
        }

        public PlayerBuilder AsHuman()
        {
            _isHuman = true;
            return this;
        }

        public PlayerBuilder AsCpu()
        {
            _isHuman = false;
            return this;
        }

        public PlayerBuilder WithTickets(int count, decimal price = 1.00m)
        {
            _ticketCount = count;
            _ticketPrice = price;
            return this;
        }

        public Player Build()
        {
            var player = new Player(_id, _startingBalance, _isHuman);
            if (_ticketCount > 0)
            {
                player.PurchaseTickets(_ticketCount, _ticketPrice);
            }
            return player;
        }
    }

    /// <summary>
    /// Builder for creating lottery games with specific configurations
    /// </summary>
    public class LotteryGameBuilder
    {
        private readonly List<Player> _players = new();
        private GameState _state = GameState.NotStarted;
        private decimal _ticketPrice = 1.00m;

        public LotteryGameBuilder WithPlayer(Player player)
        {
            _players.Add(player);
            return this;
        }

        public LotteryGameBuilder WithPlayers(IEnumerable<Player> players)
        {
            _players.AddRange(players);
            return this;
        }

        public LotteryGameBuilder WithHumanPlayer(int ticketCount = 0)
        {
            var player = new PlayerBuilder()
                .WithId(_players.Count + 1)
                .AsHuman()
                .WithTickets(ticketCount, _ticketPrice)
                .Build();
            _players.Add(player);
            return this;
        }

        public LotteryGameBuilder WithCpuPlayers(int count, int ticketsEach = 2)
        {
            for (int i = 0; i < count; i++)
            {
                var player = new PlayerBuilder()
                    .WithId(_players.Count + 1)
                    .AsCpu()
                    .WithTickets(ticketsEach, _ticketPrice)
                    .Build();
                _players.Add(player);
            }
            return this;
        }

        public LotteryGameBuilder WithState(GameState state)
        {
            _state = state;
            return this;
        }

        public LotteryGameBuilder WithTicketPrice(decimal price)
        {
            _ticketPrice = price;
            return this;
        }

        public LotteryGame Build()
        {
            var game = new LotteryGame
            {
                Players = _players,
                State = _state
            };

            if (_state >= GameState.TicketsPurchased)
            {
                game.UpdateAfterTicketPurchases(_ticketPrice);
            }

            return game;
        }
    }

    /// <summary>
    /// Builder for creating tickets with specific configurations
    /// </summary>
    public class TicketBuilder
    {
        private Player? _owner;
        private bool _isWinner = false;
        private PrizeType? _prizeType;
        private decimal _winAmount = 0m;

        public TicketBuilder WithOwner(Player owner)
        {
            _owner = owner;
            return this;
        }

        public TicketBuilder AsWinner(PrizeType prizeType, decimal winAmount)
        {
            _isWinner = true;
            _prizeType = prizeType;
            _winAmount = winAmount;
            return this;
        }

        public Ticket Build()
        {
            if (_owner == null)
                throw new InvalidOperationException("Ticket must have an owner");

            var ticket = new Ticket(_owner);
            if (_isWinner)
            {
                ticket.MarkAsWinner(_prizeType!.Value, _winAmount);
            }
            return ticket;
        }
    }

    /// <summary>
    /// Creates a collection of players for testing
    /// </summary>
    public static List<Player> CreatePlayers(int count, decimal startingBalance = 10.00m, int ticketsEach = 0)
    {
        var players = new List<Player>();
        for (int i = 1; i <= count; i++)
        {
            var builder = new PlayerBuilder()
                .WithId(i)
                .WithStartingBalance(startingBalance);

            if (i == 1)
                builder.AsHuman();
            else
                builder.AsCpu();

            if (ticketsEach > 0)
                builder.WithTickets(ticketsEach);

            players.Add(builder.Build());
        }
        return players;
    }

    /// <summary>
    /// Creates a collection of tickets for testing
    /// </summary>
    public static List<Ticket> CreateTickets(List<Player> players)
    {
        return players.SelectMany(p => p.Tickets).ToList();
    }

    /// <summary>
    /// Creates a simple game scenario for testing
    /// </summary>
    public static LotteryGame CreateSimpleGame(int playerCount = 10, int ticketsPerPlayer = 2)
    {
        return new LotteryGameBuilder()
            .WithHumanPlayer(ticketsPerPlayer)
            .WithCpuPlayers(playerCount - 1, ticketsPerPlayer)
            .WithState(GameState.TicketsPurchased)
            .Build();
    }

    // Factory methods for common scenarios
    public static PlayerBuilder Player() => new();
    public static LotteryGameBuilder Game() => new();
    public static TicketBuilder Ticket() => new();
}
