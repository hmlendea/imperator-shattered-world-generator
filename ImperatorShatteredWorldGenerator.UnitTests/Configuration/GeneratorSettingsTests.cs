using System;
using System.IO;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;

namespace ImperatorShatteredWorldGenerator.UnitTests.Configuration
{
    [TestFixture]
    public sealed class GeneratorSettingsTests
    {
        [Test]
        public static void GivenNewSettings_WhenReadingTheSeed_ThenItIsGeneratedWithinTheValidRange()
            => Assert.That(
                new GeneratorSettings().Seed,
                Is.InRange(0, int.MaxValue - 1));

        [Test]
        public static void GivenNewSettings_WhenReadingTheNameAndGameDirectory_ThenTheyAreNotSet()
        {
            GeneratorSettings settings = new();

            Assert.That(settings.ModName, Is.Null);
            Assert.That(settings.GameDirectoryPath, Is.Null);
        }

        [Test]
        public static void GivenNewSettings_WhenReadingThePopulationSettings_ThenTheDocumentedDefaultsAreUsed()
        {
            GeneratorSettings settings = new();

            Assert.That(settings.CapitalPopulation, Is.EqualTo(10));
            Assert.That(settings.CityPopulationMin, Is.EqualTo(4));
            Assert.That(settings.CityPopulationMax, Is.EqualTo(12));
        }

        [Test]
        public static void GivenNewSettings_WhenReadingTheCityLevels_ThenTheDocumentedDefaultsAreUsed()
        {
            GeneratorSettings settings = new();

            Assert.That(settings.CityCivilisationLevelMin, Is.EqualTo(0));
            Assert.That(settings.CityCivilisationLevelMax, Is.EqualTo(20));
            Assert.That(settings.CityBarbarianLevelMin, Is.EqualTo(0));
            Assert.That(settings.CityBarbarianLevelMax, Is.EqualTo(0));
        }

        [Test]
        public static void GivenNewSettings_WhenReadingTheCountrySettings_ThenTheDocumentedDefaultsAreUsed()
        {
            GeneratorSettings settings = new();

            Assert.That(settings.CountryCentralisationLevelMin, Is.EqualTo(0));
            Assert.That(settings.CountryCentralisationLevelMax, Is.EqualTo(100));
            Assert.That(settings.RandomCountriesCount, Is.EqualTo(500));
        }

        [TestCase("Shattered World", "shattered-world")]
        [TestCase("Shattered", "shattered")]
        [TestCase("NEW WORLD", "new-world")]
        [TestCase("Shattered  World", "shattered--world")]
        [TestCase("Çupișan's World!", "cupisans-world")]
        [TestCase("Țări Frumoase", "tari-frumoase")]
        public static void GivenAModName_WhenReadingTheModId_ThenPunctuationDiacriticsSpacesAndCasingAreNormalised(
            string modName,
            string expectedModId)
            => Assert.That(
                new GeneratorSettings { ModName = modName }.ModId,
                Is.EqualTo(expectedModId));

        [Test]
        public static void GivenAModName_WhenReadingTheModDirectoryPath_ThenItIsPlacedInTheOutputDirectoryOfTheCurrentDirectory()
            => Assert.That(
                new GeneratorSettings { ModName = "Shattered World" }.ModDirectoryPath,
                Is.EqualTo(Path.Combine(Environment.CurrentDirectory, "out", "shattered-world")));

        [Test]
        public static void GivenAModName_WhenReadingTheModFilePath_ThenItIsDerivedFromTheModDirectoryPath()
        {
            GeneratorSettings settings = new() { ModName = "Shattered World" };

            Assert.That(settings.ModFilePath, Is.EqualTo(settings.ModDirectoryPath + ".mod"));
        }
    }
}
