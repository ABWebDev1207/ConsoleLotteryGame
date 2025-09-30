namespace BedeLottery.Models;

public class Ticket
{
    private static int _nextId = 1;

    public int Id { get; }
    public Player Owner { get; }
    public DateTime PurchaseTime { get; }
    public bool IsWinner { get; set; }
    public PrizeType? PrizeType { get; set; }
    public decimal WinAmount { get; set; }

    public Ticket(Player owner)
    {
        Id = _nextId++;
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        PurchaseTime = DateTime.Now;
        IsWinner = false;
        PrizeType = null;
        WinAmount = 0;
    }

    public void MarkAsWinner(PrizeType prizeType, decimal winAmount)
    {
        IsWinner = true;
        PrizeType = prizeType;
        WinAmount = winAmount;
    }

    public override string ToString()
    {
        var status = IsWinner ? $"WINNER - {PrizeType} (${WinAmount:F2})" : "Not a winner";
        return $"Ticket #{Id} - Owner: {Owner.Name} - {status}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Ticket ticket && Id == ticket.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public enum PrizeType
{
    Grand,
    SecondTier,
    ThirdTier
}
