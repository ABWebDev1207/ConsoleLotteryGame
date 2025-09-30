namespace BedeLottery.Models;

public class LotteryGame
{
    public List<Player> Players { get; set; }
    public List<Ticket> AllTickets { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal HouseProfit { get; set; }
    public GameState State { get; set; }
    public LotteryResults? Results { get; set; }

    public LotteryGame()
    {
        Players = new List<Player>();
        AllTickets = new List<Ticket>();
        TotalRevenue = 0;
        HouseProfit = 0;
        State = GameState.NotStarted;
    }

    public void UpdateAfterTicketPurchases(decimal ticketPrice)
    {
        AllTickets.Clear();
        AllTickets.AddRange(Players.SelectMany(p => p.Tickets));
        TotalRevenue = AllTickets.Count * ticketPrice;
        State = GameState.TicketsPurchased;
    }

    public List<PlayerWinSummary> GetPlayerWinSummaries()
    {
        if (Results == null)
            return new List<PlayerWinSummary>();

        var allWinningTickets = Results.GrandPrizeWinners
            .Concat(Results.SecondTierWinners)
            .Concat(Results.ThirdTierWinners);

        return allWinningTickets
            .GroupBy(t => t.Owner)
            .Select(g => new PlayerWinSummary
            {
                Player = g.Key,
                WinningTicketCount = g.Count(),
                TotalWinnings = g.Sum(t => t.WinAmount)
            })
            .OrderByDescending(s => s.TotalWinnings)
            .ToList();
    }
}

public enum GameState
{
    NotStarted,
    Initialized,
    TicketsPurchased,
    DrawCompleted
}

public class LotteryResults
{
    public List<Ticket> GrandPrizeWinners { get; set; } = new();
    public List<Ticket> SecondTierWinners { get; set; } = new();
    public List<Ticket> ThirdTierWinners { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal HouseProfit { get; set; }
}

public class PlayerWinSummary
{
    public Player Player { get; set; } = null!;
    public int WinningTicketCount { get; set; }
    public decimal TotalWinnings { get; set; }
}
