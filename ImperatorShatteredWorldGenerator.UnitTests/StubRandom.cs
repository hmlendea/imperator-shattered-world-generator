using System;

namespace ImperatorShatteredWorldGenerator.UnitTests
{
    internal sealed class StubRandom(int nextValue) : Random
    {
        public override int Next(int maximumValue)
        {
            if (maximumValue <= 0)
            {
                return 0;
            }

            return nextValue % maximumValue;
        }

        public override int Next(int minimumValue, int maximumValue)
        {
            int range = maximumValue - minimumValue;

            if (range <= 0)
            {
                return minimumValue;
            }

            return minimumValue + (nextValue % range);
        }

        public override double NextDouble() => 0;
    }
}
