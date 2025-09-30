namespace BedeLottery.Interfaces;

public interface IRandomNumberGeneratorFactory
{
    IRandomNumberGenerator Create();
    IRandomNumberGenerator CreateSeeded(int seed);
}
