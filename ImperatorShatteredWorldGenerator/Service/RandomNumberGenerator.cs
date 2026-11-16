using System;

using ImperatorShatteredWorldGenerator.Configuration;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class RandomNumberGenerator : IRandomNumberGenerator
    {
        public Random Randomiser { get; }

        public RandomNumberGenerator(GeneratorSettings settings)
        {
            Randomiser = new Random(settings.Seed);
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
