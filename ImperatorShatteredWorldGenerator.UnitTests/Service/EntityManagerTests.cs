using System;
using System.Collections.Generic;

using Moq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service
{
    [TestFixture]
    public sealed class EntityManagerTests
    {
        private static readonly City RomanCity = new()
        {
            Id = "1",
            CultureId = "roman",
            ReligionId = "pagan",
            TradeGoodId = "grain",
            NameId = "Newport",
            ProvinceId = "1"
        };

        private static readonly City GreekCity = new()
        {
            Id = "2",
            CultureId = "greek",
            ReligionId = "hellenic",
            TradeGoodId = "wine",
            NameId = "Cornova",
            ProvinceId = "2"
        };

        private static readonly Country RomanCountry = new() { Id = "ROM", Name = "Roman Republic" };
        private static readonly Country GreekCountry = new() { Id = "GRE", Name = "Greece" };

        private static readonly string[] CultureIds = ["roman", "greek"];
        private static readonly string[] ReligionIds = ["pagan", "hellenic"];
        private static readonly string[] GovernmentIds = ["republic", "monarchy"];
        private static readonly string[] DiplomaticStanceIds = ["neutral", "aggressive"];

        private Mock<IEntityLoader> entityLoader = null!;
        private EntityManager entityManager = null!;

        [SetUp]
        public void SetUp()
        {
            entityLoader = new();
            entityManager = new(entityLoader.Object);
        }

        [Test]
        public void GivenANewlyConstructedManager_WhenUpdatingACity_ThenTheUpdateFails()
            => Assert.That(
                () => entityManager.UpdateCity(RomanCity),
                Throws.TypeOf<NullReferenceException>());

        [Test]
        public void GivenANewlyConstructedManager_WhenAddingACountry_ThenTheAdditionFails()
            => Assert.That(
                () => entityManager.AddCountry(RomanCountry),
                Throws.TypeOf<NullReferenceException>());

        [Test]
        public void GivenLoadedCities_WhenRequestingAnExistingCity_ThenItIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity, GreekCity]);

            City city = entityManager.GetCity("2");

            Assert.That(city, Is.SameAs(GreekCity));
            entityLoader.Verify(loader => loader.LoadCities(), Times.Once);
        }

        [Test]
        public void GivenLoadedCities_WhenRequestingAMissingCity_ThenNothingIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity]);

            City city = entityManager.GetCity("32");

            Assert.That(city, Is.Null);
        }

        [Test]
        public void GivenSeveralCityRequests_WhenRequestingCities_ThenTheCitiesAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity, GreekCity]);

            entityManager.GetCity("1");
            entityManager.GetCity("2");
            entityManager.GetCities();
            entityManager.GetCityIds();

            entityLoader.Verify(loader => loader.LoadCities(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenLoadedCities_WhenRequestingAllCities_ThenEveryCityIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity, GreekCity]);

            IEnumerable<City> cities = entityManager.GetCities();

            Assert.That(cities, Is.EqualTo([RomanCity, GreekCity]));
        }

        [Test]
        public void GivenLoadedCities_WhenRequestingAllCityIds_ThenEveryIdentifierIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity, GreekCity]);

            IEnumerable<string> cityIds = entityManager.GetCityIds();

            Assert.That(cityIds, Is.EqualTo(["1", "2"]));
        }

        [Test]
        public void GivenLoadedCities_WhenUpdatingACity_ThenTheUpdatedCityIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([RomanCity, GreekCity]);
            City updatedCity = new() { Id = "1", NameId = "Solara" };

            entityManager.GetCities();
            entityManager.UpdateCity(updatedCity);

            Assert.That(entityManager.GetCity("1"), Is.SameAs(updatedCity));
            entityLoader.Verify(loader => loader.LoadCities(), Times.Once);
        }

        [Test]
        public void GivenLoadedCountries_WhenRequestingAnExistingCountry_ThenItIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([RomanCountry, GreekCountry]);

            Country country = entityManager.GetCountry("GRE");

            Assert.That(country, Is.SameAs(GreekCountry));
            entityLoader.Verify(loader => loader.LoadCountries(), Times.Once);
        }

        [Test]
        public void GivenLoadedCountries_WhenRequestingAMissingCountry_ThenNothingIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([RomanCountry]);

            Country country = entityManager.GetCountry("COR");

            Assert.That(country, Is.Null);
        }

        [Test]
        public void GivenLoadedCountries_WhenRequestingAllCountries_ThenEveryCountryIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([RomanCountry, GreekCountry]);

            IEnumerable<Country> countries = entityManager.GetCountries();

            Assert.That(countries, Is.EqualTo([RomanCountry, GreekCountry]));
        }

        [Test]
        public void GivenLoadedCountries_WhenAddingACountry_ThenItBecomesRetrievable()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([RomanCountry]);
            Country country = new() { Id = "COR", Name = "Cornova" };

            entityManager.GetCountries();
            entityManager.AddCountry(country);

            Assert.That(entityManager.GetCountry("COR"), Is.SameAs(country));
            Assert.That(entityManager.GetCountries(), Has.Count.EqualTo(2));
        }

        [Test]
        public void GivenSeveralCountryRequests_WhenRequestingCountries_ThenTheCountriesAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([RomanCountry]);

            entityManager.GetCountry("ROM");
            entityManager.GetCountries();
            entityManager.GetCountry("COR");

            entityLoader.Verify(loader => loader.LoadCountries(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenSeveralCultureRequests_WhenRequestingCultureIds_ThenTheCultureIdsAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadCultureIds()).Returns(CultureIds);

            IEnumerable<string> cultureIds = entityManager.GetCultureIds();
            IEnumerable<string> otherCultureIds = entityManager.GetCultureIds();

            Assert.That(cultureIds, Is.EqualTo(CultureIds));
            Assert.That(otherCultureIds, Is.SameAs(cultureIds));
            entityLoader.Verify(loader => loader.LoadCultureIds(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenSeveralReligionRequests_WhenRequestingReligionIds_ThenTheReligionIdsAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadReligionIds()).Returns(ReligionIds);

            IEnumerable<string> religionIds = entityManager.GetReligionIds();
            IEnumerable<string> otherReligionIds = entityManager.GetReligionIds();

            Assert.That(religionIds, Is.EqualTo(ReligionIds));
            Assert.That(otherReligionIds, Is.SameAs(religionIds));
            entityLoader.Verify(loader => loader.LoadReligionIds(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenSeveralGovernmentRequests_WhenRequestingGovernmentIds_ThenTheGovernmentIdsAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadGovernmentIds()).Returns(GovernmentIds);

            IEnumerable<string> governmentIds = entityManager.GetGovernmentIds();
            IEnumerable<string> otherGovernmentIds = entityManager.GetGovernmentIds();

            Assert.That(governmentIds, Is.EqualTo(GovernmentIds));
            Assert.That(otherGovernmentIds, Is.SameAs(governmentIds));
            entityLoader.Verify(loader => loader.LoadGovernmentIds(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenSeveralDiplomaticStanceRequests_WhenRequestingDiplomaticStanceIds_ThenTheyAreLoadedOnlyOnce()
        {
            entityLoader.Setup(loader => loader.LoadDiplomaticStanceIds()).Returns(DiplomaticStanceIds);

            IEnumerable<string> diplomaticStanceIds = entityManager.GetDiplomaticStanceIds();
            IEnumerable<string> otherDiplomaticStanceIds = entityManager.GetDiplomaticStanceIds();

            Assert.That(diplomaticStanceIds, Is.EqualTo(DiplomaticStanceIds));
            Assert.That(otherDiplomaticStanceIds, Is.SameAs(diplomaticStanceIds));
            entityLoader.Verify(loader => loader.LoadDiplomaticStanceIds(), Times.Once);
            entityLoader.VerifyNoOtherCalls();
        }

        [Test]
        public void GivenNoCities_WhenRequestingAllCityIds_ThenNothingIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCities()).Returns([]);

            IEnumerable<string> cityIds = entityManager.GetCityIds();

            Assert.That(cityIds, Is.Empty);
        }

        [Test]
        public void GivenNoCultures_WhenRequestingCultureIds_ThenNothingIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCultureIds()).Returns([]);

            IEnumerable<string> cultureIds = entityManager.GetCultureIds();

            Assert.That(cultureIds, Is.Empty);
        }

        [Test]
        public void GivenNoCountries_WhenRequestingAllCountries_ThenNothingIsReturned()
        {
            entityLoader.Setup(loader => loader.LoadCountries()).Returns([]);

            IEnumerable<Country> countries = entityManager.GetCountries();

            Assert.That(countries, Is.Empty);
        }
    }
}
