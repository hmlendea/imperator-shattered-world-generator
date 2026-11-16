using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service
{
    [TestFixture]
    public sealed class RandomNumberGeneratorTests
    {
        [Test]
        public static void GivenASeed_WhenConstructingTheGenerator_ThenTheRandomiserIsSeeded()
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = 42 });
            RandomNumberGenerator otherGenerator = new(new GeneratorSettings { Seed = 42 });

            IEnumerable<int> values = Enumerable.Range(0, 16).Select(_ => generator.Randomiser.Next());
            IEnumerable<int> otherValues = Enumerable.Range(0, 16).Select(_ => otherGenerator.Randomiser.Next());

            Assert.That(generator.Randomiser, Is.Not.Null);
            Assert.That(values, Is.EqualTo(otherValues));
        }

        [TestCase(0, 0)]
        [TestCase(8, 8)]
        [TestCase(-42, -42)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public static void GivenEqualBounds_WhenGenerating_ThenTheBoundValueIsReturned(int minimum, int maximum)
            => Assert.That(
                new RandomNumberGenerator(new GeneratorSettings { Seed = 42 }).Get(minimum, maximum),
                Is.EqualTo(maximum));

        [TestCase(0, 3, 42)]
        [TestCase(4, 12, 42)]
        [TestCase(0, 100, 613)]
        [TestCase(0, 255, 873)]
        public static void GivenABoundRange_WhenGenerating_ThenTheResultStaysWithinTheBounds(
            int minimum,
            int maximum,
            int seed)
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = seed });

            IEnumerable<int> values = Enumerable.Range(0, 256).Select(_ => generator.Get(minimum, maximum));

            Assert.That(values, Is.All.InRange(minimum, maximum));
        }

        [Test]
        public static void GivenABoundRangeOverManyDraws_WhenGenerating_ThenBothBoundsAreReachable()
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = 42 });

            IEnumerable<int> values = Enumerable.Range(0, 1024).Select(_ => generator.Get(0, 3)).ToList();

            Assert.That(values, Does.Contain(0));
            Assert.That(values, Does.Contain(3));
        }

        [Test]
        public static void GivenTheSameSeed_WhenGenerating_ThenTheSameSequenceIsProduced()
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = 42 });
            RandomNumberGenerator otherGenerator = new(new GeneratorSettings { Seed = 42 });

            IEnumerable<int> values = Enumerable.Range(0, 64).Select(_ => generator.Get(0, 100));
            IEnumerable<int> otherValues = Enumerable.Range(0, 64).Select(_ => otherGenerator.Get(0, 100));

            Assert.That(values, Is.EqualTo(otherValues));
        }

        [Test]
        public static void GivenDifferentSeeds_WhenGenerating_ThenDifferentSequencesAreProduced()
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = 42 });
            RandomNumberGenerator otherGenerator = new(new GeneratorSettings { Seed = 873 });

            IEnumerable<int> values = Enumerable.Range(0, 64).Select(_ => generator.Get(0, 100));
            IEnumerable<int> otherValues = Enumerable.Range(0, 64).Select(_ => otherGenerator.Get(0, 100));

            Assert.That(values, Is.Not.EqualTo(otherValues));
        }

        [Test]
        public static void GivenASingleValueRange_WhenGenerating_ThenTheRangeSpansTheMinimumAndMaximum()
        {
            RandomNumberGenerator generator = new(new GeneratorSettings { Seed = 42 });

            IEnumerable<int> inclusiveValues = Enumerable.Range(0, 64).Select(_ => generator.Get(4, 5)).ToList();

            Assert.That(inclusiveValues, Does.Contain(4));
            Assert.That(inclusiveValues, Does.Contain(5));
        }
    }
}
