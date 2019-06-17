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
        const int CityPopulationMax = 12;

        const int CountryCentralisationMin = 0;
        const int CountryCentralisationMax = 100;
        
        readonly IEntitiesLoader entitiesLoader;
        readonly IModWriter modWriter;

        readonly Random random;

        IDictionary<string, City> cities;
        IList<Country> countries;

        IList<string> religionIds;
        IList<string> cultureIds;
        IList<string> governmentIds;
        IList<string> diplomaticStanceIds;

        public Generator(IEntitiesLoader entitiesLoader, IModWriter modWriter, int seed)
        {
            this.entitiesLoader = entitiesLoader;
            this.modWriter = modWriter;

            random = new Random(seed);

            LoadEntities();
        }

        public void Generate(string modDirectory)
        {
            ProcessCities();
            GenerateCountries();

            modWriter.CreateMod(modDirectory, cities.Values, countries);
        }

        void LoadEntities()
        {
            cities = entitiesLoader.LoadCities().ToDictionary(x => x.Id, x => x);
            countries = entitiesLoader.LoadCountries().ToList();
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
                city.ReligionId = religionIds.GetRandomElement(random);
                city.CultureId = cultureIds.GetRandomElement(random);
            }

            foreach (Country country in countries)
            {
                cities[country.CapitalId].CultureId = country.CultureId;
                cities[country.CapitalId].ReligionId = country.ReligionId;
            }
        }

        void GenerateCountries()
        {
            IList<string> validCityIds = cities.Values
                .Where(city => city.TotalPopulation > 0 &&
                               countries.All(country => country.CapitalId != city.Id))
                .Select(x => x.Id)
                .ToList();

            for (int i = 0; i < 1000; i++)
            {
                City city = cities[validCityIds.GetRandomElement(random)];
                Country country = new Country();

                country.Id = GenerateCountryId(city.NameId);
                country.Name = city.NameId;

                country.CultureId = city.CultureId;
                country.ReligionId = city.ReligionId;

                country.GovernmentId = governmentIds.GetRandomElement(random);
                country.DiplomaticStanceId = diplomaticStanceIds.GetRandomElement(random);
                country.CentralisationLevel = random.Next(CountryCentralisationMin, CountryCentralisationMax);
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
            const string AllowedCapitalIdCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            string id = null;
            string normalisedCapitalName = capitalName
                .RemovePunctuation()
                .Replace(" ", "")
                .ToUpper();
            
            if (normalisedCapitalName.Length == 2)
            {
                id = normalisedCapitalName.Substring(0, 2).ToUpper();
                id += normalisedCapitalName[1];
            }
            else
            {
                for (int i = 0; i < normalisedCapitalName.Length; i++)
                {
                    for (int j = i + 1; j < normalisedCapitalName.Length; j++)
                    {
                        for (int k = j + 1; k < normalisedCapitalName.Length; k++)
                        {
                            id = $"{normalisedCapitalName[i]}{normalisedCapitalName[j]}{normalisedCapitalName[k]}";

                            if (countries.All(x => x.Id != id) && id.All(AllowedCapitalIdCharacters.Contains))
                            {
                                return id;
                            }
                        }
                    }
                }
            }

            while (countries.Any(x => x.Id == id) ||
                   string.IsNullOrWhiteSpace(id) ||
                   id.Length != 3)
            {
                id = string.Empty;

                for (int i = 0; i < 3; i++)
                {
                    id += AllowedCapitalIdCharacters.GetRandomElement(random);
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
    }
}
