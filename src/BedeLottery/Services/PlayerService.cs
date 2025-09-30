namespace BedeLottery.Services;

/// <summary>
/// Service for managing player operations
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly IRandomNumberGenerator _randomGenerator;
    private readonly GameConfiguration _gameConfig;

    public PlayerService(IRandomNumberGenerator randomGenerator, IOptions<GameConfiguration> gameConfig)
    {
        _randomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
        _gameConfig = gameConfig?.Value ?? throw new ArgumentNullException(nameof(gameConfig));
    }

    public List<Player> CreatePlayers(int totalPlayers)
    {
        if (totalPlayers < _gameConfig.Players.MinimumCount || totalPlayers > _gameConfig.Players.MaximumCount)
        {
            throw new ArgumentException($"Total players must be between {_gameConfig.Players.MinimumCount} and {_gameConfig.Players.MaximumCount}");
        }

        var players = new List<Player>();

        // Add human player (Player 1)
        players.Add(new Player(1, _gameConfig.Players.StartingBalance, isHuman: true));

        // Add CPU players (Player 2, 3, etc.)
        for (int i = 2; i <= totalPlayers; i++)
        {
            players.Add(new Player(i, _gameConfig.Players.StartingBalance, isHuman: false));
        }

        return players;
    }

    public void ProcessCpuTicketPurchases(List<Player> players)
    {
        var cpuPlayers = players.Where(p => !p.IsHuman).ToList();
        
        foreach (var player in cpuPlayers)
        {
            var maxTickets = player.GetMaxPurchasableTickets();
            if (maxTickets > 0)
            {
                // CPU players randomly choose between 1 and their maximum purchasable tickets
                var ticketCount = _randomGenerator.Next(1, Math.Min(maxTickets + 1, _gameConfig.Players.MaxTicketsPerPlayer + 1));
                player.PurchaseTickets(ticketCount, _gameConfig.Tickets.Price);
            }
        }
    }

    public bool ValidateHumanTicketPurchase(Player player, int ticketCount)
    {
        if (player == null || !player.IsHuman)
            return false;

        if (ticketCount < _gameConfig.Players.MinTicketsPerPlayer || ticketCount > _gameConfig.Players.MaxTicketsPerPlayer)
            return false;

        return player.CanPurchaseTickets(ticketCount, _gameConfig.Tickets.Price);
    }
}
