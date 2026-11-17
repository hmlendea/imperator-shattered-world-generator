using System;
using System.IO;
using System.Linq;
using System.Text;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ModWriter : IModWriter
    {
        const string SupportedGameVersion = "2.0.*";

        const string VanillaCompatibilityComment =
            "\t\t\t# VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY";

        const string VanillaCompatibilityReasonComment =
            "\t\t\t# Removing it could break the game once enough ingame years (even decades) have passed";

        readonly IEntityManager entityManager;
        readonly GeneratorSettings settings;

        public ModWriter(
            IEntityManager entityManager,
            GeneratorSettings settings)
        {
            this.entityManager = entityManager;
            this.settings = settings;
        }

        public void CreateMod()
        {
            PrepareDirectoryStructure();
            WriteModDescriptor();

            WriteProvinces();
            WriteCountryFiles();
            WriteCountriesDefinitionIndexFile();
            WriteCountriesLocalisationFile();
            WriteCountrySetupFile();
            WriteEventsFile();
            WriteDefinesFile();
        }

        void PrepareDirectoryStructure()
        {
            string modPath = settings.ModDirectoryPath;
            string modSetupDir = Path.Combine(modPath, "setup");
            string modEventsDir = Path.Combine(modPath, "events");
            string modDefinesDir = Path.Combine(modPath, "common", "defines");
            string modProvincesDir = Path.Combine(modSetupDir, "provinces");
            string modCountriesDir = Path.Combine(modSetupDir, "countries");
            string modSetupMainDir = Path.Combine(modSetupDir, "main");
            string modLocalisationDir = Path.Combine(modPath, "localization", "english");

            if (Directory.Exists(modPath))
            {
                Directory.Delete(modPath, true);
            }

            if (File.Exists(settings.ModFilePath))
            {
                File.Delete(settings.ModFilePath);
            }

            Directory.CreateDirectory(modDefinesDir);
            Directory.CreateDirectory(modProvincesDir);
            Directory.CreateDirectory(modCountriesDir);
            Directory.CreateDirectory(modSetupMainDir);
            Directory.CreateDirectory(modEventsDir);
            Directory.CreateDirectory(modLocalisationDir);
        }

        void WriteModDescriptor()
        {
            string descriptorFilePath = Path.Combine(settings.ModDirectoryPath, "descriptor.mod");

            string fileContent =
                $"name = \"{settings.ModName}\"" + Environment.NewLine +
                $"path = \"mod/{settings.ModId}\"" + Environment.NewLine +
                $"supported_version = \"{SupportedGameVersion}\"";

            WriteFile(settings.ModFilePath, fileContent);
            WriteFile(descriptorFilePath, fileContent);
        }

        void WriteProvinces()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "setup", "provinces", "02_shattered_world.txt");
            string fileContent = "# VANILLA PROVINCES - RANDOMISED" + Environment.NewLine;

            foreach (City city in entityManager.GetCities())
            {
                fileContent += WriteProvinceBlock(city);
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        string WriteProvinceBlock(City city)
        {
            string block =
                Environment.NewLine +
                $"# {city.NameId}" + Environment.NewLine +
                $"{city.Id}=" + Environment.NewLine +
                "{" + Environment.NewLine +
                $"\tbarbarian_power={city.BarbarianLevel}" + Environment.NewLine +
                $"\tcivilization_value={city.CivilizationLevel}" + Environment.NewLine +
                $"\tculture=\"{city.CultureId}\"" + Environment.NewLine +
                $"\tprovince_rank=\"{city.ProvinceRank}\"" + Environment.NewLine +
                $"\treligion=\"{city.ReligionId}\"" + Environment.NewLine +
                $"\ttrade_goods=\"{city.TradeGoodId}\"" + Environment.NewLine;

            foreach (string preservedLine in city.PreservedLines ?? [])
            {
                block += preservedLine + Environment.NewLine;
            }

            block += WritePopulationBlock("citizen", city.CitizensCount);
            block += WritePopulationBlock("freemen", city.FreemenCount);
            block += WritePopulationBlock("slaves", city.SlavesCount);
            block += WritePopulationBlock("tribesmen", city.TribesmenCount);

            block += "}" + Environment.NewLine;

            return block;
        }

        static string WritePopulationBlock(string populationType, int amount)
        {
            if (amount <= 0)
            {
                return string.Empty;
            }

            return
                $"\t{populationType}=" + Environment.NewLine +
                "\t{" + Environment.NewLine +
                $"\t\tamount={amount}" + Environment.NewLine +
                "\t}" + Environment.NewLine;
        }

        void WriteCountryFiles()
        {
            string dirPath = Path.Combine(settings.ModDirectoryPath, "setup", "countries");

            foreach (Country country in entityManager.GetCountries().Where(country => !country.IsVanilla))
            {
                string filePath = Path.Combine(dirPath, $"{country.Id}.txt");
                string fileContent =
                    $"color = rgb {{ {country.ColourRed} {country.ColourGreen} {country.ColourBlue} }}" + Environment.NewLine +
                    Environment.NewLine +
                    "ship_names = {" + Environment.NewLine +
                    $"  {string.Join(' ', country.ShipNames)}" + Environment.NewLine +
                    " }" + Environment.NewLine;

                WriteUnicodeFile(filePath, fileContent);
            }
        }

        void WriteCountriesDefinitionIndexFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "setup", "countries", "countries.txt");
            string fileContent =
                "REB = \"setup/countries/rebels.txt\"" + Environment.NewLine +
                "PIR = \"setup/countries/pirates.txt\"" + Environment.NewLine +
                "BAR = \"setup/countries/barbarians.txt\"" + Environment.NewLine +
                "MER = \"setup/countries/mercenaries.txt\"" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries())
            {
                string definitionFilePath = string.IsNullOrWhiteSpace(country.DefinitionFilePath)
                    ? $"setup/countries/{country.Id}.txt"
                    : country.DefinitionFilePath;

                fileContent += $"{country.Id} = \"{definitionFilePath}\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteCountriesLocalisationFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "localization", "english", "SW_countries_l_english.yml");
            string fileContent = "l_english:" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries().Where(country => !country.IsVanilla))
            {
                fileContent += $" {country.Id}:0 \"{country.Name}\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteCountrySetupFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "setup", "main", "02_shattered_world_countries.txt");
            string fileContent =
                "country = {" + Environment.NewLine +
                "\tcountries = {" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries())
            {
                fileContent += WriteCountrySetupBlock(country);
            }

            fileContent += "\t}" + Environment.NewLine + "}";

            WriteUnicodeFile(filePath, fileContent);
        }

        string WriteCountrySetupBlock(Country country)
        {
            string block =
                Environment.NewLine +
                $"\t\t{country.Id} = {{ # {country.Name}{Environment.NewLine}";

            if (country.IsVanilla)
            {
                block +=
                    VanillaCompatibilityComment + Environment.NewLine +
                    VanillaCompatibilityReasonComment + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(country.GovernmentId))
            {
                block += $"\t\t\tgovernment = {country.GovernmentId}{Environment.NewLine}";
            }

            if (!string.IsNullOrWhiteSpace(country.DiplomaticStanceId))
            {
                block += $"\t\t\tdiplomatic_stance={country.DiplomaticStanceId}{Environment.NewLine}";
            }

            if (!string.IsNullOrWhiteSpace(country.CultureId))
            {
                block += $"\t\t\tprimary_culture = {country.CultureId}{Environment.NewLine}";
            }

            if (!string.IsNullOrWhiteSpace(country.ReligionId))
            {
                block += $"\t\t\treligion = {country.ReligionId}{Environment.NewLine}";
            }

            if (country.CentralisationLevel >= 0)
            {
                block += $"\t\t\tcentralization={country.CentralisationLevel}{Environment.NewLine}";
            }

            block += $"\t\t\tcapital = {country.CapitalId}{Environment.NewLine}";

            foreach (string preservedLine in country.PreservedLines ?? [])
            {
                block += preservedLine + Environment.NewLine;
            }

            block +=
                $"\t\t\town_control_core = {{ {country.CapitalId} }}{Environment.NewLine}" +
                "\t\t}" + Environment.NewLine;

            return block;
        }

        void WriteEventsFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "events", "flavor_sel.txt");
            string fileContent =
                "namespace = grinsel" + Environment.NewLine +
                "grinsel.2 = {" + Environment.NewLine +
                "  type = province_event" + Environment.NewLine +
                "  fire_only_once = yes" + Environment.NewLine +
                "  trigger = {" + Environment.NewLine +
                "    num_of_cities = 1" + Environment.NewLine +
                "    is_capital = yes" + Environment.NewLine +
                "  }" + Environment.NewLine +
                "  title = grinsel_Events.1.t" + Environment.NewLine +
                "  desc = grinsel_Events.1.d" + Environment.NewLine +
                "  option = {" + Environment.NewLine +
                "  random_owned_province = {" + Environment.NewLine +
                "    name = grinsel_Events.1.a" + Environment.NewLine +
                ("    create_pop = slaves" + Environment.NewLine).Repeat(5) +
                "  }" + Environment.NewLine +
                "  option = {" + Environment.NewLine +
                "    name = \"dont need pops!\"" + Environment.NewLine +
                "  }" + Environment.NewLine +
                "}";

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteDefinesFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "common", "defines", "00_defines.txt");
            string fileContent =
                "NGame = {" + Environment.NewLine +
                "  START_DATE = \"450.10.1\"" + Environment.NewLine +
                "}";

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteFile(string path, string content)
        {
            string normalisedContent = NormaliseFileContent(content);

            File.WriteAllText(path, normalisedContent);
        }

        void WriteUnicodeFile(string path, string content)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.Write(content);
            }
        }

        string NormaliseFileContent(string content)
        {
            string normalisedContent = string.Empty;

            if (!content.StartsWith(Environment.NewLine))
            {
                normalisedContent += Environment.NewLine;
            }

            normalisedContent += content;

            if (!content.EndsWith(Environment.NewLine))
            {
                normalisedContent += Environment.NewLine;
            }

            return normalisedContent;
        }
    }
}