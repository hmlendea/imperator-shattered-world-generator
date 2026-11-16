using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class CountryTests
    {
        private static readonly string[] DefaultShipNames =
        [
            "Boaty",
            "McBoatface",
            "Boatface",
            "Shippy",
            "McShipface",
            "Shipface"
        ];

        [Test]
        public static void GivenANewCountry_WhenReadingTheShipNames_ThenTheDefaultNamesAreUsed()
            => Assert.That(new Country().ShipNames, Is.EqualTo(DefaultShipNames));

        [Test]
        public static void GivenANewCountry_WhenReadingTheShipNames_ThenSixNamesAreProvided()
            => Assert.That(new Country().ShipNames, Has.Count.EqualTo(6));

        [Test]
        public static void GivenNewCountries_WhenReadingTheShipNames_ThenEachCountryHasItsOwnCollection()
            => Assert.That(new Country().ShipNames, Is.Not.SameAs(new Country().ShipNames));

        [Test]
        public static void GivenANewCountry_WhenReadingTheCentralisationLevel_ThenItIsLeftUnset()
            => Assert.That(new Country().CentralisationLevel, Is.EqualTo(-1));

        [Test]
        public static void GivenANewCountry_WhenReadingTheVanillaFlag_ThenItIsNotVanilla()
            => Assert.That(new Country().IsVanilla, Is.False);

        [Test]
        public static void GivenANewCountry_WhenReadingTheColour_ThenItIsNotSet()
        {
            Country country = new();

            Assert.That(country.ColourRed, Is.EqualTo(0));
            Assert.That(country.ColourGreen, Is.EqualTo(0));
            Assert.That(country.ColourBlue, Is.EqualTo(0));
        }

        [Test]
        public static void GivenANewCountry_WhenReadingTheIdentifiers_ThenTheyAreNotSet()
        {
            Country country = new();

            Assert.That(country.Id, Is.Null);
            Assert.That(country.Name, Is.Null);
            Assert.That(country.CultureId, Is.Null);
            Assert.That(country.ReligionId, Is.Null);
            Assert.That(country.GovernmentId, Is.Null);
            Assert.That(country.DiplomaticStanceId, Is.Null);
            Assert.That(country.CapitalId, Is.Null);
        }
    }
}
