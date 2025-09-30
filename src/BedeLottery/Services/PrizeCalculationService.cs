namespace BedeLottery.Services;

/// <summary>
/// Service for calculating prizes and conducting lottery draws
/// </summary>
public class PrizeCalculationService : IPrizeCalculationService
{
    private readonly IRandomNumberGenerator _randomGenerator;
    private readonly GameConfiguration _gameConfig;

    public PrizeCalculationService(IRandomNumberGenerator randomGenerator, IOptions<GameConfiguration> gameConfig)
    {
        _randomGenerator = randomGenerator ?? throw new ArgumentNullException(nameof(randomGenerator));
        _gameConfig = gameConfig?.Value ?? throw new ArgumentNullException(nameof(gameConfig));
    }

    public PrizeDistribution CalculatePrizeDistribution(decimal totalRevenue, int totalTickets)
    {
        if (totalRevenue <= 0 || totalTickets <= 0)
        {
            throw new ArgumentException("Total revenue and ticket count must be positive");
        }

        var distribution = new PrizeDistribution
        {
            GrandPrizeAmount = totalRevenue * _gameConfig.Prizes.GrandPrizePercentage,
            SecondTierTotalAmount = totalRevenue * _gameConfig.Prizes.SecondTierPercentage,
            ThirdTierTotalAmount = totalRevenue * _gameConfig.Prizes.ThirdTierPercentage
        };

        // Calculate winner counts
        distribution.SecondTierWinnerCount = Math.Max(1, (int)Math.Round(totalTickets * _gameConfig.Prizes.SecondTierWinnerPercentage));
        distribution.ThirdTierWinnerCount = Math.Max(1, (int)Math.Round(totalTickets * _gameConfig.Prizes.ThirdTierWinnerPercentage));

        // Calculate prize per winner
        distribution.SecondTierPrizePerWinner = distribution.SecondTierTotalAmount / distribution.SecondTierWinnerCount;
        distribution.ThirdTierPrizePerWinner = distribution.ThirdTierTotalAmount / distribution.ThirdTierWinnerCount;

        // Calculate expected house profit
        var totalPrizesExpected = distribution.GrandPrizeAmount + 
                                 distribution.SecondTierTotalAmount + 
                                 distribution.ThirdTierTotalAmount;
        distribution.ExpectedHouseProfit = totalRevenue - totalPrizesExpected;

        return distribution;
    }

    public LotteryResults ConductDraw(List<Ticket> tickets, PrizeDistribution prizeDistribution)
    {
        if (tickets == null || !tickets.Any())
        {
            throw new ArgumentException("Tickets list cannot be null or empty");
        }

        if (prizeDistribution == null)
        {
            throw new ArgumentNullException(nameof(prizeDistribution));
        }

        var results = new LotteryResults();
        var availableTickets = new List<Ticket>(tickets);

        // Grand Prize (1 winner gets 50% of revenue)
        if (availableTickets.Count > 0)
        {
            var grandWinner = SelectRandomTicket(availableTickets);
            grandWinner.MarkAsWinner(PrizeType.Grand, prizeDistribution.GrandPrizeAmount);
            grandWinner.Owner.AddWinnings(prizeDistribution.GrandPrizeAmount);
            results.GrandPrizeWinners.Add(grandWinner);
            availableTickets.Remove(grandWinner);
        }

        // Second Tier Winners
        var secondTierCount = Math.Min(prizeDistribution.SecondTierWinnerCount, availableTickets.Count);
        for (int i = 0; i < secondTierCount; i++)
        {
            var winner = SelectRandomTicket(availableTickets);
            winner.MarkAsWinner(PrizeType.SecondTier, prizeDistribution.SecondTierPrizePerWinner);
            winner.Owner.AddWinnings(prizeDistribution.SecondTierPrizePerWinner);
            results.SecondTierWinners.Add(winner);
            availableTickets.Remove(winner);
        }

        // Third Tier Winners
        var thirdTierCount = Math.Min(prizeDistribution.ThirdTierWinnerCount, availableTickets.Count);
        for (int i = 0; i < thirdTierCount; i++)
        {
            var winner = SelectRandomTicket(availableTickets);
            winner.MarkAsWinner(PrizeType.ThirdTier, prizeDistribution.ThirdTierPrizePerWinner);
            winner.Owner.AddWinnings(prizeDistribution.ThirdTierPrizePerWinner);
            results.ThirdTierWinners.Add(winner);
            availableTickets.Remove(winner);
        }

        // Calculate actual house profit
        CalculateHouseProfit(results, tickets.Count, _gameConfig.Tickets.Price);

        return results;
    }

    private Ticket SelectRandomTicket(List<Ticket> availableTickets)
    {
        if (!availableTickets.Any())
        {
            throw new InvalidOperationException("No tickets available for selection");
        }

        var index = _randomGenerator.Next(availableTickets.Count);
        return availableTickets[index];
    }

    private void CalculateHouseProfit(LotteryResults results, int totalTickets, decimal ticketPrice)
    {
        var totalPrizesAwarded = results.GrandPrizeWinners.Sum(t => t.WinAmount) +
                                results.SecondTierWinners.Sum(t => t.WinAmount) +
                                results.ThirdTierWinners.Sum(t => t.WinAmount);

        results.TotalRevenue = totalTickets * ticketPrice;
        results.HouseProfit = results.TotalRevenue - totalPrizesAwarded;
    }
}
