using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;
using NuciExtensions;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ModWriter : IModWriter
    {
        public void CreateMod(
            string modName,
            IEnumerable<City> cities,
            IEnumerable<Country> countries)
        {
            PrepareDirectoryStructure(modName);
            CreateModMetadata(modName);

            WriteProvincesSetup(modName, cities);
            WriteCountryFiles(modName, countries);
            WriteCountriesDefinitionIndexFile(modName, countries);
            WriteCountriesLocalisationFile(modName, countries);
            WriteSetupFile(modName, countries);
        }

        void PrepareDirectoryStructure(string modName)
        {
            string modPath = GetModDirectoryPath(modName);
            string modCommonDirectory = Path.Combine(modPath, "common");
            string modCountriesDirectory = Path.Combine(modCommonDirectory, "countries");
            string modLocalisationDirectory = Path.Combine(modPath, "localization");
            string modFilePath = GetModFilePath(modName);

            if (Directory.Exists(modPath))
            {
                Directory.Delete(modPath, true);
            }

            if (File.Exists(modFilePath))
            {
                File.Delete(modFilePath);
            }
            
            Directory.CreateDirectory(modCountriesDirectory);
            Directory.CreateDirectory(modLocalisationDirectory);
        }

        void WriteProvincesSetup(string modName, IEnumerable<City> cities)
        {
            string filePath = Path.Combine(GetModDirectoryPath(modName), "common", "province_setup.csv");

            IRepository<CityEntity> repo = new CsvRepository<CityEntity>(filePath);

            foreach (City city in cities)
            {
                repo.Add(city.ToDataObject());
            }

            repo.ApplyChanges();
        }

        void WriteCountryFiles(string modName, IEnumerable<Country> countries)
        {
            string dirPath = Path.Combine(GetModDirectoryPath(modName), "common", "countries");

            foreach (Country country in countries)
            {
                string filePath = Path.Combine(dirPath, $"{country.Id}.txt");
                string fileContent =
                    $"color = rgb {{ {country.ColourRed} {country.ColourGreen} {country.ColourBlue} }}" + Environment.NewLine +
                    Environment.NewLine +
                    $"ship_names = {{" + Environment.NewLine +
                    $"  {string.Join(' ', country.ShipNames)}" + Environment.NewLine +
                    $" }}" + Environment.NewLine;
                
                WriteUnicodeFile(filePath, fileContent);
            }
        }

        void WriteCountriesDefinitionIndexFile(string modName, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(GetModDirectoryPath(modName), "common", "countries.txt");
            string fileContent =
                "REB = \"countries/rebels.txt\"" + Environment.NewLine +
                "PIR = \"countries/pirates.txt\"" + Environment.NewLine +
                "BAR = \"countries/barbarians.txt\"" + Environment.NewLine +
                "MER = \"countries/mercenaries.txt\"" + Environment.NewLine;

            foreach (Country country in countries)
            {
                fileContent += $"{country.Id} = \"countries/{country.Id}.txt\"{Environment.NewLine}";
            }

            WriteFile(filePath, fileContent);
        }

        void WriteCountriesLocalisationFile(string modName, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(GetModDirectoryPath(modName), "localization", "SW_countries_l_english.yml");
            string fileContent = "l_english:" + Environment.NewLine;

            foreach (Country country in countries.Where(c => !c.IsVanilla))
            {
                fileContent += $" {country.Id}:0 \"{country.Name}\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteSetupFile(string modName, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(GetModDirectoryPath(modName), "common", "setup.txt");
            string fileContent =
                "country = {" + Environment.NewLine +
                "  countries = {" + Environment.NewLine;

            foreach (Country country in countries)
            {
                fileContent += $"    {country.Id} = {{ # " + country.Name + Environment.NewLine;

                if (country.IsVanilla)
                {
                    fileContent +=
                        $"      # VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY" + Environment.NewLine +
                        $"      # Removing it could break the game once enough ingame years (even decades) have passed" + Environment.NewLine;
                }

                fileContent +=
                    $"      government = {country.GovernmentId}" + Environment.NewLine;
                
                if (!string.IsNullOrWhiteSpace(country.DiplomaticStanceId))
                {
                    fileContent += $"      diplomatic_stance = {country.DiplomaticStanceId}" + Environment.NewLine;
                }

                fileContent +=
                    $"      primary_culture = {country.CultureId}" + Environment.NewLine +
                    $"      religion = {country.ReligionId}" + Environment.NewLine;

                if (country.CentralisationLevel >= 0)
                {
                    fileContent += $"      centralization = {country.CentralisationLevel}" + Environment.NewLine;
                }

                fileContent +=
                    $"      capital = {country.CapitalId}" + Environment.NewLine +
                    $"      own_control_core = {{ {country.CapitalId} }}" + Environment.NewLine +
                    $"    }}" + Environment.NewLine;
            }

            fileContent += "  }" + Environment.NewLine + "}";

            WriteFile(filePath, fileContent);
        }

        void CreateModMetadata(string modName)
        {
            string modDirectoryName = GetModDirectoryName(modName);
            string modFilePath = GetModFilePath(modName);
            string descriptorFilePath = Path.Combine(GetModDirectoryPath(modName), "descriptor.mod");

            string fileContent =
                $"name = \"{modName}\"" + Environment.NewLine +
                $"path = \"mod/{modDirectoryName}\"";

            WriteFile(modFilePath, fileContent);
            WriteFile(descriptorFilePath, fileContent);
        }

        string GetModFilePath(string modName)
        {
            return GetModDirectoryPath(modName) + ".mod";
        }

        string GetModDirectoryPath(string modName)
        {
            string modDirName = GetModDirectoryName(modName);
                
            return Path.Combine(Environment.CurrentDirectory, "out", modDirName);
        }

        string GetModDirectoryName(string modName)
        {
            return modName
                .RemovePunctuation()
                .RemoveDiacritics()
                .Replace(" ", "-")
                .ToLower();
        }

        void WriteFile(string path, string content)
        {
            string normalisedContent = NormaliseFileContent(content);

            File.WriteAllText(path, normalisedContent);
        }

        void WriteUnicodeFile(string path, string content)
        {
            string normalisedContent = NormaliseFileContent(content);

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
