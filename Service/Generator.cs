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
    public sealed class Generator
    {
        const int CityPopulationMin = 4;
        const int CityPopulationMax = 8;
        
        readonly IEntitiesLoader entitiesLoader;

        readonly Random random;

        IDictionary<string, City> cities;
        IList<Country> countries;

        IList<string> religionIds;
        IList<string> cultureIds;
        IList<string> governmentIds;
        IList<string> diplomaticStanceIds;

        public Generator(IEntitiesLoader entitiesLoader)
        {
            this.entitiesLoader = entitiesLoader;

            random = new Random();

            LoadEntities();
        }

        public void Generate()
        {
            ProcessCities();
            GenerateCountries();
        }

        public void Save(string modDirectory)
        {
            string modCommonDirectory = Path.Combine(modDirectory, "common");
            string modCountriesDirectory = Path.Combine(modCommonDirectory, "countries");
            string modLocalisationDirectory = Path.Combine(modDirectory, "localization", "english");

            string modProvinceSetupFilePath = Path.Combine(modCommonDirectory, "province_setup.csv");
            string countriesTxtPath = Path.Combine(modCommonDirectory, "countries.txt");
            string countriesSetupPath = Path.Combine(modCommonDirectory, "setup.txt");
            string countriesLocalisationPath = Path.Combine(modLocalisationDirectory, $"countries_l_english.yml");

            string countriesTxtContent =
                "REB = \"countries/rebels.txt\"" + Environment.NewLine +
                "PIR = \"countries/pirates.txt\"" + Environment.NewLine +
                "BAR = \"countries/barbarians.txt\"" + Environment.NewLine +
                "MER = \"countries/mercenaries.txt\"" + Environment.NewLine;
            string countriesSetupContent = "country = { countries = {" + Environment.NewLine;
            string countriesLocalisationContent = "﻿l_english:" + Environment.NewLine;

            if (Directory.Exists(modDirectory))
            {
                Directory.Delete(modDirectory, true);
            }
            
            Directory.CreateDirectory(modCountriesDirectory);
            Directory.CreateDirectory(modLocalisationDirectory);

            IRepository<CityEntity> modCityRepository = new CsvRepository<CityEntity>(modProvinceSetupFilePath);

            foreach (City city in cities.Values)
            {
                modCityRepository.Add(city.ToDataObject());
            }

            foreach (Country country in countries)
            {
                string countryFileName = $"{country.Id}.txt";
                string countryFilePath = Path.Combine(modCountriesDirectory, countryFileName);

                string countryTxtDefinition = $"{country.Id} = \"countries/{countryFileName}\"{Environment.NewLine}";

                string countryFileContent =
                    $"color = {{ {country.ColourRed} {country.ColourGreen} {country.ColourBlue} }}{Environment.NewLine}" +
                    $"ship_names = {{ {string.Join(' ', country.ShipNames)} }}{Environment.NewLine}";

                string countrySetupDefinition =
                    $"{country.Id} = {{" + Environment.NewLine +
                    $"    government = {country.GovernmentId}" + Environment.NewLine +
                    $"    diplomatic_stance = {country.DiplomaticStanceId}" + Environment.NewLine +
                    $"    primary_culture = {country.CultureId}" + Environment.NewLine +
                    $"    religion = {country.ReligionId}" + Environment.NewLine +
                    $"    capital = {country.CapitalId}" + Environment.NewLine +
                    $"    own_control_core = {{ {country.CapitalId} }}" + Environment.NewLine +
                    $"}}" + Environment.NewLine;
                
                string countryLocalisationContent =
                    $" {country.Id}:0 \"{country.Name}\"{Environment.NewLine}";

                countriesTxtContent += countryTxtDefinition;
                countriesSetupContent += countrySetupDefinition;
                countriesLocalisationContent += countryLocalisationContent;
                File.WriteAllText(countryFilePath, countryFileContent);
            }

            countriesSetupContent += "} }";
            countriesLocalisationContent += Environment.NewLine;

            modCityRepository.ApplyChanges();
            
            File.WriteAllText(countriesTxtPath, countriesTxtContent);
            File.WriteAllText(countriesSetupPath, countriesSetupContent);
            SaveUnicodeFile(countriesLocalisationPath, countriesLocalisationContent);
        }

        void LoadEntities()
        {
            cities = entitiesLoader.LoadCities().ToDictionary(x => x.Id, x => x);
            religionIds = entitiesLoader.LoadReligionIds().ToList();
            cultureIds = entitiesLoader.LoadCultureIds().ToList();
            governmentIds = entitiesLoader.LoadGovernmentIds().ToList();
            diplomaticStanceIds = entitiesLoader.LoadDiplomaticStanceIds().ToList();
        }

        void ProcessCities()
        {
            foreach (City city in cities.Values)
            {
                SetCityPopulation(city);
                city.ReligionId = religionIds.GetRandomElement();
                city.CultureId = cultureIds.GetRandomElement();
            }
        }

        void GenerateCountries()
        {
            countries = new List<Country>();

            IList<string> validCityIds = cities.Values.Where(x => x.TotalPopulation > 0).Select(x => x.Id).ToList();

            for (int i = 0; i < 1250; i++)
            {
                City city = cities[validCityIds.GetRandomElement()];
                Country country = new Country();

                country.Id = GenerateCountryId(city.NameId);
                country.Name = city.NameId;

                country.CultureId = city.CultureId;
                country.ReligionId = city.ReligionId;

                country.GovernmentId = governmentIds.GetRandomElement();
                country.DiplomaticStanceId = diplomaticStanceIds.GetRandomElement();
                country.CapitalId = city.Id;

                country.ColourRed = random.Next(0, 256);
                country.ColourGreen = random.Next(0, 256);
                country.ColourBlue = random.Next(0, 256);

                countries.Add(country);
                validCityIds.Remove(city.Id);
            }
        }

        string GenerateCountryId(string capitalName)
        {
            string id = null;
            
            if (capitalName.Length == 2)
            {
                capitalName.Substring(0, 2).ToUpper();
                capitalName += capitalName[1];
            }
            else
            {
                capitalName.Substring(0, 3).ToUpper();
            }

            while (countries.Any(x => x.Id == id) ||
                   string.IsNullOrWhiteSpace(id) ||
                   id.Length != 3)
            {
                id = string.Empty;

                for (int i = 0; i < 3; i++)
                {
                    id += (char)(random.Next(65, 91));
                }
            }

            return id;
        }

        void SetCityPopulation(City city)
        {
            city.CitizensCount = 0;
            city.FreemenCount = 0;
            city.TribesmenCount = 0;
            city.SlavesCount = 0;

            if (string.IsNullOrWhiteSpace(city.TradeGoodId) ||
                string.IsNullOrWhiteSpace(city.CultureId) ||
                string.IsNullOrWhiteSpace(city.ReligionId) ||
                string.IsNullOrWhiteSpace(city.NameId))
            {
                return;
            }

            int populationCount = random.Next(CityPopulationMin, CityPopulationMax + 1);

            if (city.TotalPopulation == populationCount)
            {
                return;
            }

            city.CitizensCount = 0;
            city.FreemenCount = 0;
            city.TribesmenCount = 0;
            city.SlavesCount = 0;

            for (int i = 0; i < populationCount; i++)
            {
                int randomPop = random.Next(0, 4);

                if (randomPop == 0)
                {
                    city.SlavesCount += 1;
                }
                else if (randomPop == 1)
                {
                    city.TribesmenCount += 1;
                }
                else if (randomPop == 2)
                {
                    city.FreemenCount += 1;
                }
                else if (randomPop == 3)
                {
                    city.CitizensCount += 1;
                }
            }
        }

        void SaveUnicodeFile(string path, string content)
        {
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                writer.Write(content.Substring(1));
            }
        }
    }
}
