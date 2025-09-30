namespace BedeLottery.Interfaces;

public interface IConsoleService
{
    void WriteLine(string message);
    void Write(string message);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    void Clear();
}
