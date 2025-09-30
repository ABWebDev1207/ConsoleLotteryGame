namespace BedeLottery.Models;

public class Player
{
    public int Id { get; }
    public string Name { get; }
    public decimal Balance { get; private set; }
    public List<Ticket> Tickets { get; private set; }
    public bool IsHuman { get; }

    public Player(int id, bool isHuman = false) : this(id, 10.00m, isHuman)
    {
    }

    public Player(int id, decimal startingBalance, bool isHuman = false)
    {
        Id = id;
        Name = isHuman ? "Player 1" : $"Player {id}";
        Balance = startingBalance;
        Tickets = new List<Ticket>();
        IsHuman = isHuman;
    }

    public List<Ticket> PurchaseTickets(int ticketCount, decimal ticketPrice = 1.00m)
    {
        if (!CanPurchaseTickets(ticketCount, ticketPrice))
        {
            return new List<Ticket>();
        }

        var purchasedTickets = new List<Ticket>();
        var totalCost = ticketCount * ticketPrice;

        Balance -= totalCost;

        for (int i = 0; i < ticketCount; i++)
        {
            var ticket = new Ticket(this);
            purchasedTickets.Add(ticket);
            Tickets.Add(ticket);
        }

        return purchasedTickets;
    }

    public bool CanPurchaseTickets(int ticketCount, decimal ticketPrice = 1.00m)
    {
        if (ticketCount < 1 || ticketCount > 10)
            return false;

        if (Tickets.Count + ticketCount > 10)
            return false;

        var totalCost = ticketCount * ticketPrice;
        return Balance >= totalCost;
    }

    public int GetMaxPurchasableTickets(decimal ticketPrice = 1.00m)
    {
        var maxByBalance = (int)Math.Floor(Balance / ticketPrice);
        var maxByLimit = 10 - Tickets.Count;
        return Math.Min(maxByBalance, maxByLimit);
    }

    public void AddWinnings(decimal amount)
    {
        if (amount > 0)
        {
            Balance += amount;
        }
    }

    public override string ToString()
    {
        return $"{Name} - Balance: ${Balance:F2}, Tickets: {Tickets.Count}";
    }
}
