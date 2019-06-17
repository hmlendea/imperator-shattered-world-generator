using System;
using System.Collections.Generic;
using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class Generator
    {
        const int CityPopulationMin = 4;
        const int CityPopulationMax = 12;

        const int CityCivilizationMin = 0;
        const int CityCivilizationMax = 20;

        const int CityBarbarianLevelMin = 0;
        const int CityBarbarianLevelMax = 5;

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
            foreach (City city in cities.Values.Where(c => c.IsHabitable))
            {
                SetCityPopulation(city);
                city.ReligionId = religionIds.GetRandomElement(random);
                city.CultureId = cultureIds.GetRandomElement(random);
                city.CivilizationLevel = GetRandomNumber(CityCivilizationMin, CityCivilizationMax);
                //city.BarbarianLevel = GetRandomNumber(CityBarbarianLevelMin, CityBarbarianLevelMax);
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
                .Where(city =>
                    city.IsHabitable &&
                    countries.All(country =>
                        country.CapitalId != city.Id &&
                        country.Name != city.NameId))
                .GroupBy(x => x.NameId)
                .Select(g => g.GetRandomElement())
                .Select(x => x.Id)
                .ToList();

            for (int i = 0; i < 1500; i++)
            {
                City city = cities[validCityIds.GetRandomElement(random)];
                Country country = new Country();

                country.Id = GenerateCountryId(city.NameId);
                country.Name = city.NameId;

                country.CultureId = city.CultureId;
                country.ReligionId = city.ReligionId;

                country.GovernmentId = governmentIds.GetRandomElement(random);
                country.DiplomaticStanceId = diplomaticStanceIds.GetRandomElement(random);
                country.CentralisationLevel = GetRandomNumber(CountryCentralisationMin, CountryCentralisationMax);
                country.CapitalId = city.Id;

                country.ColourRed = GetRandomNumber(0, 255);
                country.ColourGreen = GetRandomNumber(0, 255);
                country.ColourBlue = GetRandomNumber(0, 255);

                countries.Add(country);
                validCityIds.Remove(city.Id);
            }
        }

        int GetRandomNumber(int min, int max)
        {
            return random.Next(min, max + 1);
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

            int populationCount = GetRandomNumber(CityPopulationMin, CityPopulationMax);

            if (city.TotalPopulation == populationCount)
            {
                return;
            }

            for (int i = 0; i < populationCount; i++)
            {
                int randomPop = GetRandomNumber(0, 3);

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
