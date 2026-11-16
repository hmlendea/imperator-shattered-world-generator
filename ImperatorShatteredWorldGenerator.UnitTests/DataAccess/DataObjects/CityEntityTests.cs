using NUnit.Framework;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;

namespace ImperatorShatteredWorldGenerator.UnitTests.DataAccess.DataObjects
{
    [TestFixture]
    public sealed class CityEntityTests
    {
        [Test]
        public static void GivenANewCityEntity_WhenReadingTheIdentity_ThenItIsNotSet()
            => Assert.That(new CityEntity().Id, Is.Null);

        [Test]
        public static void GivenACityEntity_WhenReadingTheTextAttributes_ThenTheyAreNotSet()
        {
            CityEntity cityEntity = new();

            Assert.That(cityEntity.CultureId, Is.Null);
            Assert.That(cityEntity.ReligionId, Is.Null);
            Assert.That(cityEntity.TradeGoodId, Is.Null);
            Assert.That(cityEntity.CivilizationLevel, Is.Null);
            Assert.That(cityEntity.BarbarianLevel, Is.Null);
            Assert.That(cityEntity.NameId, Is.Null);
            Assert.That(cityEntity.ProvinceId, Is.Null);
        }

        [Test]
        public static void GivenANewCityEntity_WhenReadingThePopulationCounts_ThenTheyAreNotSet()
        {
            CityEntity cityEntity = new();

            Assert.That(cityEntity.CitizensCount, Is.EqualTo(0));
            Assert.That(cityEntity.FreemenCount, Is.EqualTo(0));
            Assert.That(cityEntity.TribesmenCount, Is.EqualTo(0));
            Assert.That(cityEntity.SlavesCount, Is.EqualTo(0));
        }

        [Test]
        public static void GivenACityEntity_WhenSettingEveryAttribute_ThenEveryValueIsRetained()
        {
            CityEntity cityEntity = BuildCityEntity();

            Assert.That(cityEntity.Id, Is.EqualTo("1"));
            Assert.That(cityEntity.CultureId, Is.EqualTo("roman"));
            Assert.That(cityEntity.ReligionId, Is.EqualTo("pagan"));
            Assert.That(cityEntity.TradeGoodId, Is.EqualTo("grain"));
            Assert.That(cityEntity.CitizensCount, Is.EqualTo(4));
            Assert.That(cityEntity.FreemenCount, Is.EqualTo(8));
            Assert.That(cityEntity.TribesmenCount, Is.EqualTo(16));
            Assert.That(cityEntity.SlavesCount, Is.EqualTo(32));
            Assert.That(cityEntity.CivilizationLevel, Is.EqualTo("8"));
            Assert.That(cityEntity.BarbarianLevel, Is.EqualTo("16"));
            Assert.That(cityEntity.NameId, Is.EqualTo("Newport"));
            Assert.That(cityEntity.ProvinceId, Is.EqualTo("1"));
        }

        private static CityEntity BuildCityEntity()
            => new()
            {
                Id = "1",
                CultureId = "roman",
                ReligionId = "pagan",
                TradeGoodId = "grain",
                CitizensCount = 4,
                FreemenCount = 8,
                TribesmenCount = 16,
                SlavesCount = 32,
                CivilizationLevel = "8",
                BarbarianLevel = "16",
                NameId = "Newport",
                ProvinceId = "1"
            };
    }
}
