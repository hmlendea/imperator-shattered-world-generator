using System;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IRandomNumberGenerator
    {
        Random Randomiser { get; }

        int Seed { get; }

        int Get(int min, int max);
    }
}
