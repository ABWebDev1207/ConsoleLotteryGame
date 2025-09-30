namespace BedeLottery.Services;

public class GameOrchestrator : IGameOrchestrator
{
    private readonly ILotteryGameService _gameService;
    private readonly IGameUIService _uiService;
    private readonly GameConfiguration _gameConfig;

    public GameOrchestrator(
        ILotteryGameService gameService,
        IGameUIService uiService,
        IOptions<GameConfiguration> gameConfig)
    {
        _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
        _gameConfig = gameConfig?.Value ?? throw new ArgumentNullException(nameof(gameConfig));
    }

    public Task RunGameAsync()
    {
        try
        {
            _uiService.DisplayWelcome();
            _uiService.WaitForContinue();

            var random = new Random();
            var playerCount = random.Next(_gameConfig.Players.MinimumCount, _gameConfig.Players.MaximumCount + 1);
            _uiService.DisplayPlayerCount(playerCount);

            var game = _gameService.InitializeGame(playerCount);

            var humanPlayer = game.Players.First(p => p.IsHuman);
            var humanTicketCount = _uiService.GetHumanTicketCount(humanPlayer);

            var purchaseSuccess = _gameService.ProcessHumanTicketPurchase(game, humanTicketCount);
            if (!purchaseSuccess)
            {
                _uiService.DisplayError("Failed to purchase tickets. Please try again.");
                return Task.CompletedTask;
            }

            _gameService.ProcessCpuTicketPurchases(game);

            _uiService.DisplayGameState(game);
            _uiService.WaitForContinue();

            _uiService.DisplayWelcome();
            var results = _gameService.ConductDraw(game);

            var winSummaries = _gameService.GetPlayerWinSummaries(game);
            _uiService.DisplayResults(game, winSummaries);

            _uiService.WaitForContinue();
        }
        catch (Exception ex)
        {
            _uiService.DisplayError($"An unexpected error occurred: {ex.Message}");
            _uiService.WaitForContinue();
        }

        return Task.CompletedTask;
    }
}
