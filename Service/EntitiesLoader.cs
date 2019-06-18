using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class EntitiesLoader : IEntityLoader
    {
        readonly string gameDirectory;

        public EntitiesLoader(string gameDirectory)
        {
            this.gameDirectory = gameDirectory;
        }

        public IEnumerable<City> LoadCities()
        {
            string citiesFilePath = Path.Combine(gameDirectory, "game", "common", "province_setup.csv");
            IRepository<CityEntity> vanillaCityRepository = new CsvRepository<CityEntity>(citiesFilePath);

            return vanillaCityRepository.GetAll().ToServiceModels();
        }

        public IEnumerable<Country> LoadCountries()
        {
            const string CountryIdRegexPattern = "^\\t\\t([A-Z]*)\\s*=";
            const string GovernmentRegexPattern = "^\\t\\t\\tgovernment\\s*=\\s*(.*)";
            const string DiplomaticStanceRegexPattern = "^\\t\\t\\tdiplomatic_stance\\s*=\\s*(.*)";
            const string CultureRegexPattern = "^\\t\\t\\tprimary_culture\\s*=\\s*([^ #]*)";
            const string ReligionRegexPattern = "^\\t\\t\\treligion\\s*=\\s*(.*)";
            const string CentralisationRegexPattern = "^\\t\\t\\tcentralization\\s*=\\s*([0-9]*)";
            const string CapitalRegexPattern = "^\\t\\t\\tcapital\\s*=\\s*([0-9]*)";
            const string ColourRegexPattern = "rgb\\s*{\\s*([0-9]*)\\s*([0-9]*)\\s*([0-9]*)\\s*}";

            string setupFilePath = Path.Combine(gameDirectory, "game", "common", "setup.txt");
            string localisationFilePath = Path.Combine(gameDirectory, "game", "localization", "english", "countries_l_english.yml");
            string countriesFilePath = Path.Combine(gameDirectory, "game", "common", "countries.txt");
            
            IList<Country> countries = new List<Country>();
            IList<string> lines = File.ReadAllLines(setupFilePath);

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                Match match = Regex.Match(line, CapitalRegexPattern);

                if (match.Success)
                {
                    Country country = new Country();
                    country.IsVanilla = true;
                    
                    for (int j = i; j > 0; j--)
                    {
                        string line2 = lines[j];

                        if (string.IsNullOrWhiteSpace(country.Id))
                        {
                            Match countryIdMatch = Regex.Match(line2, CountryIdRegexPattern);

                            if (countryIdMatch.Success)
                            {
                                country.Id = countryIdMatch.Groups[1].Value;
                                break;
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(country.GovernmentId))
                        {
                            Match governmentMatch = Regex.Match(line2, GovernmentRegexPattern);

                            if (governmentMatch.Success)
                            {
                                country.GovernmentId = governmentMatch.Groups[1].Value;
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(country.DiplomaticStanceId))
                        {
                            Match diplomaticStanceMatch = Regex.Match(line2, DiplomaticStanceRegexPattern);

                            if (diplomaticStanceMatch.Success)
                            {
                                country.DiplomaticStanceId = diplomaticStanceMatch.Groups[1].Value;
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(country.CultureId))
                        {
                            Match cultureMatch = Regex.Match(line2, CultureRegexPattern);

                            if (cultureMatch.Success)
                            {
                                country.CultureId = cultureMatch.Groups[1].Value;
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(country.ReligionId))
                        {
                            Match religionMatch = Regex.Match(line2, ReligionRegexPattern);

                            if (religionMatch.Success)
                            {
                                country.ReligionId = religionMatch.Groups[1].Value;
                            }
                        }
                        
                        if (country.CentralisationLevel == 0)
                        {
                            Match centralizationMatch = Regex.Match(line2, CentralisationRegexPattern);

                            if (centralizationMatch.Success && !string.IsNullOrWhiteSpace(centralizationMatch.Groups[1].Value))
                            {
                                country.CentralisationLevel = int.Parse(centralizationMatch.Groups[1].Value);
                            }
                        }
                        
                        if (string.IsNullOrWhiteSpace(country.CapitalId))
                        {
                            Match capitalMatch = Regex.Match(line2, CapitalRegexPattern);

                            if (capitalMatch.Success)
                            {
                                country.CapitalId = capitalMatch.Groups[1].Value;
                            }
                        }
                    }

                    string countryNameRegexPattern = $"^\\s*{country.Id}:.*\\s\"([^\"]*)\"";
                    string countryFileRegexPattern = $"^\\s*{country.Id}\\s*=\\s*\"([^\"]*)\"";

                    foreach (string localisationLine in File.ReadAllLines(localisationFilePath))
                    {
                        Match nameMatch = Regex.Match(localisationLine, countryNameRegexPattern);

                        if (nameMatch.Success)
                        {
                            country.Name = nameMatch.Groups[1].Value;
                            break;
                        }
                    }

                    foreach (string countriesFileLine in File.ReadAllLines(countriesFilePath))
                    {
                        Match fileMatch = Regex.Match(countriesFileLine, countryFileRegexPattern);

                        if (!fileMatch.Success)
                        {
                            continue;
                        }

                        string countryFilePath = Path.Combine(gameDirectory, "game", "common", fileMatch.Groups[1].Value);
                        string countryFileContent = File.ReadAllText(countryFilePath);

                        Match colourMatch = Regex.Match(countryFileContent, ColourRegexPattern);

                        country.ColourRed = int.Parse(colourMatch.Groups[1].Value);
                        country.ColourGreen = int.Parse(colourMatch.Groups[2].Value);
                        country.ColourBlue = int.Parse(colourMatch.Groups[3].Value);

                        break;
                    }
                    
                    countries.Add(country);
                }
            }

            return countries;
        }

        public IEnumerable<string> LoadReligionIds()
        {
            const string ReligionIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            string religionsFilePath = Path.Combine(gameDirectory, "game", "common", "religions", "00_default.txt");
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
            const string CultureIdRegexPattern = "^\\t\\t([^\\s]*)\\s*=\\s*{\\s*}";

            IList<string> cultureIds = new List<string>();

            string culturesDirPath = Path.Combine(gameDirectory, "game", "common", "cultures");
            DirectoryInfo dir = new DirectoryInfo(culturesDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                IEnumerable<string> lines = File.ReadAllLines(file.FullName);

                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, CultureIdRegexPattern);

                    if (!match.Success)
                    {
                        continue;
                    }

                    cultureIds.Add(match.Groups[1].Value);
                }
            }

            return cultureIds;
        }

        public IEnumerable<string> LoadGovernmentIds()
        {
            const string GovernmentIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            IList<string> governmentIds = new List<string>();

            string governmentsDirPath = Path.Combine(gameDirectory, "game", "common", "governments");
            DirectoryInfo dir = new DirectoryInfo(governmentsDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                IEnumerable<string> lines = File.ReadAllLines(file.FullName);

                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, GovernmentIdRegexPattern);

                    if (!match.Success)
                    {
                        continue;
                    }

                    governmentIds.Add(match.Groups[1].Value);
                }
            }

            return governmentIds;
        }

        public IEnumerable<string> LoadDiplomaticStanceIds()
        {
            const string DiplomaticStanceIdRegexPattern = "^([^#\\s]*)\\s*=\\s*{";

            IList<string> diplomaticStanceIds = new List<string>();

            string diplomaticStancesDirPath = Path.Combine(gameDirectory, "game", "common", "diplomatic_stances");
            DirectoryInfo dir = new DirectoryInfo(diplomaticStancesDirPath);

            foreach (FileInfo file in dir.GetFiles("*.txt"))
            {
                IEnumerable<string> lines = File.ReadAllLines(file.FullName);

                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, DiplomaticStanceIdRegexPattern);

                    if (!match.Success)
                    {
                        continue;
                    }

                    diplomaticStanceIds.Add(match.Groups[1].Value);
                }
            }

            return diplomaticStanceIds;
        }
    }
}
