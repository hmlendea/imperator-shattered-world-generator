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
        
        readonly IEntityGenerator entityGenerator;
        readonly IEntitiesLoader entitiesLoader;
        readonly IModWriter modWriter;

        readonly IRandomNumberGenerator rng;

        IDictionary<string, City> cities;
        IList<Country> countries;

        IList<string> religionIds;
        IList<string> cultureIds;
        IList<string> governmentIds;
        IList<string> diplomaticStanceIds;

        public Generator(
            IEntityGenerator entityGenerator,
            IEntitiesLoader entitiesLoader,
            IModWriter modWriter,
            IRandomNumberGenerator rng)
        {
            this.entityGenerator = entityGenerator;
            this.entitiesLoader = entitiesLoader;
            this.modWriter = modWriter;
            this.rng = rng;

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
                city.ReligionId = religionIds.GetRandomElement(rng.Randomiser);
                city.CultureId = cultureIds.GetRandomElement(rng.Randomiser);
                city.CivilizationLevel = rng.Get(CityCivilizationMin, CityCivilizationMax);
                //city.BarbarianLevel = rng.Get(CityBarbarianLevelMin, CityBarbarianLevelMax);
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
                City city = cities[validCityIds.GetRandomElement(rng.Randomiser)];
                Country country = new Country();

                country.Id = entityGenerator.GenerateCountryId(countries, city.NameId);
                country.Name = city.NameId;

                country.CultureId = city.CultureId;
                country.ReligionId = city.ReligionId;

                country.GovernmentId = governmentIds.GetRandomElement(rng.Randomiser);
                country.DiplomaticStanceId = diplomaticStanceIds.GetRandomElement(rng.Randomiser);
                country.CentralisationLevel = rng.Get(CountryCentralisationMin, CountryCentralisationMax);
                country.CapitalId = city.Id;

                country.ColourRed = rng.Get(0, 255);
                country.ColourGreen = rng.Get(0, 255);
                country.ColourBlue = rng.Get(0, 255);

                countries.Add(country);
                validCityIds.Remove(city.Id);
            }
        }

        void SetCityPopulation(City city)
        {
            city.CitizensCount = 0;
            city.FreemenCount = 0;
            city.TribesmenCount = 0;
            city.SlavesCount = 0;

            int populationCount = rng.Get(CityPopulationMin, CityPopulationMax);

            if (city.TotalPopulation == populationCount)
            {
                return;
            }

            for (int i = 0; i < populationCount; i++)
            {
                int randomPop = rng.Get(0, 3);

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
