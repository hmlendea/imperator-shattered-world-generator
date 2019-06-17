using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ModWriter : IModWriter
    {
        public void CreateMod(
            string modPath,
            IEnumerable<City> cities,
            IEnumerable<Country> countries)
        {
            CreateModStructure(modPath);

            WriteProvincesSetup(modPath, cities);
            WriteCountryFiles(modPath, countries);
            WriteCountriesDefinitionIndexFile(modPath, countries);
            WriteCountriesLocalisationFile(modPath, countries);
            WriteSetupFile(modPath, countries);
        }

        void CreateModStructure(string modPath)
        {
            string modCommonDirectory = Path.Combine(modPath, "common");
            string modCountriesDirectory = Path.Combine(modCommonDirectory, "countries");
            string modLocalisationDirectory = Path.Combine(modPath, "localization");

            if (Directory.Exists(modPath))
            {
                Directory.Delete(modPath, true);
            }
            
            Directory.CreateDirectory(modCountriesDirectory);
            Directory.CreateDirectory(modLocalisationDirectory);
        }

        void WriteProvincesSetup(string modPath, IEnumerable<City> cities)
        {
            string filePath = Path.Combine(modPath, "common", "province_setup.csv");

            IRepository<CityEntity> repo = new CsvRepository<CityEntity>(filePath);

            foreach (City city in cities)
            {
                repo.Add(city.ToDataObject());
            }

            repo.ApplyChanges();
        }

        void WriteCountryFiles(string modPath, IEnumerable<Country> countries)
        {
            string dirPath = Path.Combine(modPath, "common", "countries");

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

        void WriteCountriesDefinitionIndexFile(string modPath, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(modPath, "common", "countries.txt");
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

        void WriteCountriesLocalisationFile(string modPath, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(modPath, "localization", "SW_countries.yml");
            string fileContent = "l_english:" + Environment.NewLine;

            foreach (Country country in countries.Where(c => !c.IsVanilla))
            {
                fileContent += $"  {country.Id}:0 \"{country.Name}\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteSetupFile(string modPath, IEnumerable<Country> countries)
        {
            string filePath = Path.Combine(modPath, "common", "setup.txt");
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
