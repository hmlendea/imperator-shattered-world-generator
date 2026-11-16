using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.UnitTests.Service.Models
{
    [TestFixture]
    public sealed class CityTests
    {
        [TestCase(0, 0, 0, 0, 0)]
        [TestCase(4, 8, 16, 32, 60)]
        [TestCase(4, 0, 0, 0, 4)]
        [TestCase(0, 0, 0, 613, 613)]
        public static void GivenPopulationCounts_WhenReadingTheTotalPopulation_ThenTheCountsAreSummed(
            int citizensCount,
            int freemenCount,
            int tribesmenCount,
            int slavesCount,
            int expectedTotalPopulation)
            => Assert.That(
                BuildCity(citizensCount, freemenCount, tribesmenCount, slavesCount).TotalPopulation,
                Is.EqualTo(expectedTotalPopulation));

        [Test]
        public static void GivenACityWithEveryAttribute_WhenCheckingHabitability_ThenItIsHabitable()
            => Assert.That(BuildCity(4, 0, 0, 0).IsHabitable);

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public static void GivenACityWithoutACulture_WhenCheckingHabitability_ThenItIsNotHabitable(string cultureId)
        {
            City city = BuildCity(4, 0, 0, 0);
            city.CultureId = cultureId;

            Assert.That(city.IsHabitable, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public static void GivenACityWithoutAReligion_WhenCheckingHabitability_ThenItIsNotHabitable(string religionId)
        {
            City city = BuildCity(4, 0, 0, 0);
            city.ReligionId = religionId;

            Assert.That(city.IsHabitable, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public static void GivenACityWithoutATradeGood_WhenCheckingHabitability_ThenItIsNotHabitable(string tradeGoodId)
        {
            City city = BuildCity(4, 0, 0, 0);
            city.TradeGoodId = tradeGoodId;

            Assert.That(city.IsHabitable, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public static void GivenACityWithoutAName_WhenCheckingHabitability_ThenItIsNotHabitable(string nameId)
        {
            City city = BuildCity(4, 0, 0, 0);
            city.NameId = nameId;

            Assert.That(city.IsHabitable, Is.False);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t\r\n")]
        public static void GivenACityWithoutAProvince_WhenCheckingHabitability_ThenItIsNotHabitable(string provinceId)
        {
            City city = BuildCity(4, 0, 0, 0);
            city.ProvinceId = provinceId;

            Assert.That(city.IsHabitable, Is.False);
        }

        private static City BuildCity(
            int citizensCount,
            int freemenCount,
            int tribesmenCount,
            int slavesCount)
            => new()
            {
                Id = "1",
                CultureId = "Roman",
                ReligionId = "Pagan",
                TradeGoodId = "Grain",
                NameId = "Newport",
                ProvinceId = "1",
                CitizensCount = citizensCount,
                FreemenCount = freemenCount,
                TribesmenCount = tribesmenCount,
                SlavesCount = slavesCount
            };
    }
}
