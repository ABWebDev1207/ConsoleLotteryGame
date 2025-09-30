namespace BedeLottery.Interfaces;

public interface IGameUIService
{
    void DisplayWelcome();
    void DisplayPlayerCount(int playerCount);
    int GetHumanTicketCount(Player player);
    void DisplayGameState(LotteryGame game);
    void DisplayResults(LotteryGame game, List<PlayerWinSummary> winSummaries);
    void DisplayError(string message);
    void WaitForContinue();
}
