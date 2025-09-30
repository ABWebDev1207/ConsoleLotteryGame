namespace BedeLottery.Services;

/// <summary>
/// Main service for orchestrating the lottery game operations
/// </summary>
public class LotteryGameService : ILotteryGameService
{
    private readonly IPlayerService _playerService;
    private readonly IPrizeCalculationService _prizeCalculationService;
    private readonly GameConfiguration _gameConfig;

    public LotteryGameService(
        IPlayerService playerService,
        IPrizeCalculationService prizeCalculationService,
        IOptions<GameConfiguration> gameConfig)
    {
        _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        _prizeCalculationService = prizeCalculationService ?? throw new ArgumentNullException(nameof(prizeCalculationService));
        _gameConfig = gameConfig?.Value ?? throw new ArgumentNullException(nameof(gameConfig));
    }

    public LotteryGame InitializeGame(int totalPlayers)
    {
        var game = new LotteryGame();
        var players = _playerService.CreatePlayers(totalPlayers);

        game.Players = players;
        game.State = GameState.Initialized;

        return game;
    }

    public bool ProcessHumanTicketPurchase(LotteryGame game, int ticketCount)
    {
        if (game.State != GameState.Initialized)
        {
            return false;
        }

        var humanPlayer = game.Players.FirstOrDefault(p => p.IsHuman);
        if (humanPlayer == null)
        {
            return false;
        }

        if (!_playerService.ValidateHumanTicketPurchase(humanPlayer, ticketCount))
        {
            return false;
        }

        var purchasedTickets = humanPlayer.PurchaseTickets(ticketCount, _gameConfig.Tickets.Price);
        return purchasedTickets.Count == ticketCount;
    }

    public void ProcessCpuTicketPurchases(LotteryGame game)
    {
        if (game.State != GameState.Initialized)
        {
            throw new InvalidOperationException("Game must be initialized before processing CPU purchases");
        }

        _playerService.ProcessCpuTicketPurchases(game.Players);

        // Update game state and calculate total revenue
        game.UpdateAfterTicketPurchases(_gameConfig.Tickets.Price);
    }

    public LotteryResults ConductDraw(LotteryGame game)
    {
        if (game.State != GameState.TicketsPurchased)
        {
            throw new InvalidOperationException("Tickets must be purchased before conducting draw");
        }

        var allTickets = game.AllTickets;
        if (!allTickets.Any())
        {
            throw new InvalidOperationException("No tickets available for draw");
        }

        var prizeDistribution = _prizeCalculationService.CalculatePrizeDistribution(game.TotalRevenue, allTickets.Count);
        var results = _prizeCalculationService.ConductDraw(allTickets, prizeDistribution);

        // Update game state
        game.Results = results;
        game.HouseProfit = results.HouseProfit;
        game.State = GameState.DrawCompleted;

        return results;
    }

    public List<PlayerWinSummary> GetPlayerWinSummaries(LotteryGame game)
    {
        return game.GetPlayerWinSummaries();
    }
}
