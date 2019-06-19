using System;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IRandomNumberGenerator
    {
        Random Randomiser { get; }

        int Get(int min, int max);
    }
}
