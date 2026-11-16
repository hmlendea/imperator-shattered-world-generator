using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using NuciDAL.Repositories;
using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ModWriter : IModWriter
    {
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

            WriteProvincesSetup();
            WriteCountryFiles();
            WriteCountriesDefinitionIndexFile();
            WriteCountriesLocalisationFile();
            WriteSetupFile();
            WriteEventsFile();
            WriteDefinesFile();
        }

        void PrepareDirectoryStructure()
        {
            string modPath = settings.ModDirectoryPath;
            string modCommonDir = Path.Combine(modPath, "common");
            string modEventsDir = Path.Combine(modPath, "events");
            string modDefinesDir = Path.Combine(modCommonDir, "defines");
            string modCountriesDir = Path.Combine(modCommonDir, "countries");
            string modLocalisationDir = Path.Combine(modPath, "localization");

            if (Directory.Exists(modPath))
            {
                Directory.Delete(modPath, true);
            }

            if (File.Exists(settings.ModFilePath))
            {
                File.Delete(settings.ModFilePath);
            }
            
            Directory.CreateDirectory(modDefinesDir);
            Directory.CreateDirectory(modCountriesDir);
            Directory.CreateDirectory(modEventsDir);
            Directory.CreateDirectory(modLocalisationDir);
        }

        void WriteModDescriptor()
        {
            string descriptorFilePath = Path.Combine(settings.ModDirectoryPath, "descriptor.mod");

            string fileContent =
                $"name = \"{settings.ModName}\"" + Environment.NewLine +
                $"path = \"mod/{settings.ModId}\"";

            WriteFile(settings.ModFilePath, fileContent);
            WriteFile(descriptorFilePath, fileContent);
        }

        void WriteProvincesSetup()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "common", "province_setup.csv");
            string fileContent = "#ProvID;Culture;Religion;TradeGoods;Citizens;Freedmen;Slaves;Tribesmen;Civilization;Barbarian;NameRef;AraRef" + Environment.NewLine;

            IRepository<CityEntity> repo = new CsvRepository<CityEntity>(filePath);

            foreach (City city in entityManager.GetCities())
            {
                repo.Add(city.ToDataObject());

                fileContent +=
                    $"{city.Id}," +
                    $"{city.CultureId}," +
                    $"{city.ReligionId}," +
                    $"{city.TradeGoodId}," +
                    $"{city.CitizensCount}," +
                    $"{city.FreemenCount}," +
                    $"{city.TribesmenCount}," +
                    $"{city.SlavesCount}," +
                    $"{city.CivilizationLevel}," +
                    $"{city.BarbarianLevel}," +
                    $"{city.NameId}," +
                    $"{city.ProvinceId}" + Environment.NewLine;
            }
            
            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteCountryFiles()
        {
            string dirPath = Path.Combine(settings.ModDirectoryPath, "common", "countries");

            foreach (Country country in entityManager.GetCountries())
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

        void WriteCountriesDefinitionIndexFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "common", "countries.txt");
            string fileContent =
                "REB = \"countries/rebels.txt\"" + Environment.NewLine +
                "PIR = \"countries/pirates.txt\"" + Environment.NewLine +
                "BAR = \"countries/barbarians.txt\"" + Environment.NewLine +
                "MER = \"countries/mercenaries.txt\"" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries())
            {
                fileContent += $"{country.Id} = \"countries/{country.Id}.txt\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteCountriesLocalisationFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "localization", "SW_countries_l_english.yml");
            string fileContent = "l_english:" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries().Where(c => !c.IsVanilla))
            {
                fileContent += $" {country.Id}:0 \"{country.Name}\"{Environment.NewLine}";
            }

            WriteUnicodeFile(filePath, fileContent);
        }

        void WriteSetupFile()
        {
            string filePath = Path.Combine(settings.ModDirectoryPath, "common", "setup.txt");
            string fileContent =
                "country = {" + Environment.NewLine +
                "  countries = {" + Environment.NewLine;

            foreach (Country country in entityManager.GetCountries())
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
                "  START_DATE = \"001.10.1\"" + Environment.NewLine +
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
