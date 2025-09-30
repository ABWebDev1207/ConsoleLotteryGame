namespace BedeLottery.Services;

public class RandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    public IRandomNumberGenerator Create()
    {
        return new RandomNumberGenerator();
    }

    public IRandomNumberGenerator CreateSeeded(int seed)
    {
        return new RandomNumberGenerator(seed);
    }
}
