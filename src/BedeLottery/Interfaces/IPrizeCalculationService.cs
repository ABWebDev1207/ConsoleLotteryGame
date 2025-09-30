namespace BedeLottery.Interfaces;

public interface IPrizeCalculationService
{
    PrizeDistribution CalculatePrizeDistribution(decimal totalRevenue, int totalTickets);
    LotteryResults ConductDraw(List<Ticket> tickets, PrizeDistribution prizeDistribution);
}

public class PrizeDistribution
{
    public decimal GrandPrizeAmount { get; set; }
    public decimal SecondTierTotalAmount { get; set; }
    public decimal ThirdTierTotalAmount { get; set; }
    public int SecondTierWinnerCount { get; set; }
    public int ThirdTierWinnerCount { get; set; }
    public decimal SecondTierPrizePerWinner { get; set; }
    public decimal ThirdTierPrizePerWinner { get; set; }
    public decimal ExpectedHouseProfit { get; set; }
}
