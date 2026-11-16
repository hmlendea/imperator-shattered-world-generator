using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service
{
    [TestFixture]
    public sealed class CountryGeneratorTests
    {
        private static readonly string[] GovernmentIds = ["republic", "monarchy"];
        private static readonly string[] DiplomaticStanceIds = ["neutral", "aggressive"];

        private Mock<IEntityManager> entityManager = null!;
        private Mock<IRandomNumberGenerator> rng = null!;
        private GeneratorSettings settings = null!;
        private CountryGenerator countryGenerator = null!;

        [SetUp]
        public void SetUp()
        {
            entityManager = new();
            rng = new();
            settings = new()
            {
                CountryCentralisationLevelMin = 0,
                CountryCentralisationLevelMax = 100
            };
            rng.SetupGet(generator => generator.Randomiser).Returns(new StubRandom(0));
            rng.Setup(generator => generator.Get(0, 100)).Returns(42);
            rng.SetupSequence(generator => generator.Get(0, 255))
                .Returns(4)
                .Returns(8)
                .Returns(16);
            entityManager.Setup(manager => manager.GetGovernmentIds()).Returns(GovernmentIds);
            entityManager.Setup(manager => manager.GetDiplomaticStanceIds()).Returns(DiplomaticStanceIds);

            countryGenerator = new(entityManager.Object, rng.Object, settings);
        }

        [Test]
        public void GivenACapitalCity_WhenGeneratingACountry_ThenTheIdentityAndLocationAreTakenFromTheCapital()
        {
            City capital = BuildCity("1", "Cornova");
            entityManager.Setup(manager => manager.GetCity("1")).Returns(capital);

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.Id, Is.EqualTo("COR"));
            Assert.That(country.Name, Is.EqualTo("Cornova"));
            Assert.That(country.CultureId, Is.EqualTo(capital.CultureId));
            Assert.That(country.ReligionId, Is.EqualTo(capital.ReligionId));
            Assert.That(country.CapitalId, Is.EqualTo("1"));
        }

        [TestCase("New-port", "NEW")]
        [TestCase("New California Republic", "NEW")]
        [TestCase("Cornova", "COR")]
        [TestCase("Çupișan", "UPI")]
        [TestCase("C+ornova", "COR")]
        [TestCase("Rome2", "ROM")]
        [TestCase("Mercia", "MEC")]
        [TestCase("Roman Empire", "ROM")]
        public void GivenACapitalCityName_WhenGeneratingACountry_ThenTheCountryIdentifierIsDerivedFromItsLetters(
            string nameId,
            string expectedId)
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", nameId));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.Id, Is.EqualTo(expectedId));
            Assert.That(country.Name, Is.EqualTo(nameId));
        }

        [TestCase("Al")]
        [TestCase("REB")]
        [TestCase("MER")]
        [TestCase("")]
        [TestCase(" ")]
        public void GivenACapitalCityNameWithoutAValidCombination_WhenGeneratingACountry_ThenARandomIdentifierIsUsed(
            string nameId)
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", nameId));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.Id, Is.EqualTo("AAA"));
            Assert.That(country.Name, Is.EqualTo(nameId));
        }

        [Test]
        public void GivenACapitalCityNameWhoseIdentifierIsTaken_WhenGeneratingACountry_ThenTheNextCombinationIsUsed()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Cornova"));
            entityManager.Setup(manager => manager.GetCountry("COR")).Returns(new Country { Id = "COR" });

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.Id, Is.EqualTo("CON"));
        }

        [Test]
        public void GivenARandomIdentifierCollision_WhenGeneratingACountry_ThenAnotherRandomIdentifierIsUsed()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Al"));
            entityManager.Setup(manager => manager.GetCountry("AAA")).Returns(new Country { Id = "AAA" });
            rng.SetupGet(generator => generator.Randomiser).Returns(new SequenceRandom(0, 0, 0, 12, 4, 17, 2, 14, 17));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.Id, Is.EqualTo("COR"));
        }

        [Test]
        public void GivenMultipleGovernmentsAndStances_WhenGeneratingACountry_ThenARandomEntryOfEachIsUsed()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Cornova"));
            rng.SetupGet(generator => generator.Randomiser).Returns(new StubRandom(1));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.GovernmentId, Is.EqualTo("monarchy"));
            Assert.That(country.DiplomaticStanceId, Is.EqualTo("aggressive"));
        }

        [Test]
        public void GivenACentralisationRange_WhenGeneratingACountry_ThenTheCentralisationIsDrawnFromTheConfiguredRange()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Cornova"));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.CentralisationLevel, Is.EqualTo(42));
            rng.Verify(generator => generator.Get(0, 100), Times.Once);
        }

        [Test]
        public void GivenACountry_WhenGeneratingACountry_ThenTheColourIsDrawnFromTheFullColourRange()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Cornova"));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.ColourRed, Is.EqualTo(4));
            Assert.That(country.ColourGreen, Is.EqualTo(8));
            Assert.That(country.ColourBlue, Is.EqualTo(16));
            rng.Verify(generator => generator.Get(0, 255), Times.Exactly(3));
        }

        [Test]
        public void GivenACountry_WhenGeneratingACountry_ThenTheDefaultShipNamesAndVanillaFlagAreKept()
        {
            entityManager.Setup(manager => manager.GetCity("1")).Returns(BuildCity("1", "Cornova"));

            Country country = countryGenerator.GenerateCountry("1");

            Assert.That(country.ShipNames, Has.Count.EqualTo(6));
            Assert.That(country.IsVanilla, Is.False);
        }

        private static City BuildCity(string id, string nameId)
            => new()
            {
                Id = id,
                CultureId = "roman",
                ReligionId = "pagan",
                TradeGoodId = "grain",
                NameId = nameId,
                ProvinceId = id
            };
    }
}
