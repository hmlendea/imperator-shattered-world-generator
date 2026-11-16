using System;
using System.Collections.Generic;
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
        private static readonly string[] ShipNames = ["Boaty", "McBoatface"];

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
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            Assert.That(workingDirectory.DirectoryExists("out/shattered-world"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/common"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/common/countries"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/common/defines"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/events"), Is.True);
            Assert.That(workingDirectory.DirectoryExists("out/shattered-world/localization"), Is.True);
        }

        [Test]
        public void GivenAMod_WhenCreatingTheMod_ThenTheDescriptorAndModFilesAreWritten()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            string expectedContent = $"{Environment.NewLine}name = \"Shattered World\"{Environment.NewLine}path = \"mod/shattered-world\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world.mod"), Is.True);
            Assert.That(workingDirectory.ReadFile("out/shattered-world.mod"), Is.EqualTo(expectedContent));
            Assert.That(workingDirectory.FileExists("out/shattered-world/descriptor.mod"), Is.True);
            Assert.That(workingDirectory.ReadFile("out/shattered-world/descriptor.mod"), Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAnExistingModDirectory_WhenCreatingTheMod_ThenThePreviousOutputIsDiscarded()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            workingDirectory.WriteFile("out/shattered-world/common/stale.txt", "stale");
            workingDirectory.WriteFile("out/shattered-world.mod", "stale");

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world/common/stale.txt"), Is.False);
            Assert.That(workingDirectory.ReadFile("out/shattered-world.mod"), Does.Not.Contain("stale"));
        }

        [Test]
        public void GivenCities_WhenCreatingTheMod_ThenTheProvinceSetupContainsTheHeaderAndEveryCity()
        {
            City city = BuildCity("1", "Newport");
            entityManager.Setup(manager => manager.GetCities()).Returns([city]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            string expectedContent =
                "#ProvID;Culture;Religion;TradeGoods;Citizens;Freedmen;Slaves;Tribesmen;Civilization;Barbarian;NameRef;AraRef" +
                $"{Environment.NewLine}1,roman,pagan,grain,4,8,16,32,8,16,Newport,1{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/province_setup.csv"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenNoCities_WhenCreatingTheMod_ThenTheProvinceSetupContainsOnlyTheHeader()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            string expectedContent =
                "#ProvID;Culture;Religion;TradeGoods;Citizens;Freedmen;Slaves;Tribesmen;Civilization;Barbarian;NameRef;AraRef" +
                $"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/province_setup.csv"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenCountries_WhenCreatingTheMod_ThenACountryFileIsWrittenForEachOfThem()
        {
            Country country = BuildCountry("ROM", "Rome");
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);
            string expectedContent =
                $"color = rgb {{ 4 8 16 }}{Environment.NewLine}" +
                Environment.NewLine +
                $"ship_names = {{{Environment.NewLine}" +
                $"  Boaty McBoatface{Environment.NewLine}" +
                $" }}{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(workingDirectory.FileExists("out/shattered-world/common/countries/ROM.txt"), Is.True);
            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/countries/ROM.txt"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenCountries_WhenCreatingTheMod_ThenTheDefinitionIndexListsTheDefaultEntriesAndEveryCountry()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([BuildCountry("ROM", "Rome")]);
            string expectedContent =
                $"REB = \"countries/rebels.txt\"{Environment.NewLine}" +
                $"PIR = \"countries/pirates.txt\"{Environment.NewLine}" +
                $"BAR = \"countries/barbarians.txt\"{Environment.NewLine}" +
                $"MER = \"countries/mercenaries.txt\"{Environment.NewLine}" +
                $"ROM = \"countries/ROM.txt\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/countries.txt"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAGeneratedCountry_WhenCreatingTheMod_ThenItIsAddedToTheLocalisationFile()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = false;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);
            string expectedContent =
                $"l_english:{Environment.NewLine}" +
                $" ROM:0 \"Rome\"{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/localization/SW_countries_l_english.yml"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAVanillaCountry_WhenCreatingTheMod_ThenItIsNotAddedToTheLocalisationFile()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.IsVanilla = true;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);
            string expectedContent = $"l_english:{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/localization/SW_countries_l_english.yml"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenAGeneratedCountry_WhenCreatingTheMod_ThenTheSetupContainsItsDefinition()
        {
            Country country = BuildCountry("ROM", "Rome");
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/common/setup.txt");

            Assert.That(content, Does.Contain($"    ROM = {{ # Rome{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      government = aristocratic_republic{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      diplomatic_stance = neutral_stance{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      primary_culture = latin{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      religion = roman_pantheon{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      centralization = 42{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      capital = 1{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"      own_control_core = {{ 1 }}{Environment.NewLine}"));
        }

        [Test]
        public void GivenACountryWithoutADiplomaticStance_WhenCreatingTheMod_ThenTheStanceIsOmitted()
        {
            Country country = BuildCountry("ROM", "Rome");
            country.DiplomaticStanceId = null;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/setup.txt"),
                Does.Not.Contain("diplomatic_stance"));
        }

        [TestCase(-1)]
        [TestCase(-42)]
        public void GivenACountryWithoutACentralisationLevel_WhenCreatingTheMod_ThenTheCentralisationIsOmitted(
            int centralisationLevel)
        {
            Country country = BuildCountry("ROM", "Rome");
            country.CentralisationLevel = centralisationLevel;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/setup.txt"),
                Does.Not.Contain("centralization ="));
        }

        [TestCase(0)]
        [TestCase(42)]
        [TestCase(100)]
        public void GivenACountryWithACentralisationLevel_WhenCreatingTheMod_ThenTheCentralisationIsWritten(
            int centralisationLevel)
        {
            Country country = BuildCountry("ROM", "Rome");
            country.CentralisationLevel = centralisationLevel;
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([country]);

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/setup.txt"),
                Does.Contain($"      centralization = {centralisationLevel}{Environment.NewLine}"));
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
                workingDirectory.ReadFile("out/shattered-world/common/setup.txt"),
                Does.Contain("VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY"));
        }

        [Test]
        public void GivenNoCountries_WhenCreatingTheMod_ThenTheSetupStillContainsTheClosingBlocks()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            string expectedContent =
                $"{Environment.NewLine}" +
                $"country = {{{Environment.NewLine}" +
                $"  countries = {{{Environment.NewLine}" +
                $"  }}{Environment.NewLine}" +
                $"}}{Environment.NewLine}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/setup.txt"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenTheEventsFileIsWritten()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/events/flavor_sel.txt");

            Assert.That(content, Does.Contain($"namespace = grinsel{Environment.NewLine}"));
            Assert.That(content, Does.Contain($"grinsel.2 = {{{Environment.NewLine}"));
            Assert.That(content, Does.Contain("create_pop = slaves"));
            Assert.That(content, Does.Contain("namespace = grinsel"));
        }

        [Test]
        public void GivenAFreshMod_WhenCreatingTheMod_ThenFiveSlavePopsAreCreatedByTheEvent()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            string content = workingDirectory.ReadFile("out/shattered-world/events/flavor_sel.txt");

            Assert.That(content.Split("create_pop = slaves"), Has.Length.EqualTo(6));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenTheDefinesFileIsWritten()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);
            string expectedContent =
                $"NGame = {{{Environment.NewLine}" +
                $"  START_DATE = \"001.10.1\"{Environment.NewLine}" +
                "}";

            modWriter.CreateMod();

            Assert.That(
                workingDirectory.ReadFile("out/shattered-world/common/defines/00_defines.txt"),
                Is.EqualTo(expectedContent));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenEveryWrittenFileEndsWithANewLine()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([BuildCity("1", "Newport")]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([BuildCountry("ROM", "Rome")]);

            modWriter.CreateMod();

            Assert.That(workingDirectory.ReadFile("out/shattered-world/common/setup.txt"), Does.EndWith(Environment.NewLine));
            Assert.That(workingDirectory.ReadFile("out/shattered-world.mod"), Does.EndWith(Environment.NewLine));
            Assert.That(workingDirectory.ReadFile("out/shattered-world/descriptor.mod"), Does.EndWith(Environment.NewLine));
        }

        [Test]
        public void GivenACreatingModRequest_WhenCreatingTheMod_ThenEveryUnicodeFileStartsWithAByteOrderMark()
        {
            entityManager.Setup(manager => manager.GetCities()).Returns([]);
            entityManager.Setup(manager => manager.GetCountries()).Returns([]);

            modWriter.CreateMod();

            Assert.That(
                HasByteOrderMark("out/shattered-world/common/province_setup.csv"),
                Is.True);
            Assert.That(
                HasByteOrderMark("out/shattered-world/events/flavor_sel.txt"),
                Is.True);
            Assert.That(
                HasByteOrderMark("out/shattered-world/common/defines/00_defines.txt"),
                Is.True);
            Assert.That(
                HasByteOrderMark("out/shattered-world/localization/SW_countries_l_english.yml"),
                Is.True);
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

        private Country BuildCountry(string id, string name)
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
                ShipNames = [.. ShipNames]
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
                ProvinceId = id
            };
    }
}
