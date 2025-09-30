namespace BedeLottery.Services;

public class ConsoleService : IConsoleService
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void Write(string message)
    {
        Console.Write(message);
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        return Console.ReadKey(intercept);
    }

    public void Clear()
    {
        Console.Clear();
    }
}
