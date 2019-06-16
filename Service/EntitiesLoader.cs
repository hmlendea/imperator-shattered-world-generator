using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class EntitiesLoader : IEntitiesLoader
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
