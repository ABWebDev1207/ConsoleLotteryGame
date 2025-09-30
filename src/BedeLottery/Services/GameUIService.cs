namespace BedeLottery.Services;

public class GameUIService : IGameUIService
{
    private readonly IConsoleService _console;
    private readonly GameConfiguration _gameConfig;

    public GameUIService(IConsoleService console, IOptions<GameConfiguration> gameConfig)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _gameConfig = gameConfig?.Value ?? throw new ArgumentNullException(nameof(gameConfig));
    }

    public void DisplayWelcome()
    {
        if (_gameConfig.Display.ClearScreenBetweenPhases)
        {
            _console.Clear();
        }

        _console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        _console.WriteLine("║                    BEDE LOTTERY GAME                         ║");
        _console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        _console.WriteLine("");
        _console.WriteLine("Welcome to the Simplified Lottery Game!");
        _console.WriteLine("");
        _console.WriteLine("GAME RULES:");
        _console.WriteLine($"• Players: {_gameConfig.Players.MinimumCount}-{_gameConfig.Players.MaximumCount} total (you + CPU players)");
        _console.WriteLine($"• Starting balance: ${_gameConfig.Players.StartingBalance:F2} per player");
        _console.WriteLine($"• Ticket price: ${_gameConfig.Tickets.Price:F2} each");
        _console.WriteLine($"• Ticket limit: {_gameConfig.Players.MinTicketsPerPlayer}-{_gameConfig.Players.MaxTicketsPerPlayer} tickets per player");
        _console.WriteLine("");
        _console.WriteLine("PRIZE DISTRIBUTION:");
        _console.WriteLine($"• Grand Prize: 1 ticket wins {_gameConfig.Prizes.GrandPrizePercentage:P0} of total revenue");
        _console.WriteLine($"• Second Tier: {_gameConfig.Prizes.SecondTierWinnerPercentage:P0} of tickets share {_gameConfig.Prizes.SecondTierPercentage:P0} of revenue");
        _console.WriteLine($"• Third Tier: {_gameConfig.Prizes.ThirdTierWinnerPercentage:P0} of tickets share {_gameConfig.Prizes.ThirdTierPercentage:P0} of revenue");
        _console.WriteLine($"• House keeps remaining {_gameConfig.Prizes.HouseProfitPercentage:P0}");
        _console.WriteLine("");
    }

    public void DisplayPlayerCount(int playerCount)
    {
        _console.WriteLine("");
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        _console.WriteLine("                        PLAYER SETUP");
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        _console.WriteLine($"🎲 Randomly selected {playerCount} players for this game!");
        _console.WriteLine($"   (Range: {_gameConfig.Players.MinimumCount}-{_gameConfig.Players.MaximumCount} players)");
        _console.WriteLine("");
    }

    public int GetHumanTicketCount(Player player)
    {
        _console.WriteLine("");
        _console.WriteLine($"Player 1 (You) - Balance: ${player.Balance:F2}");
        _console.WriteLine($"You can purchase between {_gameConfig.Players.MinTicketsPerPlayer} and {player.GetMaxPurchasableTickets(_gameConfig.Tickets.Price)} tickets.");
        _console.WriteLine("");

        while (true)
        {
            _console.Write("How many tickets would you like to purchase? ");
            var input = _console.ReadLine();

            if (int.TryParse(input, out int ticketCount))
            {
                if (ticketCount >= _gameConfig.Players.MinTicketsPerPlayer &&
                    ticketCount <= _gameConfig.Players.MaxTicketsPerPlayer &&
                    player.CanPurchaseTickets(ticketCount, _gameConfig.Tickets.Price))
                {
                    return ticketCount;
                }

                if (ticketCount < _gameConfig.Players.MinTicketsPerPlayer || ticketCount > _gameConfig.Players.MaxTicketsPerPlayer)
                {
                    DisplayError($"You must purchase between {_gameConfig.Players.MinTicketsPerPlayer} and {_gameConfig.Players.MaxTicketsPerPlayer} tickets.");
                }
                else if (!player.CanPurchaseTickets(ticketCount, _gameConfig.Tickets.Price))
                {
                    DisplayError($"You can only afford {player.GetMaxPurchasableTickets(_gameConfig.Tickets.Price)} tickets with your current balance.");
                }
            }
            else
            {
                DisplayError("Please enter a valid number.");
            }
        }
    }

    public void DisplayGameState(LotteryGame game)
    {
        _console.WriteLine("");
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        _console.WriteLine("                        GAME STATE");
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        
        foreach (var player in game.Players.OrderBy(p => p.Id))
        {
            _console.WriteLine($"{player.Name}: {player.Tickets.Count} tickets, Balance: ${player.Balance:F2}");
        }

        _console.WriteLine("");
        _console.WriteLine($"Total tickets sold: {game.AllTickets.Count}");
        _console.WriteLine($"Total revenue: ${game.TotalRevenue:F2}");
        _console.WriteLine("");
    }

    public void DisplayResults(LotteryGame game, List<PlayerWinSummary> winSummaries)
    {
        DisplayResultsHeader();
        DisplayGameStats(game);
        DisplayPrizesByTier(game);
        DisplayPlayerSummary(winSummaries);
        DisplayHouseProfit(game.HouseProfit);

        if (_gameConfig.Display.ShowPrizeBreakdown && game.Results != null)
        {
            DisplayPrizeBreakdown(game.Results);
        }
    }

    private void DisplayResultsHeader()
    {
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        _console.WriteLine("                      LOTTERY RESULTS");
        _console.WriteLine("═══════════════════════════════════════════════════════════════");
        _console.WriteLine("");
    }

    private void DisplayGameStats(LotteryGame game)
    {
        _console.WriteLine($"📊 Total tickets sold: {game.AllTickets.Count}");
        _console.WriteLine($"💰 Total revenue: ${game.TotalRevenue:F2}");
        _console.WriteLine("");
    }

    private void DisplayPrizesByTier(LotteryGame game)
    {
        _console.WriteLine("🎯 PRIZE BREAKDOWN BY TIER:");
        _console.WriteLine("");

        var grandPrizeWinners = game.AllTickets.Where(t => t.PrizeType == Models.PrizeType.Grand).ToList();
        var secondTierWinners = game.AllTickets.Where(t => t.PrizeType == Models.PrizeType.SecondTier).ToList();
        var thirdTierWinners = game.AllTickets.Where(t => t.PrizeType == Models.PrizeType.ThirdTier).ToList();

        if (grandPrizeWinners.Any())
        {
            _console.WriteLine($"🥇 GRAND PRIZE (${grandPrizeWinners.First().WinAmount:F2}):");
            foreach (var ticket in grandPrizeWinners)
            {
                _console.WriteLine($"   🎉 {ticket.Owner.Name} - Ticket #{ticket.Id}");
            }
            _console.WriteLine("");
        }

        if (secondTierWinners.Any())
        {
            _console.WriteLine($"🥈 SECOND TIER (${secondTierWinners.First().WinAmount:F2} each):");
            foreach (var ticket in secondTierWinners)
            {
                _console.WriteLine($"   🎊 {ticket.Owner.Name} - Ticket #{ticket.Id}");
            }
            _console.WriteLine("");
        }

        if (thirdTierWinners.Any())
        {
            _console.WriteLine($"🥉 THIRD TIER (${thirdTierWinners.First().WinAmount:F2} each):");
            foreach (var ticket in thirdTierWinners)
            {
                _console.WriteLine($"   🎈 {ticket.Owner.Name} - Ticket #{ticket.Id}");
            }
            _console.WriteLine("");
        }
    }

    private void DisplayPlayerSummary(List<PlayerWinSummary> winSummaries)
    {
        _console.WriteLine("🏆 PLAYER SUMMARY:");
        _console.WriteLine("");

        if (winSummaries.Any())
        {
            foreach (var summary in winSummaries)
            {
                _console.WriteLine($"👤 {summary.Player.Name}:");
                _console.WriteLine($"   💳 Tickets purchased: {summary.Player.Tickets.Count}");
                _console.WriteLine($"   🎫 Winning tickets: {summary.WinningTicketCount}");
                _console.WriteLine($"   💰 Total winnings: ${summary.TotalWinnings:F2}");
                _console.WriteLine($"   📈 Net result: ${summary.TotalWinnings - (summary.Player.Tickets.Count * 1.0m):F2}");
                _console.WriteLine("");
            }
        }
        else
        {
            _console.WriteLine("No winners in this draw.");
            _console.WriteLine("");
        }
    }

    private void DisplayHouseProfit(decimal houseProfit)
    {
        _console.WriteLine($"House profit: ${houseProfit:F2}");
        _console.WriteLine("");
    }

    private void DisplayPrizeBreakdown(LotteryResults results)
    {
        _console.WriteLine("PRIZE BREAKDOWN:");
        _console.WriteLine($"• Grand Prize: ${results.GrandPrizeWinners.Sum(t => t.WinAmount):F2} ({results.GrandPrizeWinners.Count} winner)");
        _console.WriteLine($"• Second Tier: ${results.SecondTierWinners.Sum(t => t.WinAmount):F2} ({results.SecondTierWinners.Count} winners)");
        _console.WriteLine($"• Third Tier: ${results.ThirdTierWinners.Sum(t => t.WinAmount):F2} ({results.ThirdTierWinners.Count} winners)");
        _console.WriteLine($"• Total Revenue: ${results.TotalRevenue:F2}");
    }

    public void DisplayError(string message)
    {
        _console.WriteLine($"ERROR: {message}");
    }

    public void WaitForContinue()
    {
        _console.WriteLine("");
        _console.WriteLine("Press any key to continue...");
        _console.ReadKey(true);
    }
}
