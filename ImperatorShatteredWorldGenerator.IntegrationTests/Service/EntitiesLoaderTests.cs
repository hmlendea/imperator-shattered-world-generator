using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.IntegrationTests.Service
{
    [TestFixture]
    public sealed class EntitiesLoaderTests
    {
        private const string CityHeader =
            "1,roman,pagan,grain,4,8,16,32,8,16,Newport,1\n";

        private TemporaryDirectory gameDirectory = null!;
        private EntitiesLoader entitiesLoader = null!;

        [SetUp]
        public void SetUp()
        {
            gameDirectory = new();
            entitiesLoader = new(new GeneratorSettings { GameDirectoryPath = gameDirectory.DirectoryPath });
        }

        [TearDown]
        public void TearDown() => gameDirectory.Dispose();

        [Test]
        public void GivenAProvinceSetupFile_WhenLoadingCities_ThenEveryCityIsMappedToAServiceModel()
        {
            gameDirectory.WriteFile("game/common/province_setup.csv", CityHeader);

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));

            City city = cities[0];

            Assert.That(city.Id, Is.EqualTo("1"));
            Assert.That(city.CultureId, Is.EqualTo("roman"));
            Assert.That(city.ReligionId, Is.EqualTo("pagan"));
            Assert.That(city.TradeGoodId, Is.EqualTo("grain"));
            Assert.That(city.CitizensCount, Is.EqualTo(4));
            Assert.That(city.FreemenCount, Is.EqualTo(8));
            Assert.That(city.TribesmenCount, Is.EqualTo(16));
            Assert.That(city.SlavesCount, Is.EqualTo(32));
            Assert.That(city.CivilizationLevel, Is.EqualTo(8));
            Assert.That(city.BarbarianLevel, Is.EqualTo(16));
            Assert.That(city.NameId, Is.EqualTo("Newport"));
            Assert.That(city.ProvinceId, Is.EqualTo("1"));
            Assert.That(city.TotalPopulation, Is.EqualTo(60));
            Assert.That(city.IsHabitable, Is.True);
        }

        [Test]
        public void GivenAProvinceSetupFileWithSeveralCities_WhenLoadingCities_ThenEveryCityIsLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/province_setup.csv",
                CityHeader +
                "2,greek,hellenic,wine,4,8,16,32,8,16,Cornova,2\n" +
                "3,latin,roman_pantheon,olives,4,8,16,32,8,16,Solara,3\n");

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(3));
            Assert.That(cities.Select(city => city.Id), Is.EquivalentTo(["1", "2", "3"]));
            Assert.That(cities.Single(city => city.Id == "2").NameId, Is.EqualTo("Cornova"));
            Assert.That(cities.Single(city => city.Id == "3").NameId, Is.EqualTo("Solara"));
        }

        [Test]
        public void GivenAProvinceSetupFileWithCommentedLines_WhenLoadingCities_ThenTheCommentsAreSkipped()
        {
            gameDirectory.WriteFile(
                "game/common/province_setup.csv",
                "#ProvID;Culture;Religion;TradeGoods;Citizens;Freedmen;Slaves;Tribesmen;Civilization;Barbarian;NameRef;AraRef\n" +
                CityHeader);

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));
            Assert.That(cities[0].Id, Is.EqualTo("1"));
        }

        [Test]
        public void GivenNoProvinceSetupFile_WhenLoadingCities_ThenNothingIsLoaded()
        {
            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Is.Empty);
        }

        [Test]
        public void GivenAMalformedProvinceSetupFile_WhenLoadingCities_ThenTheLoadingFails()
        {
            gameDirectory.WriteFile("game/common/province_setup.csv", "1,roman\n");

            Assert.That(
                () => entitiesLoader.LoadCities(),
                Throws.TypeOf<SerializationException>());
        }

        [Test]
        public void GivenNoCulturesDirectory_WhenLoadingCultureIds_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadCultureIds(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenACulturesDirectory_WhenLoadingCultureIds_ThenOnlyEmptyCultureDefinitionsAreLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/cultures/00_latin.txt",
                "latin = {\n" +
                "\t\tmunicipal = {}\n" +
                "\t\tbritannic = {} #deprecated\n" +
                "\tnon_empty = { color = hsv { 0 0.91 0.55 } }\n" +
                "}\n");

            List<string> cultureIds = [.. entitiesLoader.LoadCultureIds()];

            Assert.That(cultureIds, Is.EqualTo(["municipal", "britannic"]));
        }

        [Test]
        public void GivenSeveralCultureFiles_WhenLoadingCultureIds_ThenEveryFileIsRead()
        {
            gameDirectory.WriteFile("game/common/cultures/00_latin.txt", "\t\tlatin = {}\n");
            gameDirectory.WriteFile("game/common/cultures/00_greek.txt", "\t\tgreek = {}\n");

            List<string> cultureIds = [.. entitiesLoader.LoadCultureIds()];

            Assert.That(cultureIds, Is.EquivalentTo(["latin", "greek"]));
        }

        [Test]
        public void GivenACulturesDirectoryWithNonTextFiles_WhenLoadingCultureIds_ThenOnlyTextFilesAreRead()
        {
            gameDirectory.WriteFile("game/common/cultures/00_latin.txt", "\t\tlatin = {}\n");
            gameDirectory.WriteFile("game/common/cultures/notes.md", "\t\tignored = {}\n");

            List<string> cultureIds = [.. entitiesLoader.LoadCultureIds()];

            Assert.That(cultureIds, Is.EqualTo(["latin"]));
        }

        [Test]
        public void GivenNoGovernmentDirectory_WhenLoadingGovernmentIds_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadGovernmentIds(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenAGovernmentFile_WhenLoadingGovernmentIds_ThenOnlyBlockIdentifiersAreLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/governments/00_default.txt",
                "#Republics\n" +
                "aristocratic_republic = { #military party\n" +
                "\t\ttype = republic\n" +
                "}\n" +
                "monarchy = {\n" +
                "}\n");

            List<string> governmentIds = [.. entitiesLoader.LoadGovernmentIds()];

            Assert.That(governmentIds, Is.EqualTo(["aristocratic_republic", "monarchy"]));
        }

        [Test]
        public void GivenSeveralGovernmentFiles_WhenLoadingGovernmentIds_ThenEveryFileIsRead()
        {
            gameDirectory.WriteFile("game/common/governments/00_default.txt", "republic = {\n}\n");
            gameDirectory.WriteFile("game/common/governments/01_extra.txt", "tribal = {\n}\n");

            List<string> governmentIds = [.. entitiesLoader.LoadGovernmentIds()];

            Assert.That(governmentIds, Is.EquivalentTo(["republic", "tribal"]));
        }

        [Test]
        public void GivenNoDiplomaticStanceDirectory_WhenLoadingDiplomaticStanceIds_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadDiplomaticStanceIds(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenADiplomaticStanceFile_WhenLoadingDiplomaticStanceIds_ThenOnlyBlockIdentifiersAreLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/diplomatic_stances/00_default.txt",
                "neutral_stance = {\n" +
                "\t\tmodifier = {\n" +
                "\t\t}\n" +
                "}\n" +
                "appeasement_stance = {\n" +
                "}\n");

            List<string> diplomaticStanceIds = [.. entitiesLoader.LoadDiplomaticStanceIds()];

            Assert.That(diplomaticStanceIds, Is.EqualTo(["neutral_stance", "appeasement_stance"]));
        }

        [Test]
        public void GivenNoReligionFile_WhenLoadingReligionIds_ThenTheLoadingFails()
        {
            gameDirectory.CreateDirectory("game/common/religions");

            Assert.That(
                () => entitiesLoader.LoadReligionIds(),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenAReligionFile_WhenLoadingReligionIds_ThenOnlyBlockIdentifiersAreLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/religions/00_default.txt",
                "roman_pantheon = { # Hellenic\n" +
                "\t\tmodifier = { global_citizen_happyness = happiness_large_svalue }\n" +
                "}\n" +
                "egyptian_pantheon = {\n" +
                "}\n");

            List<string> religionIds = [.. entitiesLoader.LoadReligionIds()];

            Assert.That(religionIds, Is.EqualTo(["roman_pantheon", "egyptian_pantheon"]));
        }

        [Test]
        public void GivenNoSetupFile_WhenLoadingCountries_ThenTheLoadingFails()
        {
            gameDirectory.CreateDirectory("game/common");

            Assert.That(
                () => entitiesLoader.LoadCountries(),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenNoGameDirectory_WhenLoadingCountries_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadCountries(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenNoGameDirectory_WhenLoadingReligionIds_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadReligionIds(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenASetupFileWithoutCapitalDefinitions_WhenLoadingCountries_ThenNothingIsLoaded()
        {
            gameDirectory.WriteFile("game/common/setup.txt", "countries = {\n}\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries, Is.Empty);
        }

        [Test]
        public void GivenAVanillaCountry_WhenLoadingCountries_ThenTheDefinitionAndLocalisedNameAreLoaded()
        {
            WriteVanillaCountryFiles();

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries, Has.Count.EqualTo(1));

            Country country = countries[0];

            Assert.That(country.Id, Is.EqualTo("ROM"));
            Assert.That(country.Name, Is.EqualTo("Rome"));
            Assert.That(country.GovernmentId, Is.EqualTo("aristocratic_republic"));
            Assert.That(country.DiplomaticStanceId, Is.EqualTo("neutral_stance"));
            Assert.That(country.CultureId, Is.EqualTo("latin"));
            Assert.That(country.ReligionId, Is.EqualTo("roman_pantheon"));
            Assert.That(country.CapitalId, Is.EqualTo("1"));
            Assert.That(country.IsVanilla, Is.True);
        }

        [Test]
        public void GivenAVanillaCountry_WhenLoadingCountries_ThenTheColourIsReadFromTheCountryFile()
        {
            WriteVanillaCountryFiles();

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].ColourRed, Is.EqualTo(143));
            Assert.That(countries[0].ColourGreen, Is.EqualTo(141));
            Assert.That(countries[0].ColourBlue, Is.EqualTo(128));
        }

        [Test]
        public void GivenAVanillaCountryWithACentralisationValue_WhenLoadingCountries_ThenTheCentralisationLevelIsLeftUnset()
        {
            WriteVanillaCountryFiles();

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].CentralisationLevel, Is.EqualTo(-1));
        }

        [Test]
        public void GivenSeveralVanillaCountries_WhenLoadingCountries_ThenEveryCountryIsLoaded()
        {
            WriteVanillaCountryFiles();
            gameDirectory.WriteFile(
                "game/common/setup.txt",
                BuildSetupFile(
                    ("ROM", "aristocratic_republic", "neutral_stance", "latin", "roman_pantheon", "1"),
                    ("CAR", "monarchy", "appeasement_stance", "greek", "egyptian_pantheon", "2")));
            gameDirectory.WriteFile(
                "game/localization/english/countries_l_english.yml",
                "l_english:\n ROM:0 \"Rome\"\n CAR:0 \"Carthage\"\n");
            gameDirectory.WriteFile(
                "game/common/countries.txt",
                "ROM = \"countries/italia/rome.txt\"\n" +
                "CAR = \"countries/africa/carthage.txt\"\n");
            gameDirectory.WriteFile("game/common/countries/italia/rome.txt", "color = rgb { 143  141  128 }\n");
            gameDirectory.WriteFile("game/common/countries/africa/carthage.txt", "color = rgb { 4  8  16 }\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries, Has.Count.EqualTo(2));
            Assert.That(countries[0].Id, Is.EqualTo("ROM"));
            Assert.That(countries[0].Name, Is.EqualTo("Rome"));
            Assert.That(countries[0].ColourRed, Is.EqualTo(143));
            Assert.That(countries[1].Id, Is.EqualTo("CAR"));
            Assert.That(countries[1].Name, Is.EqualTo("Carthage"));
            Assert.That(countries[1].ColourBlue, Is.EqualTo(16));
        }

        [Test]
        public void GivenAVanillaCountryWithoutALocalisedName_WhenLoadingCountries_ThenTheNameIsNotSet()
        {
            WriteVanillaCountryFiles();
            gameDirectory.WriteFile("game/localization/english/countries_l_english.yml", "l_english:\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].Name, Is.Null);
        }

        [Test]
        public void GivenAVanillaCountryMissingFromTheIndex_WhenLoadingCountries_ThenTheColourIsNotSet()
        {
            WriteVanillaCountryFiles();
            gameDirectory.WriteFile("game/common/countries.txt", "CAR = \"countries/africa/carthage.txt\"\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].ColourRed, Is.EqualTo(0));
            Assert.That(countries[0].ColourGreen, Is.EqualTo(0));
            Assert.That(countries[0].ColourBlue, Is.EqualTo(0));
        }

        private void WriteVanillaCountryFiles()
        {
            gameDirectory.WriteFile(
                "game/common/setup.txt",
                BuildSetupFile(("ROM", "aristocratic_republic", "neutral_stance", "latin", "roman_pantheon", "1")));
            gameDirectory.WriteFile(
                "game/localization/english/countries_l_english.yml",
                "l_english:\n ROM:0 \"Rome\"\n");
            gameDirectory.WriteFile("game/common/countries.txt", "ROM = \"countries/italia/rome.txt\"\n");
            gameDirectory.WriteFile("game/common/countries/italia/rome.txt", "color = rgb { 143  141  128 }\n");
        }

        private static string BuildSetupFile(
            params (string Id, string GovernmentId, string DiplomaticStanceId, string CultureId, string ReligionId, string CapitalId)[] countries)
        {
            string content = "countries = {\n";

            foreach ((string id, string governmentId, string diplomaticStanceId, string cultureId, string religionId, string capitalId) in countries)
            {
                content +=
                    $"\t\t{id} = {{\n" +
                    $"\t\t\tgovernment = {governmentId}\n" +
                    $"\t\t\tdiplomatic_stance = {diplomaticStanceId}\n" +
                    $"\t\t\tprimary_culture = {cultureId}\n" +
                    $"\t\t\treligion = {religionId}\n" +
                    $"\t\t\tcentralization = 42\n" +
                    $"\t\t\tcapital = {capitalId}\n" +
                    "\t\t}\n";
            }

            return content + "}\n";
        }
    }
}
