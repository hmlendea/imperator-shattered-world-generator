using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.DataAccess.Parsing;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ModWriter(
        IEntityManager entityManager,
        GeneratorSettings settings) : IModWriter
    {
        const string SupportedGameVersion = "2.0.*";

        const string VanillaCompatibilityComment =
            "\t\t\t# VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY";

        static readonly string[] ReservedCountryIds = ["REB", "PIR", "BAR", "MER"];

        const string VanillaCompatibilityReasonComment =
            "\t\t\t# Removing it could break the game once enough ingame years (even decades) have passed";

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
            WriteEventsLocalisationFile();
        }

        void PrepareDirectoryStructure()
        {
            string modPath = settings.ModDirectoryPath;
            string modSetupDir = Path.Combine(modPath, "setup");
            string modEventsDir = Path.Combine(modPath, "events");
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

            HashSet<string> writtenCountryIds = new(ReservedCountryIds);

            foreach (Country country in entityManager.GetCountries())
            {
                if (!writtenCountryIds.Add(country.Id))
                {
                    continue;
                }

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
            string filePath = Path.Combine(settings.ModDirectoryPath, "setup", "main", "00_default.txt");
            string fileContent = BuildCountrySetupFileContent() + Environment.NewLine;

            WriteUnicodeFileWithoutBom(filePath, fileContent);
        }

        string BuildCountrySetupFileContent()
        {
            IList<string> vanillaLines = ReadVanillaSetupFile();
            IList<string> countryBlockLines = BuildCountryBlockLines();

            int headerIndex = FindCountryBlockHeaderIndex(vanillaLines);

            if (headerIndex < 0)
            {
                return JoinLines(vanillaLines, countryBlockLines);
            }

            int endIndex = ClausewitzText.FindBlockEnd(vanillaLines, headerIndex, 1);

            List<string> fileLines = [];

            for (int index = 0; index < headerIndex; index++)
            {
                fileLines.Add(vanillaLines[index]);
            }

            fileLines.AddRange(countryBlockLines);

            for (int index = endIndex + 1; index < vanillaLines.Count; index++)
            {
                fileLines.Add(vanillaLines[index]);
            }

            return JoinLines(fileLines);
        }

        IList<string> ReadVanillaSetupFile()
        {
            if (string.IsNullOrWhiteSpace(settings.GameDirectoryPath))
            {
                return [];
            }

            string vanillaSetupFilePath = Path.Combine(
                settings.GameDirectoryPath,
                "game",
                "setup",
                "main",
                "00_default.txt");

            if (!File.Exists(vanillaSetupFilePath))
            {
                return [];
            }

            return File.ReadAllLines(vanillaSetupFilePath);
        }

        static int FindCountryBlockHeaderIndex(IList<string> lines)
        {
            for (int index = 0; index < lines.Count; index++)
            {
                string line = lines[index];

                if (line.Length == 0 || char.IsWhiteSpace(line[0]))
                {
                    continue;
                }

                if (line.Trim() == "country = {")
                {
                    return index;
                }
            }

            return -1;
        }

        IList<string> BuildCountryBlockLines()
        {
            List<string> countryBlockLines =
            [
                "country = {",
                "\tcountries = {"
            ];

            foreach (Country country in entityManager.GetCountries())
            {
                countryBlockLines.AddRange(SplitLines(WriteCountrySetupBlock(country)));
            }

            countryBlockLines.Add("\t}");
            countryBlockLines.Add("}");

            return countryBlockLines;
        }

        static string JoinLines(params IList<string>[] lineGroups)
        {
            List<string> lines = [];

            foreach (IList<string> lineGroup in lineGroups)
            {
                lines.AddRange(lineGroup);
            }

            return string.Join(Environment.NewLine, lines);
        }

        static IEnumerable<string> SplitLines(string content)
            => content
                .Replace("\r\n", "\n")
                .Split('\n')
                .Where(line => line.Length > 0);

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

            if (!string.IsNullOrWhiteSpace(country.CapitalId))
            {
                block += $"\t\t\tcapital = {country.CapitalId}{Environment.NewLine}";
            }

            foreach (string preservedLine in country.PreservedLines ?? [])
            {
                block += preservedLine + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(country.CapitalId))
            {
                block += $"\t\t\town_control_core = {{ {country.CapitalId} }}{Environment.NewLine}";
            }

            block += "\t\t}" + Environment.NewLine;

            return block;
        }

        void WriteEventsFile()
        {
            string fileBody =
                "namespace = grinsel" + Environment.NewLine +
                Environment.NewLine +
                "grinsel.2 = {" + Environment.NewLine +
                "\ttype = country_event" + Environment.NewLine +
                "\tfire_only_once = yes" + Environment.NewLine +
                "\ttrigger = {" + Environment.NewLine +
                "\t\tnum_of_cities = 1" + Environment.NewLine +
                "\t}" + Environment.NewLine +
                "\ttitle = grinsel.2.t" + Environment.NewLine +
                "\tdesc = grinsel.2.d" + Environment.NewLine +
                "\toption = {" + Environment.NewLine +
                "\t\tname = grinsel.2.a" + Environment.NewLine +
                "\t\trandom_owned_province = {" + Environment.NewLine +
                ("\t\t\tcreate_pop = slaves" + Environment.NewLine).Repeat(5) +
                "\t\t}" + Environment.NewLine +
                "\t}" + Environment.NewLine +
                "\toption = {" + Environment.NewLine +
                "\t\tname = grinsel.2.b" + Environment.NewLine +
                "\t}" + Environment.NewLine +
                "}";

            string filePath = Path.Combine(settings.ModDirectoryPath, "events", "SW_events.txt");
            WriteUnicodeFile(filePath, fileBody);
        }

        void WriteEventsLocalisationFile()
        {
            string fileContent =
                "l_english:" + Environment.NewLine +
                " grinsel.2.t:0 \"A Minor Settlement\"" + Environment.NewLine +
                " grinsel.2.d:0 \"Our capital is the only settlement we hold.\"" + Environment.NewLine +
                " grinsel.2.a:0 \"We need more people.\"" + Environment.NewLine +
                " grinsel.2.b:0 \"We do not need pops!\"";

            string filePath = Path.Combine(settings.ModDirectoryPath, "localization", "english", "SW_events_l_english.yml");
            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteFile(string path, string content)
        {
            string normalisedContent = NormaliseFileContent(content);

            File.WriteAllText(path, normalisedContent);
        }

        void WriteUnicodeFile(string path, string content)
        {
            using StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8);
            writer.Write(content);
        }

        static void WriteUnicodeFileWithoutBom(string path, string content)
        {
            using StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(false));
            writer.Write(content);
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