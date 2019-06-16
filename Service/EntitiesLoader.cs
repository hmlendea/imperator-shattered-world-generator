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
    }
}
