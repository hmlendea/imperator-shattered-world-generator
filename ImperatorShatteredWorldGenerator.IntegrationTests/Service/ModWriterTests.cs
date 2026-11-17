using System;
using System.IO;

using Moq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.IntegrationTests.Service
{
    [TestFixture]
    public sealed class ModWriterTests
    {
        private TemporaryDirectory workingDirectory = null!;
        private WorkingDirectoryScope workingDirectoryScope = null!;
        private Mock<IEntityManager> entityManager = null!;
        private GeneratorSettings settings = null!;
        private ModWriter modWriter = null!;

        [SetUp]
        public void SetUp()
        {
            workingDirectory = new();
            workingDirectoryScope = new(workingDirectory.DirectoryPath);
            entityManager = new();
            settings = new() { ModName = "Shattered World" };
            modWriter = new(entityManager.Object, settings);
        }

        [TearDown]
        public void TearDown()
        {
            workingDirectoryScope.Dispose();
            workingDirectory.Dispose();
        }

        [Test]
        public void GivenAMod_WhenCreatingTheMod_ThenTheDirectoryStructureIsPrepared()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/setup/provinces"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/setup/countries"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/setup/main"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/common/defines"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/events"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/localization/english"), Is.True);
        }

        [Test]
        public void GivenAMod_WhenCreatingTheMod_ThenTheDescriptorAndModFilesAreWritten()
        {
            SetupEmptyWorld();
            string expectedContent = $"{Environment.NewLine}name = \"Shattered World\"{Environment.NewLine}path = \"mod/shattered-world\"{Environment.NewLine}supported_version = \"2.0.*\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world.mod"), Is.True);
            Assert.That(workingDirectory.ReadFile("out/shattered-world.mod"), Is.EqualTo(expectedContent));
            Assert.That(workingDirectory.FileExists("out/shattered-world/descriptor.mod"), Is.True);
            Assert.That(workingDirectory.ReadFile("out/shattered-world/descriptor.mod"), Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAnExistingModDirectory_WhenCreatingTheMod_ThenThePreviousOutputIsDiscarded()
        {
            SetupEmptyWorld();
            workingDirectory.WriteFile("out/shattered-world/setup/stale.txt", "stale");
            workingDirectory.WriteFile("out/shattered-world.mod", "stale");

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world/setup/stale.txt"), Is.False);
            Assert.That(workingDirectory.ReadFile("out/shattered-world.mod"), Does.Not.Contain("stale"));
        }

        [Test]
        public void GivenCities_WhenCreatingTheMod_ThenTheProvinceFileContainsEveryRandomisedCity()
        {
            City city = BuildCity("1", "Newport");
            entityManager.Setup(manager => manager.GetCities()).Returns([city]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/provinces/02_shattered_world.txt");

            Assert.That(content, Does.Contain("1="));
            Assert.That(content, Does.Contain("culture=\"roman\""));
            Assert.That(content, Does.Contain("religion=\"pagan\""));
            Assert.That(content, Does.Contain("trade_goods=\"grain\""));
            Assert.That(content, Does.Contain("province_rank=\"settlement\""));
            Assert.That(content, Does.Contain("civilization_value=8"));
            Assert.That(content, Does.Contain("barbarian_power=16"));
        }

        [Test]
        public void GivenACityWithNonRandomisedLines_WhenCreatingTheMod_ThenThePreservedLinesAreWritten()
        {
            City city = BuildCity("1", "Newport");
            city.PreservedLines = ["\tterrain=\"farmland\""];
            entityManager.Setup(manager => manager.GetCities()).Returns([city]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/setup/provinces/02_shattered_world.txt"),
                Does.Contain("\tterrain=\"farmland\""));
        }

        [Test]
        public void GivenACityWithPopulations_WhenCreatingTheMod_ThenThePopulationBlocksAreWritten()
        {
            City city = BuildCity("1", "Newport");
            entityManager.Setup(manager => manager.GetCities()).Returns([city]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/provinces/02_shattered_world.txt");

            Assert.That(content, Does.Contain("\tcitizen="));
            Assert.That(content, Does.Contain("\t\tamount=4"));
            Assert.That(content, Does.Contain("\tfreemen="));
            Assert.That(content, Does.Contain("\tslaves="));
            Assert.That(content, Does.Contain("\ttribesmen="));
        }

        [Test]
        public void GivenNoCities_WhenCreatingTheMod_ThenTheProvinceFileContainsOnlyTheHeader()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/provinces/02_shattered_world.txt");

            Assert.That(content.TrimStart(), Does.StartWith("# VANILLA PROVINCES - RANDOMISED"));
            Assert.That(content, Does.Not.Contain("="));
        }

        [Test]
        public void GivenAGeneratedCountry_WhenCreatingTheMod_ThenACountryFileIsWrittenForIt()
        {
            Country country = BuildCountry("COR", "Cornova");
            country.IsVanilla = false;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);
            string expectedContent =
                $"color = rgb {{ 4 8 16 }}{Environment.NewLine}" +
                Environment.NewLine +
                $"ship_names = {{{Environment.NewLine}" +
                $"  Boaty McBoatface{Environment.NewLine}" +
                $" }}{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world/setup/countries/COR.txt"), Is.True);
            Assert.That(workingDirectory.ReadFile("out/shattered-world/setup/countries/COR.txt"), Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAVanillaCountry_WhenCreatingTheMod_ThenNoCountryFileIsWrittenForIt()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = true;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world/setup/countries/ROM.txt"), Is.False);
        }

        [Test]
        public void GivenCountries_WhenCreatingTheMod_ThenTheDefinitionIndexListsTheDefaultEntriesAndEveryCountry()
        {
            Country vanillaCountry = BuildCountry("ROM", "Rome");
            vanillaCountry.IsVanilla = true;
            vanillaCountry.DefinitionFilePath = "setup/countries/italia/rome.txt";
            Country generatedCountry = BuildCountry("COR", "Cornova");
            generatedCountry.IsVanilla = false;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([vanillaCountry, generatedCountry]);
            string expectedContent =
                $"REB = \"setup/countries/rebels.txt\"{Environment.NewLine}" +
                $"PIR = \"setup/countries/pirates.txt\"{Environment.NewLine}" +
                $"BAR = \"setup/countries/barbarians.txt\"{Environment.NewLine}" +
                $"MER = \"setup/countries/mercenaries.txt\"{Environment.NewLine}" +
                $"ROM = \"setup/countries/italia/rome.txt\"{Environment.NewLine}" +
                $"COR = \"setup/countries/COR.txt\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.ReadFile("out/shattered-world/setup/countries/countries.txt"), Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAGeneratedCountry_WhenCreatingTheMod_ThenItIsAddedToTheLocalisationFile()
        {
            Country country = BuildCountry("COR", "Cornova");
            country.IsVanilla = false;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);
            string expectedContent = $"l_english:{Environment.NewLine} COR:0 \"Cornova\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.ReadFile("out/shattered-world/localization/english/SW_countries_l_english.yml"), Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAVanillaCountry_WhenCreatingTheMod_ThenItIsNotAddedToTheLocalisationFile()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = true;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/localization/english/SW_countries_l_english.yml"),
                Is.EqualTo($"l_english:{Environment.NewLine}"));
        }

        [Test]
        public void GivenAGeneratedCountry_WhenCreatingTheMod_ThenTheSetupContainsItsDefinition()
        {
            Country country = BuildCountry("COR", "Cornova");
            country.IsVanilla = false;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt");

            Assert.That(content, Does.Contain($"\t\tCOR = {{ # Cornova{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\tgovernment = aristocratic_republic{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\tdiplomatic_stance=neutral_stance{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\tprimary_culture = latin{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\treligion = roman_pantheon{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\tcentralization=42{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\tcapital = 1{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"\t\t\town_control_core = {{ 1 }}{Environment.NewLine}"));
        }

        [Test]
        public void GivenAVanillaCountry_WhenCreatingTheMod_ThenACompatibilityNoticeIsWritten()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = true;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt"),
                Does.Contain("VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY"));
        }

        [Test]
        public void GivenAVanillaCountryWithPreservedLines_WhenCreatingTheMod_ThenTheyAreWritten()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = true;
            country.PreservedLines = ["\t\t\tfamily = 0", "\t\t\tis_antagonist = yes"];
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt");

            Assert.That(content, Does.Contain("\t\t\tfamily = 0"));
            Assert.That(content, Does.Contain("\t\t\tis_antagonist = yes"));
        }

        [TestCase(-1)]
        [TestCase(-42)]
        public void GivenACountryWithoutACentralisationLevel_WhenCreatingTheMod_ThenTheCentralisationIsOmitted(int centralisationLevel)
        {
            Country country = BuildCountry("COR", "Cornova");
            country.IsVanilla = false;
            country.CentralisationLevel = centralisationLevel;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt"),
                Does.Not.Contain("centralization="));
        }

        [TestCase(0)]
        [TestCase(42)]
        public void GivenACountryWithACentralisationLevel_WhenCreatingTheMod_ThenTheCentralisationIsWritten(int centralisationLevel)
        {
            Country country = BuildCountry("COR", "Cornova");
            country.IsVanilla = false;
            country.CentralisationLevel = centralisationLevel;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt"),
                Does.Contain($"\t\t\tcentralization={centralisationLevel}{Environment.NewLine}"));
        }

        [Test]
        public void GivenNoCountries_WhenCreatingTheMod_ThenTheSetupStillContainsTheClosingBlocks()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/setup/main/02_shattered_world_countries.txt");

            Assert.That(content, Does.Contain("country = {"));
            Assert.That(content, Does.Contain("\tcountries = {"));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenTheEventsFileIsWritten()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/events/flavor_sel.txt");

            Assert.That(content, Does.Contain($"namespace = grinsel{Environment.NewLine}"));
            Assert.That(content, Does.Contain("create_pop = slaves"));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenFiveSlavePopsAreCreatedByTheEvent()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/events/flavor_sel.txt");

            Assert.That(content.Split("create_pop = slaves"), Has.Length.EqualTo(6));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenTheDefinesFileIsWritten()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/common/defines/00_defines.txt");

            Assert.That(content, Does.Contain("NGame = {"));
            Assert.That(content, Does.Contain("START_DATE = \"450.10.1\""));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenEveryUnicodeFileStartsWithAByteOrderMark()
        {
            SetupEmptyWorld();

            modWriter.CreateMod();

            Assert.That(HasByteOrderMark("out/shattered-world/setup/provinces/02_shattered_world.txt"), Is.True);
            Assert.That(HasByteOrderMark("out/shattered-world/setup/countries/countries.txt"), Is.True);
            Assert.That(HasByteOrderMark("out/shattered-world/setup/main/02_shattered_world_countries.txt"), Is.True);
            Assert.That(HasByteOrderMark("out/shattered-world/events/flavor_sel.txt"), Is.True);
            Assert.That(HasByteOrderMark("out/shattered-world/localization/english/SW_countries_l_english.yml"), Is.True);
        }

        private void SetupEmptyWorld()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
        }

        private bool HasByteOrderMark(string relativePath)
        {
            byte[] bytes = File.ReadAllBytes(workingDirectory.GetPath(relativePath));

            return
                bytes.Length >= 3 &&
                bytes[0] == 0xEF &&
                bytes[1] == 0xBB &&
                bytes[2] == 0xBF;
        }

        private static Country BuildCountry(string id, string name)
            => new()
            {
                Id = id,
                Name = name,
                ColourRed = 4,
                ColourGreen = 8,
                ColourBlue = 16,
                CultureId = "latin",
                ReligionId = "roman_pantheon",
                GovernmentId = "aristocratic_republic",
                DiplomaticStanceId = "neutral_stance",
                CapitalId = "1",
                CentralisationLevel = 42,
                ShipNames = ["Boaty", "McBoatface"]
            };

        private static City BuildCity(string id, string nameId)
            => new()
            {
                Id = id,
                CultureId = "roman",
                ReligionId = "pagan",
                TradeGoodId = "grain",
                CitizensCount = 4,
                FreemenCount = 8,
                TribesmenCount = 16,
                SlavesCount = 32,
                CivilizationLevel = 8,
                BarbarianLevel = 16,
                NameId = nameId,
                ProvinceId = id,
                ProvinceRank = "settlement"
            };
    }
}