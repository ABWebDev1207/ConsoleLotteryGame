namespace BedeLottery.Configuration;

public class GameConfiguration
{
    public const string SectionName = "GameConfiguration";

    public PlayerConfiguration Players { get; set; } = new();
    public TicketConfiguration Tickets { get; set; } = new();
    public PrizeConfiguration Prizes { get; set; } = new();
    public DisplayConfiguration Display { get; set; } = new();

    public void Validate()
    {
        Players.Validate();
        Tickets.Validate();
        Prizes.Validate();
    }
}

public class PlayerConfiguration
{
    public int MinimumCount { get; set; } = 10;
    public int MaximumCount { get; set; } = 15;
    public decimal StartingBalance { get; set; } = 10.00m;
    public int MinTicketsPerPlayer { get; set; } = 1;
    public int MaxTicketsPerPlayer { get; set; } = 10;

    public void Validate()
    {
        if (MinimumCount <= 0 || MaximumCount <= 0 || MinimumCount > MaximumCount)
            throw new InvalidOperationException("Invalid player count configuration");

        if (StartingBalance <= 0)
            throw new InvalidOperationException("Starting balance must be positive");

        if (MinTicketsPerPlayer <= 0 || MaxTicketsPerPlayer <= 0 || MinTicketsPerPlayer > MaxTicketsPerPlayer)
            throw new InvalidOperationException("Invalid ticket count configuration");
    }
}

public class TicketConfiguration
{
    public decimal Price { get; set; } = 1.00m;

    public void Validate()
    {
        if (Price <= 0)
            throw new InvalidOperationException("Ticket price must be positive");
    }
}

public class PrizeConfiguration
{
    public decimal GrandPrizePercentage { get; set; } = 0.50m;
    public decimal SecondTierPercentage { get; set; } = 0.30m;
    public decimal ThirdTierPercentage { get; set; } = 0.10m;
    public decimal HouseProfitPercentage { get; set; } = 0.10m;
    public decimal SecondTierWinnerPercentage { get; set; } = 0.10m;
    public decimal ThirdTierWinnerPercentage { get; set; } = 0.20m;

    public void Validate()
    {
        var totalPercentage = GrandPrizePercentage + SecondTierPercentage + ThirdTierPercentage + HouseProfitPercentage;
        if (Math.Abs(totalPercentage - 1.0m) > 0.001m)
            throw new InvalidOperationException("Prize percentages must sum to 100%");

        if (SecondTierWinnerPercentage <= 0 || SecondTierWinnerPercentage > 1)
            throw new InvalidOperationException("Second tier winner percentage must be between 0 and 1");

        if (ThirdTierWinnerPercentage <= 0 || ThirdTierWinnerPercentage > 1)
            throw new InvalidOperationException("Third tier winner percentage must be between 0 and 1");
    }
}

public class DisplayConfiguration
{
    public bool ShowDetailedResults { get; set; } = true;
    public bool ShowPrizeBreakdown { get; set; } = true;
    public bool ClearScreenBetweenPhases { get; set; } = true;
}
