namespace BedeLottery.Interfaces;

public interface ILotteryGameService
{
    LotteryGame InitializeGame(int totalPlayers);
    bool ProcessHumanTicketPurchase(LotteryGame game, int ticketCount);
    void ProcessCpuTicketPurchases(LotteryGame game);
    LotteryResults ConductDraw(LotteryGame game);
    List<PlayerWinSummary> GetPlayerWinSummaries(LotteryGame game);
}
