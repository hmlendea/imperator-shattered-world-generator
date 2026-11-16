using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service
{
    [TestFixture]
    public sealed class CityGeneratorTests
    {
        private static readonly string[] CultureIds = ["roman", "greek"];
        private static readonly string[] ReligionIds = ["pagan", "hellenic"];

        private Mock<IEntityManager> entityManager = null!;
        private Mock<IRandomNumberGenerator> rng = null!;
        private GeneratorSettings settings = null!;
        private CityGenerator cityGenerator = null!;

        [SetUp]
        public void SetUp()
        {
            entityManager = new();
            rng = new();
            settings = new()
            {
                CapitalPopulation = 4,
                CityPopulationMin = 4,
                CityPopulationMax = 12,
                CityBarbarianLevelMin = 0,
                CityCivilisationLevelMax = 8,
                CityBarbarianLevelMax = 16
            };
            rng.SetupGet(generator => generator.Randomiser).Returns(new StubRandom(0));
            entityManager.Setup(manager => manager.GetCultureIds()).Returns(CultureIds);
            entityManager.Setup(manager => manager.GetReligionIds()).Returns(ReligionIds);

            cityGenerator = new(entityManager.Object, rng.Object, settings);
        }

        [Test]
        public void GivenACapital_WhenGeneratingTheCapital_ThenTheCountryCultureAndReligionAreApplied()
        {
            Country country = new() { Id = "COR", CapitalId = "1", CultureId = "greek", ReligionId = "hellenic" };
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);

            City capital = cityGenerator.GenerateCapital(country);

            Assert.That(capital, Is.SameAs(city));
            Assert.That(capital.CultureId, Is.EqualTo("greek"));
            Assert.That(capital.ReligionId, Is.EqualTo("hellenic"));
        }

        [Test]
        public void GivenACapital_WhenGeneratingTheCapital_ThenTheConfiguredCapitalPopulationIsUsed()
        {
            Country country = new() { Id = "COR", CapitalId = "1" };
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(0, 3)).Returns(3);

            City capital = cityGenerator.GenerateCapital(country);

            Assert.That(capital.TotalPopulation, Is.EqualTo(4));
            Assert.That(capital.CitizensCount, Is.EqualTo(4));
            rng.Verify(generator => generator.Get(0, 3), Times.Exactly(4));
        }

        [Test]
        public void GivenACityWithAnAlreadyMatchingPopulation_WhenGeneratingACity_ThenTheExistingDistributionIsKept()
        {
            City city = BuildCity("1");
            city.SlavesCount = 4;
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(4, 12)).Returns(4);

            City generatedCity = cityGenerator.GenerateCity("1");

            Assert.That(generatedCity.TotalPopulation, Is.EqualTo(4));
            Assert.That(generatedCity.SlavesCount, Is.EqualTo(4));
            Assert.That(generatedCity.CitizensCount, Is.EqualTo(0));
            rng.Verify(generator => generator.Get(0, 3), Times.Never);
        }

        [Test]
        public void GivenAnExistingPopulation_WhenGeneratingTheCapital_ThenThePreviousPopulationsAreDiscarded()
        {
            Country country = new() { Id = "COR", CapitalId = "1" };
            City city = BuildCity("1");
            city.CitizensCount = 128;
            city.SlavesCount = 64;
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(0, 3)).Returns(0);

            City capital = cityGenerator.GenerateCapital(country);

            Assert.That(capital.CitizensCount, Is.EqualTo(0));
            Assert.That(capital.SlavesCount, Is.EqualTo(4));
            Assert.That(capital.TotalPopulation, Is.EqualTo(4));
        }

        [Test]
        public void GivenARandomlyDistributedPopulation_WhenGeneratingACity_ThenThePopulationsAreDistributedByKind()
        {
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(4, 12)).Returns(4);
            rng.SetupSequence(generator => generator.Get(0, 3))
                .Returns(0)
                .Returns(1)
                .Returns(2)
                .Returns(3);

            City generatedCity = cityGenerator.GenerateCity("1");

            Assert.That(generatedCity.SlavesCount, Is.EqualTo(1));
            Assert.That(generatedCity.TribesmenCount, Is.EqualTo(1));
            Assert.That(generatedCity.FreemenCount, Is.EqualTo(1));
            Assert.That(generatedCity.CitizensCount, Is.EqualTo(1));
            Assert.That(generatedCity.TotalPopulation, Is.EqualTo(4));
            rng.Verify(generator => generator.Get(0, 3), Times.Exactly(4));
        }

        [Test]
        public void GivenACityPopulationRange_WhenGeneratingACity_ThenThePopulationIsDrawnFromTheConfiguredRange()
        {
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(4, 12)).Returns(12);
            rng.Setup(generator => generator.Get(0, 3)).Returns(0);

            City generatedCity = cityGenerator.GenerateCity("1");

            Assert.That(generatedCity.TotalPopulation, Is.EqualTo(12));
            rng.Verify(generator => generator.Get(4, 12), Times.Once);
        }

        [Test]
        public void GivenARandomCultureAndReligion_WhenGeneratingACity_ThenTheSelectedValuesAreApplied()
        {
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            StubRandom randomiser = new(1);
            rng.SetupGet(generator => generator.Randomiser).Returns(randomiser);

            City generatedCity = cityGenerator.GenerateCity("1");

            Assert.That(generatedCity.CultureId, Is.EqualTo("greek"));
            Assert.That(generatedCity.ReligionId, Is.EqualTo("hellenic"));
        }

        [Test]
        public void GivenRandomLevels_WhenGeneratingACity_ThenTheGeneratedLevelsAreApplied()
        {
            City city = BuildCity("1");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(city);
            rng.Setup(generator => generator.Get(0, 8)).Returns(8);
            rng.Setup(generator => generator.Get(0, 16)).Returns(16);

            City generatedCity = cityGenerator.GenerateCity("1");

            Assert.That(generatedCity.CivilizationLevel, Is.EqualTo(8));
            Assert.That(generatedCity.BarbarianLevel, Is.EqualTo(16));
        }

        [Test]
        public void GivenACityIdentifier_WhenGeneratingACity_ThenTheCityIsLoadedWithThatIdentifier()
        {
            entityManager.Setup(manager => manager.GetCity("873")).Returns(BuildCity("873"));

            City generatedCity = cityGenerator.GenerateCity("873");

            Assert.That(generatedCity.Id, Is.EqualTo("873"));
            entityManager.Verify(manager => manager.GetCity("873"), Times.Once);
        }

        [Test]
        public void GivenConfiguredBounds_WhenGeneratingManyCities_ThenEveryGeneratedValueStaysWithinItsBounds()
        {
            RandomNumberGenerator deterministicRng = new(settings);
            CityGenerator generator = new(entityManager.Object, deterministicRng, settings);
            entityManager.Setup(manager => manager.GetCity("1")).Returns(() => BuildCity("1"));

            for (int index = 0; index < 64; index++)
            {
                City city = generator.GenerateCity("1");

                Assert.That(city.TotalPopulation, Is.InRange(settings.CityPopulationMin, settings.CityPopulationMax));
                Assert.That(
                    city.CivilizationLevel,
                    Is.InRange(settings.CityBarbarianLevelMin, settings.CityCivilisationLevelMax));
                Assert.That(city.BarbarianLevel, Is.InRange(settings.CityBarbarianLevelMin, settings.CityBarbarianLevelMax));
                Assert.That(city.CultureId, Is.AnyOf(CultureIds));
                Assert.That(city.ReligionId, Is.AnyOf(ReligionIds));
            }
        }

        private static City BuildCity(string id)
            => new()
            {
                Id = id,
                CultureId = "roman",
                ReligionId = "pagan",
                TradeGoodId = "grain",
                NameId = "Newport",
                ProvinceId = id
            };
    }
}
