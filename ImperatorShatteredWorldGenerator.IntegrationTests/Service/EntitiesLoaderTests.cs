using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.IntegrationTests.Service
{
    [TestFixture]
    public sealed class EntitiesLoaderTests
    {
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
        public void GivenAProvinceSetupFile_WhenLoadingCities_ThenEveryHabitableCityIsMappedToAServiceModel()
        {
            WriteProvinceDefinitions((1, "Newport"));
            gameDirectory.WriteFile("game/setup/provinces/00_latin_region.txt", BuildProvinceBlock("1", "roman", "settlement"));

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));

            City city = cities[0];

            Assert.That(city.Id, Is.EqualTo("1"));
            Assert.That(city.CultureId, Is.EqualTo("roman"));
            Assert.That(city.ReligionId, Is.EqualTo("roman_pantheon"));
            Assert.That(city.TradeGoodId, Is.EqualTo("grain"));
            Assert.That(city.CitizensCount, Is.EqualTo(4));
            Assert.That(city.CivilizationLevel, Is.EqualTo(8));
            Assert.That(city.BarbarianLevel, Is.EqualTo(0));
            Assert.That(city.NameId, Is.EqualTo("Newport"));
            Assert.That(city.ProvinceId, Is.EqualTo("1"));
            Assert.That(city.ProvinceRank, Is.EqualTo("settlement"));
            Assert.That(city.IsHabitable, Is.True);
        }

        [Test]
        public void GivenAProvinceWithFractionalCivilizationValue_WhenLoadingCities_ThenTheValueIsTruncated()
        {
            WriteProvinceDefinitions((1, "Newport"));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                "1=\n{\n\tcivilization_value=18.5\n\tculture=\"roman\"\n\tprovince_rank=\"city\"\n\treligion=\"roman_pantheon\"\n\ttrade_goods=\"grain\"\n}\n");

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities[0].CivilizationLevel, Is.EqualTo(18));
        }

        [Test]
        public void GivenAProvinceWithSeveralPopBlocks_WhenLoadingCities_ThenPopulationsOfTheSameTypeAreSummed()
        {
            WriteProvinceDefinitions((1, "Newport"));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                "1=\n{\n\tcivilization_value=1\n\tculture=\"roman\"\n\tprovince_rank=\"city\"\n\treligion=\"roman_pantheon\"\n\ttrade_goods=\"grain\"\n" +
                "\tcitizen=\n\t{\n\t\tamount=4\n\t}\n" +
                "\tcitizen=\n\t{\n\t\tculture=\"hebrew\"\n\t\tamount=20\n\t}\n}\n");

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities[0].CitizensCount, Is.EqualTo(24));
        }

        [Test]
        public void GivenAProvinceFileWithSeveralProvinces_WhenLoadingCities_ThenEveryHabitableProvinceIsLoaded()
        {
            WriteProvinceDefinitions((1, "Newport"), (2, "Cornova"), (3, "Solara"));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                BuildProvinceBlock("1", "roman", "city") +
                BuildProvinceBlock("2", "greek", "settlement") +
                BuildProvinceBlock("3", "latin", "settlement"));

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(3));
            Assert.That(cities.Select(city => city.Id), Is.EquivalentTo(["1", "2", "3"]));
            Assert.That(cities.Single(city => city.Id == "2").NameId, Is.EqualTo("Cornova"));
            Assert.That(cities.Single(city => city.Id == "3").NameId, Is.EqualTo("Solara"));
        }

        [Test]
        public void GivenAProvinceWithoutAProvinceRank_WhenLoadingCities_ThenItIsNotLoaded()
        {
            WriteProvinceDefinitions((1, "Newport"), (2, "Bargylus Mons"));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                BuildProvinceBlock("1", "roman", "settlement") +
                "2=\n{\n\tcivilization_value=0\n\tculture=\"roman\"\n\tprovince_rank=\"\"\n\treligion=\"roman_pantheon\"\n\ttrade_goods=\"\"\n}\n");

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));
            Assert.That(cities[0].Id, Is.EqualTo("1"));
        }

        [Test]
        public void GivenAProvinceWithoutADefinitionName_WhenLoadingCities_ThenItIsNotLoaded()
        {
            WriteProvinceDefinitions((1, "Newport"), (2, null));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                BuildProvinceBlock("1", "roman", "settlement") +
                BuildProvinceBlock("2", "roman", "settlement"));

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));
            Assert.That(cities[0].Id, Is.EqualTo("1"));
        }

        [Test]
        public void GivenAProvinceWithNonRandomisedAttributes_WhenLoadingCities_ThenTheyArePreserved()
        {
            WriteProvinceDefinitions((1, "Newport"));
            gameDirectory.WriteFile(
                "game/setup/provinces/00_latin_region.txt",
                "1=\n{\n\tcivilization_value=1\n\tculture=\"roman\"\n\tholy_site=\"omen_jupiter\"\n\tport_building=2\n\tprovince_rank=\"city\"\n\treligion=\"roman_pantheon\"\n\tterrain=\"farmland\"\n\ttrade_goods=\"grain\"\n}\n");

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities[0].PreservedLines, Does.Contain("\tholy_site=\"omen_jupiter\""));
            Assert.That(cities[0].PreservedLines, Does.Contain("\tport_building=2"));
            Assert.That(cities[0].PreservedLines, Does.Contain("\tterrain=\"farmland\""));
            Assert.That(cities[0].PreservedLines, Does.Not.Contain("\tculture=\"roman\""));
        }

        [Test]
        public void GivenNoProvincesDirectory_WhenLoadingCities_ThenNothingIsLoaded()
        {
            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Is.Empty);
        }

        [Test]
        public void GivenNoCulturesDirectory_WhenLoadingCultureIds_ThenTheLoadingFails()
            => Assert.That(
                () => entitiesLoader.LoadCultureIds(),
                Throws.TypeOf<DirectoryNotFoundException>());

        [Test]
        public void GivenACulturesDirectory_WhenLoadingCultureIds_ThenOnlySubculturesAreLoaded()
        {
            gameDirectory.WriteFile(
                "game/common/cultures/00_latin.txt",
                "latin = {\n\tmale_names = {\n\t\tAppius\n\t}\n\tculture = {\n\t\troman = { levy_template = levy_roman }\n\t\tvenetic = {}\n\t\tetruscan = {\n\t\t\tlevy_template = levy_etruscan\n\t\t}\n\t}\n}\n");

            List<string> cultureIds = [.. entitiesLoader.LoadCultureIds()];

            Assert.That(cultureIds, Is.EqualTo(["roman", "venetic", "etruscan"]));
        }

        [Test]
        public void GivenSeveralCultureFiles_WhenLoadingCultureIds_ThenEveryFileIsRead()
        {
            gameDirectory.WriteFile("game/common/cultures/00_latin.txt", "latin = {\n\tculture = {\n\t\tlatin = {}\n\t}\n}\n");
            gameDirectory.WriteFile("game/common/cultures/00_greek.txt", "greek = {\n\tculture = {\n\t\tgreek = {}\n\t}\n}\n");

            List<string> cultureIds = [.. entitiesLoader.LoadCultureIds()];

            Assert.That(cultureIds, Is.EquivalentTo(["latin", "greek"]));
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
                "#Republics\naristocratic_republic = { #military party\n\t\ttype = republic\n}\nmonarchy = {\n}\n");

            List<string> governmentIds = [.. entitiesLoader.LoadGovernmentIds()];

            Assert.That(governmentIds, Is.EqualTo(["aristocratic_republic", "monarchy"]));
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
                "neutral_stance = {\n\t\tmodifier = {\n\t\t}\n}\nappeasement_stance = {\n}\n");

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
                "roman_pantheon = { # Hellenic\n\t\tmodifier = { global_citizen_happyness = happiness_large_svalue }\n}\negyptian_pantheon = {\n}\n");

            List<string> religionIds = [.. entitiesLoader.LoadReligionIds()];

            Assert.That(religionIds, Is.EqualTo(["roman_pantheon", "egyptian_pantheon"]));
        }

        [Test]
        public void GivenNoCountriesIndexFile_WhenLoadingCountries_ThenTheLoadingFails()
        {
            gameDirectory.CreateDirectory("game/setup/countries");

            Assert.That(
                () => entitiesLoader.LoadCountries(),
                Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void GivenASetupFileWithoutCountryBlocks_WhenLoadingCountries_ThenNothingIsLoaded()
        {
            gameDirectory.WriteFile("game/setup/countries/countries.txt", "REB = \"setup/countries/rebels.txt\"\n");
            gameDirectory.WriteFile("game/setup/main/00_default.txt", "country = {\n\tcountries = {\n\t}\n}\n");

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
            Assert.That(country.DefinitionFilePath, Is.EqualTo("setup/countries/italia/rome.txt"));
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
        public void GivenAVanillaCountryWithACentralisationValue_WhenLoadingCountries_ThenTheCentralisationLevelIsLoaded()
        {
            WriteVanillaCountryFiles();

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].CentralisationLevel, Is.EqualTo(42));
        }

        [Test]
        public void GivenAVanillaCountryWithAnInlineCommentOnTheCapital_WhenLoadingCountries_ThenTheCommentIsNotPartOfTheCapitalId()
        {
            WriteVanillaCountryFiles();
            gameDirectory.WriteFile(
                "game/setup/main/00_default.txt",
                "country = {\n" +
                "\tcountries = {\n" +
                "\t\tROM = {\n" +
                "\t\t\tgovernment = aristocratic_republic\n" +
                "\t\t\tcapital = 1 #Roma\n" +
                "\t\t}\n" +
                "\t}\n" +
                "}\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].CapitalId, Is.EqualTo("1"));
        }

        [Test]
        public void GivenAVanillaCountry_WhenLoadingCountries_ThenNonManagedBlockLinesArePreserved()
        {
            WriteVanillaCountryFiles();

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries[0].PreservedLines, Does.Contain("\t\t\tfamily = 0"));
            Assert.That(countries[0].PreservedLines, Does.Contain("\t\t\tis_antagonist = yes"));
            Assert.That(countries[0].PreservedLines, Does.Not.Contain("\t\t\town_control_core = { 1 2 3 }"));
        }

        [Test]
        public void GivenAVanillaCountryWithoutACapital_WhenLoadingCountries_ThenItIsLoadedWithoutACapital()
        {
            gameDirectory.WriteFile("game/setup/countries/countries.txt", "ROM = \"setup/countries/italia/rome.txt\"\n");
            gameDirectory.WriteFile("game/setup/countries/italia/rome.txt", "color = rgb { 143  141  128 }\n");
            gameDirectory.WriteFile(
                "game/setup/main/00_default.txt",
                "country = {\n\tcountries = {\n\t\tROM = {\n\t\t\tgovernment = aristocratic_republic\n\t\t}\n\t}\n}\n");

            List<Country> countries = [.. entitiesLoader.LoadCountries()];

            Assert.That(countries, Has.Count.EqualTo(1));
            Assert.That(countries[0].Id, Is.EqualTo("ROM"));
            Assert.That(countries[0].CapitalId, Is.Empty);
        }

        [Test]
        public void GivenAProvinceDefinitionFile_WhenLoadingCities_ThenTheProvinceNamesAreReadFromTheCsv()
        {
            gameDirectory.WriteFile(
                "game/map_data/definition.csv",
                "#Province id 0 is ignored, hard coded.\n0;0;0;0;;x;;;;;;;;;;;;;;;;;;;,\n1;42;3;128;Roma;x;;;;;;;;;;;;;;;;;;;,\n");
            gameDirectory.WriteFile("game/setup/provinces/00_latin_region.txt", BuildProvinceBlock("1", "roman", "city"));

            List<City> cities = [.. entitiesLoader.LoadCities()];

            Assert.That(cities, Has.Count.EqualTo(1));
            Assert.That(cities[0].NameId, Is.EqualTo("Roma"));
        }

        private void WriteProvinceDefinitions(params (int Id, string Name)[] provinces)
        {
            string content = "#Province id 0 is ignored, hard coded.\n0;0;0;0;;x;;;;;;;;;;;;;;;;;;;,\n";

            foreach ((int id, string name) in provinces)
            {
                content += $"{id};42;3;128;{name};x;;;;;;;;;;;;;;;;;;;,\n";
            }

            gameDirectory.WriteFile("game/map_data/definition.csv", content);
        }

        private static string BuildProvinceBlock(string provinceId, string cultureId, string provinceRank)
            => $"{provinceId}=\n" +
               "{\n" +
               "\tbarbarian_power=0\n" +
               "\tcivilization_value=8\n" +
               $"\tculture=\"{cultureId}\"\n" +
               $"\tprovince_rank=\"{provinceRank}\"\n" +
               "\treligion=\"roman_pantheon\"\n" +
               "\ttrade_goods=\"grain\"\n" +
               "\tcitizen=\n" +
               "\t{\n" +
               "\t\tamount=4\n" +
               "\t}\n" +
               "}\n";

        private void WriteVanillaCountryFiles()
        {
            gameDirectory.WriteFile("game/setup/countries/countries.txt", "ROM = \"setup/countries/italia/rome.txt\"\n");
            gameDirectory.WriteFile("game/setup/countries/italia/rome.txt", "color = rgb { 143  141  128 }\n");
            gameDirectory.WriteFile("game/localization/english/countries_l_english.yml", "l_english:\n ROM:0 \"Rome\"\n");
            gameDirectory.WriteFile(
                "game/setup/main/00_default.txt",
                "country = {\n" +
                "\tcountries = {\n" +
                "\t\tROM = {\n" +
                "\t\t\tfamily = 0\n" +
                "\t\t\tgovernment = aristocratic_republic\n" +
                "\t\t\tdiplomatic_stance = neutral_stance\n" +
                "\t\t\tprimary_culture = latin\n" +
                "\t\t\treligion = roman_pantheon\n" +
                "\t\t\tcentralization = 42\n" +
                "\t\t\tcapital = 1\n" +
                "\t\t\tis_antagonist = yes\n" +
                "\t\t\town_control_core = { 1 2 3 }\n" +
                "\t\t}\n" +
                "\t}\n" +
                "}\n");
        }
    }
}