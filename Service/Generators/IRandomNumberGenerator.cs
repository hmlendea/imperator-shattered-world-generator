using System;

namespace ImperatorShatteredWorldGenerator.Service.Generators
{
    public interface IRandomNumberGenerator
    {
        Random Randomiser { get; }

        int Get(int min, int max);
    }
}
