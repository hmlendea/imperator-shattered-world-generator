using System;
using System.Collections.Generic;

namespace ImperatorShatteredWorldGenerator.UnitTests
{
    internal sealed class SequenceRandom(params int[] values) : Random
    {
        private readonly IReadOnlyList<int> values = values;
        private int position;

        public override int Next(int maximumValue)
        {
            if (maximumValue <= 0)
            {
                return 0;
            }

            return TakeValue() % maximumValue;
        }

        public override int Next(int minimumValue, int maximumValue)
        {
            int range = maximumValue - minimumValue;

            if (range <= 0)
            {
                return minimumValue;
            }

            return minimumValue + (TakeValue() % range);
        }

        public override double NextDouble() => 0;

        private int TakeValue()
        {
            if (values.Count == 0)
            {
                return 0;
            }

            int value = values[position];
            position += 1;

            if (position >= values.Count)
            {
                position = 0;
            }

            return value;
        }
    }
}
