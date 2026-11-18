using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NuciDAL.IO;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.DataAccess.Parsing;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class EntitiesLoader(GeneratorSettings settings) : IEntityLoader
    {
        const char ProvinceDefinitionSeparator = ';';
        const int ProvinceDefinitionFieldCount = 5;

        static readonly Regex AmountRegex = new("amount=([0-9]+)", RegexOptions.Compiled);

        public IEnumerable<City> LoadCities()
        {
            string provincesDirectoryPath = Path.Combine(settings.GameDirectoryPath, "game", "setup", "provinces");
            IDictionary<string, string> provinceNames = LoadProvinceNames();

            List<City> cities = new();

            if (!Directory.Exists(provincesDirectoryPath))
            {
                return cities;
            }

            string[] provinceFilePaths = Directory.GetFiles(provincesDirectoryPath, "*.txt");
            Array.Sort(provinceFilePaths, StringComparer.Ordinal);

            foreach (string provinceFilePath in provinceFilePaths)
            {
                cities.AddRange(LoadCityFile(provinceFilePath, provinceNames));
            }

            return cities;
        }

        public IEnumerable<Country> LoadCountries()
        {
            string gameDirectoryPath = settings.GameDirectoryPath;
            string setupDirectoryPath = Path.Combine(gameDirectoryPath, "game", "setup");
            IDictionary<string, string> countryFilePaths = ReadCountryFilePaths(Path.Combine(setupDirectoryPath, "countries", "countries.txt"));
            IDictionary<string, string> countryNames = LoadCountryNames();
            string setupFilePath = Path.Combine(setupDirectoryPath, "main", "00_default.txt");
            IList<string> lines = File.ReadAllLines(setupFilePath);
            int countriesBlockIndex = FindCountriesBlockIndex(lines);

            IDictionary<string, Country> countries = new Dictionary<string, Country>();

            if (countriesBlockIndex < 0)
            {
                return countries.Values;
            }

            for (int index = countriesBlockIndex + 1; index < lines.Count; index++)
            {
                Match countryMatch = Regex.Match(lines[index], "^\\t\\t([A-Z0-9]{2,})\\s*=\\s*\\{");

                if (!countryMatch.Success)
                {
                    continue;
                }

                int blockEndIndex = ClausewitzText.FindBlockEnd(lines, index, 1);
                string countryId = countryMatch.Groups[1].Value;
                IList<string> blockContent = GetBlockContent(lines, index, blockEndIndex);

                index = blockEndIndex;

                if (countries.ContainsKey(countryId))
                {
                    continue;
                }

                Country country = BuildCountry(countryId, blockContent, countryFilePaths, countryNames, gameDirectoryPath);

                if (country is null)
                {
                    continue;
                }

                countries[countryId] = country;
            }

            return countries.Values;
        }

        static Country BuildCountry(
            string countryId,
            IList<string> blockContent,
            IDictionary<string, string> countryFilePaths,
            IDictionary<string, string> countryNames,
            string gameDirectoryPath)
        {
            Country country = new()
            {
                Id = countryId,
                IsVanilla = true,
                GovernmentId = GetAttributeValue(blockContent, "government"),
                DiplomaticStanceId = GetAttributeValue(blockContent, "diplomatic_stance"),
                CultureId = GetAttributeValue(blockContent, "primary_culture"),
                ReligionId = GetAttributeValue(blockContent, "religion"),
                CapitalId = GetAttributeValue(blockContent, "capital"),
                PreservedLines = GetPreservedCountryLines(blockContent)
            };

            string centralisationLevel = GetAttributeValue(blockContent, "centralization");

            if (int.TryParse(centralisationLevel, out int parsedCentralisationLevel))
            {
                country.CentralisationLevel = parsedCentralisationLevel;
            }

            if (countryNames.TryGetValue(countryId, out string countryName))
            {
                country.Name = countryName;
            }

            if (countryFilePaths.TryGetValue(countryId, out string relativeFilePath))
            {
                country.DefinitionFilePath = relativeFilePath;

                string countryFilePath = Path.Combine(gameDirectoryPath, "game", relativeFilePath);

                if (File.Exists(countryFilePath))
                {
                    ApplyCountryFileColours(country, countryFilePath);
                }
            }

            return country;
        }

        static IEnumerable<string> GetPreservedCountryLines(IList<string> blockContent)
        {
            IList<string> preservedLines = new List<string>();
            int depth = 0;
            int managedBlockDepth = 0;

            foreach (string line in blockContent)
            {
                string trimmedLine = line.Trim();
                int braceDelta = ClausewitzText.GetBraceDelta(line);

                if (managedBlockDepth > 0)
                {
                    managedBlockDepth += braceDelta;

                    continue;
                }

                if (depth > 0)
                {
                    preservedLines.Add(line);
                    depth += braceDelta;

                    continue;
                }

                if (trimmedLine.Length == 0 || trimmedLine == "{" || trimmedLine == "}")
                {
                    continue;
                }

                if (IsManagedCountryAttribute(trimmedLine))
                {
                    if (braceDelta > 0)
                    {
                        managedBlockDepth = braceDelta;
                    }

                    continue;
                }

                preservedLines.Add(line);
                depth = braceDelta > 0 ? braceDelta : 0;
            }

            return preservedLines;
        }

        static bool IsManagedCountryAttribute(string line)
            => line.StartsWith("government ", StringComparison.Ordinal) ||
               line.StartsWith("government=", StringComparison.Ordinal) ||
               line.StartsWith("diplomatic_stance", StringComparison.Ordinal) ||
               line.StartsWith("primary_culture", StringComparison.Ordinal) ||
               line.StartsWith("religion ", StringComparison.Ordinal) ||
               line.StartsWith("religion=", StringComparison.Ordinal) ||
               line.StartsWith("centralization", StringComparison.Ordinal) ||
               line.StartsWith("capital ", StringComparison.Ordinal) ||
               line.StartsWith("capital=", StringComparison.Ordinal) ||
               line.StartsWith("own_control_core", StringComparison.Ordinal);

        public IEnumerable<string> LoadReligionIds()
        {
            const string ReligionIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            string religionsFilePath = Path.Combine(settings.GameDirectoryPath, "game", "common", "religions", "00_default.txt");
            IEnumerable<string> lines = File.ReadAllLines(religionsFilePath);
            IList<string> religionIds = new List<string>();

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, ReligionIdRegexPattern);

                if (!match.Success)
                {
                    continue;
                }

                religionIds.Add(match.Groups[1].Value);
            }

            return religionIds;
        }

        public IEnumerable<string> LoadCultureIds()
        {
            IList<string> cultureIds = new List<string>();

            string culturesDirPath = Path.Combine(settings.GameDirectoryPath, "game", "common", "cultures");
            DirectoryInfo dir = new DirectoryInfo(culturesDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                foreach (string cultureId in LoadCultureIds(file.FullName))
                {
                    cultureIds.Add(cultureId);
                }
            }

            return cultureIds;
        }

        public IEnumerable<string> LoadGovernmentIds()
        {
            const string GovernmentIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            IList<string> governmentIds = new List<string>();

            string governmentsDirPath = Path.Combine(settings.GameDirectoryPath, "game", "common", "governments");
            DirectoryInfo dir = new DirectoryInfo(governmentsDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                foreach (string line in File.ReadAllLines(file.FullName))
                {
                    Match match = Regex.Match(line, GovernmentIdRegexPattern);

                    if (match.Success)
                    {
                        governmentIds.Add(match.Groups[1].Value);
                    }
                }
            }

            return governmentIds;
        }

        public IEnumerable<string> LoadDiplomaticStanceIds()
        {
            const string DiplomaticStanceIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            IList<string> diplomaticStanceIds = new List<string>();

            string diplomaticStancesDirPath = Path.Combine(settings.GameDirectoryPath, "game", "common", "diplomatic_stances");
            DirectoryInfo dir = new DirectoryInfo(diplomaticStancesDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                foreach (string line in File.ReadAllLines(file.FullName))
                {
                    Match match = Regex.Match(line, DiplomaticStanceIdRegexPattern);

                    if (match.Success)
                    {
                        diplomaticStanceIds.Add(match.Groups[1].Value);
                    }
                }
            }

            return diplomaticStanceIds;
        }

        static IEnumerable<City> LoadCityFile(string provinceFilePath, IDictionary<string, string> provinceNames)
        {
            IList<string> lines = File.ReadAllLines(provinceFilePath);
            IList<City> cities = new List<City>();
            int index = 0;

            while (index < lines.Count)
            {
                Match headerMatch = Regex.Match(lines[index], "^\\s*([0-9]+)\\s*=");

                if (!headerMatch.Success)
                {
                    index += 1;

                    continue;
                }

                int blockEndIndex = ClausewitzText.FindBlockEnd(lines, index, 0);
                string provinceId = headerMatch.Groups[1].Value;

                City city = BuildCity(lines, index, blockEndIndex, provinceId, provinceNames);

                if (city is not null)
                {
                    cities.Add(city);
                }

                index = blockEndIndex + 1;
            }

            return cities;
        }

        static City BuildCity(IList<string> lines, int headerIndex, int blockEndIndex, string provinceId, IDictionary<string, string> provinceNames)
        {
            if (!provinceNames.TryGetValue(provinceId, out string nameId) || string.IsNullOrWhiteSpace(nameId))
            {
                return null;
            }

            IList<string> blockContent = GetBlockContent(lines, headerIndex, blockEndIndex);

            CityEntity cityEntity = new()
            {
                Id = provinceId,
                ProvinceId = provinceId,
                NameId = nameId,
                CultureId = GetAttributeValue(blockContent, "culture"),
                ReligionId = GetAttributeValue(blockContent, "religion"),
                TradeGoodId = GetAttributeValue(blockContent, "trade_goods"),
                ProvinceRank = GetAttributeValue(blockContent, "province_rank"),
                CivilizationLevel = GetAttributeValue(blockContent, "civilization_value"),
                BarbarianLevel = GetAttributeValue(blockContent, "barbarian_power")
            };

            IList<string> preservedLines = new List<string>();
            ProcessBlockBody(blockContent, cityEntity, preservedLines);
            cityEntity.PreservedLines = preservedLines;

            City city = cityEntity.ToServiceModel();

            return city.IsHabitable ? city : null;
        }

        static IList<string> GetBlockContent(IList<string> lines, int headerIndex, int blockEndIndex)
        {
            IList<string> blockContent = new List<string>();

            for (int index = headerIndex + 1; index < blockEndIndex; index++)
            {
                blockContent.Add(lines[index]);
            }

            return blockContent;
        }

        static void ProcessBlockBody(IList<string> blockContent, CityEntity cityEntity, IList<string> preservedLines)
        {
            int depth = 0;
            string populationType = null;
            bool isAwaitingPopulationBrace = false;

            foreach (string line in blockContent)
            {
                string trimmedLine = line.Trim();

                if (isAwaitingPopulationBrace)
                {
                    if (trimmedLine.Length == 0)
                    {
                        continue;
                    }

                    isAwaitingPopulationBrace = false;
                    depth = ClausewitzText.GetBraceDelta(line);
                    AddInlineAmount(trimmedLine, cityEntity, populationType);

                    if (depth <= 0)
                    {
                        depth = 0;
                        populationType = null;
                    }

                    continue;
                }

                if (depth <= 0)
                {
                    if (trimmedLine.Length == 0 || trimmedLine == "{" || trimmedLine == "}")
                    {
                        continue;
                    }

                    if (TryGetPopulationHeader(trimmedLine, out string typeId))
                    {
                        populationType = typeId;
                        depth = ClausewitzText.GetBraceDelta(line);
                        AddInlineAmount(trimmedLine, cityEntity, populationType);

                        if (depth <= 0)
                        {
                            if (line.Contains('{'))
                            {
                                depth = 0;
                                populationType = null;
                            }
                            else
                            {
                                isAwaitingPopulationBrace = true;
                            }
                        }

                        continue;
                    }

                    if (IsManagedAttribute(trimmedLine))
                    {
                        continue;
                    }

                    preservedLines.Add(line);
                    depth = ClausewitzText.GetBraceDelta(line);

                    continue;
                }

                AddInlineAmount(trimmedLine, cityEntity, populationType);
                depth += ClausewitzText.GetBraceDelta(line);

                if (depth <= 0)
                {
                    depth = 0;
                    populationType = null;
                }
            }
        }

        static void AddInlineAmount(string line, CityEntity cityEntity, string populationType)
        {
            Match amountMatch = AmountRegex.Match(line);

            if (amountMatch.Success && int.TryParse(amountMatch.Groups[1].Value, out int amount))
            {
                AddPopulation(cityEntity, populationType, amount);
            }
        }

        static string GetAttributeValue(IList<string> blockContent, string attributeId)
        {
            foreach (string line in blockContent)
            {
                string trimmedLine = ClausewitzText.StripComment(line).Trim();
                int equalsIndex = trimmedLine.IndexOf('=');

                if (equalsIndex <= 0)
                {
                    continue;
                }

                string key = trimmedLine.Substring(0, equalsIndex).Trim();

                if (!string.Equals(key, attributeId, StringComparison.Ordinal))
                {
                    continue;
                }

                return trimmedLine.Substring(equalsIndex + 1).Trim().Trim('"');
            }

            return string.Empty;
        }

        static bool TryGetPopulationHeader(string line, out string populationType)
        {
            populationType = null;

            Match match = Regex.Match(line, "^(citizen|freemen|slaves|tribesmen|nobles)\\s*=");

            if (!match.Success)
            {
                return false;
            }

            populationType = match.Groups[1].Value;

            return true;
        }

        static bool IsManagedAttribute(string line)
            => line.StartsWith("culture=", StringComparison.Ordinal) ||
               line.StartsWith("religion=", StringComparison.Ordinal) ||
               line.StartsWith("trade_goods=", StringComparison.Ordinal) ||
               line.StartsWith("province_rank=", StringComparison.Ordinal) ||
               line.StartsWith("civilization_value=", StringComparison.Ordinal) ||
               line.StartsWith("barbarian_power=", StringComparison.Ordinal);

        static void AddPopulation(CityEntity cityEntity, string populationType, int amount)
        {
            if (populationType == "citizen")
            {
                cityEntity.CitizensCount += amount;
            }
            else if (populationType == "freemen")
            {
                cityEntity.FreemenCount += amount;
            }
            else if (populationType == "slaves")
            {
                cityEntity.SlavesCount += amount;
            }
            else if (populationType == "tribesmen")
            {
                cityEntity.TribesmenCount += amount;
            }
        }

        IDictionary<string, string> LoadProvinceNames()
        {
            string definitionFilePath = Path.Combine(settings.GameDirectoryPath, "game", "map_data", "definition.csv");
            IDictionary<string, string> provinceNames = new Dictionary<string, string>();

            if (!File.Exists(definitionFilePath))
            {
                return provinceNames;
            }

            string normalisedDefinitionFilePath = NormaliseProvinceDefinition(definitionFilePath);

            CsvFile<ProvinceDefinitionEntity> provinceDefinitionFile = new(normalisedDefinitionFilePath, ProvinceDefinitionSeparator);

            foreach (ProvinceDefinitionEntity provinceDefinition in provinceDefinitionFile.LoadEntities())
            {
                if (string.IsNullOrWhiteSpace(provinceDefinition.Name))
                {
                    continue;
                }

                provinceNames[provinceDefinition.Id] = provinceDefinition.Name;
            }

            File.Delete(normalisedDefinitionFilePath);

            return provinceNames;
        }

        static string NormaliseProvinceDefinition(string definitionFilePath)
        {
            List<string> normalisedLines = new();

            foreach (string line in File.ReadAllLines(definitionFilePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                string[] fields = line.Split(ProvinceDefinitionSeparator);

                if (fields.Length < ProvinceDefinitionFieldCount)
                {
                    continue;
                }

                normalisedLines.Add(string.Join(ProvinceDefinitionSeparator, fields.Take(ProvinceDefinitionFieldCount)));
            }

            string normalisedFilePath = Path.Combine(Path.GetTempPath(), $"imperator-province-definition-{Guid.NewGuid():N}.csv");
            File.WriteAllLines(normalisedFilePath, normalisedLines);

            return normalisedFilePath;
        }

        IDictionary<string, string> LoadCountryNames()
        {
            string localisationFilePath = Path.Combine(settings.GameDirectoryPath, "game", "localization", "english", "countries_l_english.yml");
            IDictionary<string, string> countryNames = new Dictionary<string, string>();

            if (!File.Exists(localisationFilePath))
            {
                return countryNames;
            }

            foreach (string line in File.ReadAllLines(localisationFilePath))
            {
                Match match = Regex.Match(line, "^\\s*([A-Z0-9]{2,}):0\\s*\"([^\"]*)\"");

                if (!match.Success)
                {
                    continue;
                }

                countryNames[match.Groups[1].Value] = match.Groups[2].Value;
            }

            return countryNames;
        }

        static void ApplyCountryFileColours(Country country, string countryFilePath)
        {
            Match match = Regex.Match(File.ReadAllText(countryFilePath), "rgb\\s*{\\s*([0-9]+)\\s+([0-9]+)\\s+([0-9]+)\\s*}");

            if (!match.Success)
            {
                return;
            }

            country.ColourRed = int.Parse(match.Groups[1].Value);
            country.ColourGreen = int.Parse(match.Groups[2].Value);
            country.ColourBlue = int.Parse(match.Groups[3].Value);
        }

        static IDictionary<string, string> ReadCountryFilePaths(string countriesIndexFilePath)
        {
            IDictionary<string, string> countryFilePaths = new Dictionary<string, string>();

            foreach (string line in File.ReadAllLines(countriesIndexFilePath))
            {
                Match match = Regex.Match(line, "^\\s*([A-Z0-9]{2,})\\s*=\\s*\"([^\"]+)\"");

                if (match.Success)
                {
                    countryFilePaths[match.Groups[1].Value] = match.Groups[2].Value;
                }
            }

            return countryFilePaths;
        }

        static int FindCountriesBlockIndex(IList<string> lines)
        {
            int countryBlockIndex = -1;

            for (int index = 0; index < lines.Count; index++)
            {
                string trimmedLine = lines[index].TrimStart();

                if (countryBlockIndex < 0 && trimmedLine.StartsWith("country", StringComparison.Ordinal) && trimmedLine.Contains('{'))
                {
                    countryBlockIndex = index;

                    continue;
                }

                if (countryBlockIndex >= 0 && trimmedLine.StartsWith("countries", StringComparison.Ordinal) && trimmedLine.Contains('{'))
                {
                    return index;
                }
            }

            return -1;
        }

        static IEnumerable<string> LoadCultureIds(string culturesFilePath)
        {
            IList<string> lines = File.ReadAllLines(culturesFilePath);
            IList<string> cultureIds = new List<string>();
            int cultureBlockIndex = -1;
            int cultureBlockEndIndex = lines.Count;

            for (int index = 0; index < lines.Count; index++)
            {
                if (Regex.IsMatch(lines[index], "^\\s*culture\\s*=\\s*\\{"))
                {
                    cultureBlockIndex = index;
                    cultureBlockEndIndex = ClausewitzText.FindBlockEnd(lines, index, ClausewitzText.GetBraceDelta(lines[index]));

                    break;
                }
            }

            if (cultureBlockIndex < 0)
            {
                return cultureIds;
            }

            for (int index = cultureBlockIndex + 1; index < cultureBlockEndIndex; index++)
            {
                Match match = Regex.Match(lines[index], "^\\t\\t([a-zA-Z0-9_.]+)\\s*=\\s*\\{");

                if (match.Success)
                {
                    cultureIds.Add(match.Groups[1].Value);
                }
            }

            return cultureIds;
        }
    }
}