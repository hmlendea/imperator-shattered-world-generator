using System;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class RandomNumberGenerator : IRandomNumberGenerator
    {
        public Random Randomiser { get; }

        public int Seed { get; }

        public RandomNumberGenerator(int seed)
        {
            Randomiser = new Random(seed);
            Seed = seed;
        }

        public int Get(int min, int max)
        {
            if (min == max)
            {
                return max;
            }

            return Randomiser.Next(min, max + 1);
        }
    }
}
