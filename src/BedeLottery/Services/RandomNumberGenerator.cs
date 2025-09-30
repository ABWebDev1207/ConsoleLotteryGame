namespace BedeLottery.Services;

public class RandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Random _random;

    public RandomNumberGenerator()
    {
        _random = new Random();
    }

    public RandomNumberGenerator(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }
}
