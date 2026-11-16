using System;
using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service
{
    [TestFixture]
    public sealed class ShatteredWorldGeneratorTests
    {
        private Mock<IEntityManager> entityManager = null!;
        private Mock<ICityGenerator> cityGenerator = null!;
        private Mock<ICountryGenerator> countryGenerator = null!;
        private Mock<IModWriter> modWriter = null!;
        private Mock<IRandomNumberGenerator> rng = null!;
        private GeneratorSettings settings = null!;
        private ShatteredWorldGenerator shatteredWorldGenerator = null!;

        [SetUp]
        public void SetUp()
        {
            entityManager = new();
            cityGenerator = new();
            countryGenerator = new();
            modWriter = new();
            rng = new();
            settings = new() { RandomCountriesCount = 0 };
            rng.SetupGet(generator => generator.Randomiser).Returns(new SequenceRandom(0));

            shatteredWorldGenerator = new(
                entityManager.Object,
                cityGenerator.Object,
                countryGenerator.Object,
                modWriter.Object,
                rng.Object,
                settings);
        }

        [Test]
        public void GivenLoadedCities_WhenGenerating_ThenEveryCityIsRegeneratedAndStored()
        {
            City firstCity = BuildCity("1", "Newport");
            City secondCity = BuildCity("2", "Cornova");
            entityManager.Setup(manager => manager.GetCityIds()).Returns(["1", "2"]);
            cityGenerator.Setup(generator => generator.GenerateCity("1")).Returns(firstCity);
            cityGenerator.Setup(generator => generator.GenerateCity("2")).Returns(secondCity);
            entityManager.Setup(manager => manager.GetCities()).Returns([firstCity, secondCity]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            shatteredWorldGenerator.Generate();

            entityManager.Verify(manager => manager.UpdateCity(firstCity), Times.Once);
            entityManager.Verify(manager => manager.UpdateCity(secondCity), Times.Once);
            cityGenerator.Verify(generator => generator.GenerateCity("1"), Times.Once);
            cityGenerator.Verify(generator => generator.GenerateCity("2"), Times.Once);
        }

        [Test]
        public void GivenNoCities_WhenGenerating_ThenNoCityIsGeneratedOrStored()
        {
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            shatteredWorldGenerator.Generate();

            cityGenerator.VerifyNoOtherCalls();
            entityManager.Verify(manager => manager.UpdateCity(It.IsAny<City>()), Times.Never);
            entityManager.Verify(manager => manager.AddCountry(It.IsAny<Country>()), Times.Never);
        }

        [Test]
        public void GivenNoRandomCountriesRequested_WhenGenerating_ThenNoCountryIsGeneratedOrAdded()
        {
            settings.RandomCountriesCount = 0;

            shatteredWorldGenerator.Generate();

            countryGenerator.VerifyNoOtherCalls();
            entityManager.Verify(manager => manager.AddCountry(It.IsAny<Country>()), Times.Never);
            entityManager.Verify(manager => manager.GetCountries(), Times.Once);
        }

        [Test]
        public void GivenRandomCountriesRequested_WhenGenerating_ThenTheConfiguredNumberOfCountriesIsCreated()
        {
            List<Country> countries = [];
            Country firstCountry = new() { Id = "NEW", CapitalId = "1" };
            Country secondCountry = new() { Id = "COR", CapitalId = "2" };
            settings.RandomCountriesCount = 2;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport"), BuildCity("2", "Cornova")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            countryGenerator.Setup(generator => generator.GenerateCountry("1")).Returns(firstCountry);
            countryGenerator.Setup(generator => generator.GenerateCountry("2")).Returns(secondCountry);
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Returns<Country>(country => BuildCity(country.CapitalId, "Newport"));

            shatteredWorldGenerator.Generate();

            Assert.That(countries, Is.EqualTo([firstCountry, secondCountry]));
            countryGenerator.Verify(generator => generator.GenerateCountry("1"), Times.Once);
            countryGenerator.Verify(generator => generator.GenerateCountry("2"), Times.Once);
            entityManager.Verify(manager => manager.AddCountry(firstCountry), Times.Once);
            entityManager.Verify(manager => manager.AddCountry(secondCountry), Times.Once);
        }

        [Test]
        public void GivenCitiesSharingAName_WhenGeneratingCountries_ThenOnlyOneCountryIsCreatedForThatName()
        {
            List<Country> countries = [];
            Country country = new() { Id = "NEW", CapitalId = "1" };
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities())
                .Returns([BuildCity("1", "Newport"), BuildCity("2", "Newport")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(addedCountry => countries.Add(addedCountry));
            countryGenerator.Setup(generator => generator.GenerateCountry("1")).Returns(country);
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Returns<Country>(generatedCountry => BuildCity(generatedCountry.CapitalId, "Newport"));

            shatteredWorldGenerator.Generate();

            Assert.That(countries, Has.Count.EqualTo(1));
            countryGenerator.Verify(generator => generator.GenerateCountry("2"), Times.Never);
        }

        [Test]
        public void GivenACityUsedAsAVanillaCapital_WhenGeneratingCountries_ThenItIsNotUsedAsANewCountry()
        {
            List<Country> countries = [new Country { Id = "ROM", CapitalId = "1", Name = "Roman Republic" }];
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport"), BuildCity("2", "Cornova")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            Country newCountry = new() { Id = "COR", CapitalId = "2" };
            countryGenerator.Setup(generator => generator.GenerateCountry("2")).Returns(newCountry);
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Returns<Country>(country => BuildCity(country.CapitalId, "Cornova"));

            shatteredWorldGenerator.Generate();

            countryGenerator.Verify(generator => generator.GenerateCountry("1"), Times.Never);
            countryGenerator.Verify(generator => generator.GenerateCountry("2"), Times.Once);
        }

        [Test]
        public void GivenACityNamedAfterAVanillaCountry_WhenGeneratingCountries_ThenItIsNotUsedAsANewCountry()
        {
            List<Country> countries = [new Country { Id = "ROM", CapitalId = "9", Name = "Newport" }];
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport"), BuildCity("2", "Cornova")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            Country newCountry = new() { Id = "COR", CapitalId = "2" };
            countryGenerator.Setup(generator => generator.GenerateCountry("2")).Returns(newCountry);
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Returns<Country>(country => BuildCity(country.CapitalId, "Cornova"));

            shatteredWorldGenerator.Generate();

            countryGenerator.Verify(generator => generator.GenerateCountry("1"), Times.Never);
            countryGenerator.Verify(generator => generator.GenerateCountry("2"), Times.Once);
        }

        [Test]
        public void GivenAnInhabitableCity_WhenGeneratingCountries_ThenItIsNotUsedAsANewCountry()
        {
            List<Country> countries = [];
            City uninhabitableCity = BuildCity("1", "Newport");
            uninhabitableCity.CultureId = null;
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([uninhabitableCity, BuildCity("2", "Cornova")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            countryGenerator.Setup(generator => generator.GenerateCountry("2")).Returns(new Country { Id = "COR", CapitalId = "2" });
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Returns<Country>(country => BuildCity(country.CapitalId, "Cornova"));

            shatteredWorldGenerator.Generate();

            countryGenerator.Verify(generator => generator.GenerateCountry("1"), Times.Never);
            countryGenerator.Verify(generator => generator.GenerateCountry("2"), Times.Once);
        }

        [Test]
        public void GivenNoVacantCities_WhenGeneratingCountries_ThenTheGenerationFails()
        {
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            Assert.That(
                () => shatteredWorldGenerator.Generate(),
                Throws.TypeOf<InvalidOperationException>());
            entityManager.Verify(manager => manager.AddCountry(It.IsAny<Country>()), Times.Never);
        }

        [Test]
        public void GivenMoreCountriesThanVacantCities_WhenGeneratingCountries_ThenTheGenerationFails()
        {
            settings.RandomCountriesCount = 2;
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport")]);
            List<Country> countries = [];
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            countryGenerator.Setup(generator => generator.GenerateCountry("1")).Returns(new Country { Id = "NEW", CapitalId = "1" });

            Assert.That(
                () => shatteredWorldGenerator.Generate(),
                Throws.TypeOf<InvalidOperationException>());
            entityManager.Verify(manager => manager.AddCountry(It.IsAny<Country>()), Times.Once);
        }

        [Test]
        public void GivenCountries_WhenGeneratingCapitals_ThenEveryCapitalIsRegeneratedAndStored()
        {
            List<Country> countries = [new Country { Id = "ROM", CapitalId = "1" }];
            City capital = BuildCity("1", "Newport");
            entityManager.Setup(manager => manager.GetCityIds()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(countries);
            cityGenerator.Setup(generator => generator.GenerateCapital(countries[0])).Returns(capital);

            shatteredWorldGenerator.Generate();

            entityManager.Verify(manager => manager.UpdateCity(capital), Times.Once);
            cityGenerator.Verify(generator => generator.GenerateCapital(countries[0]), Times.Once);
        }

        [Test]
        public void GivenAWorldGeneration_WhenGenerating_ThenCitiesCountriesCapitalsAndTheModAreProducedInOrder()
        {
            List<string> callOrder = [];
            List<Country> countries = [];
            settings.RandomCountriesCount = 1;
            entityManager.Setup(manager => manager.GetCityIds())
                .Callback(() => callOrder.Add("cities"))
                .Returns(["1"]);
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns(() => countries);
            entityManager.Setup(manager => manager.AddCountry(It.IsAny<Country>()))
                .Callback<Country>(country => countries.Add(country));
            cityGenerator.Setup(generator => generator.GenerateCity("1")).Returns(BuildCity("1", "Newport"));
            countryGenerator.Setup(generator => generator.GenerateCountry("1"))
                .Callback(() => callOrder.Add("countries"))
                .Returns(new Country { Id = "NEW", CapitalId = "1" });
            cityGenerator.Setup(generator => generator.GenerateCapital(It.IsAny<Country>()))
                .Callback(() => callOrder.Add("capitals"))
                .Returns(BuildCity("1", "Newport"));
            modWriter.Setup(writer => writer.CreateMod()).Callback(() => callOrder.Add("mod"));

            shatteredWorldGenerator.Generate();

            Assert.That(callOrder, Is.EqualTo(["cities", "countries", "capitals", "mod"]));
            modWriter.Verify(writer => writer.CreateMod(), Times.Once);
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
