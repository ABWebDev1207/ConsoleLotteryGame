namespace BedeLottery.Interfaces;

public interface IPlayerService
{
    List<Player> CreatePlayers(int totalPlayers);
    void ProcessCpuTicketPurchases(List<Player> players);
    bool ValidateHumanTicketPurchase(Player player, int ticketCount);
}
