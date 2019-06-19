using System.Collections.Generic;
using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ShatteredWorldGenerator : IShatteredWorldGenerator
    {
        readonly IEntityManager entityManager;
        readonly ICityGenerator cityGenerator;
        readonly ICountryGenerator countryGenerator;
        readonly IModWriter modWriter;
        readonly IRandomNumberGenerator rng;
        readonly GeneratorSettings settings;

        public ShatteredWorldGenerator(
            IEntityManager entityManager,
            ICityGenerator cityGenerator,
            ICountryGenerator countryGenerator,
            IModWriter modWriter,
            IRandomNumberGenerator rng,
            GeneratorSettings settings)
        {
            this.entityManager = entityManager;
            this.cityGenerator = cityGenerator;
            this.countryGenerator = countryGenerator;
            this.modWriter = modWriter;
            this.rng = rng;
            this.settings = settings;
        }

        public void Generate()
        {
            GenerateCities();
            GenerateCountries();
            GenerateCapitals();

            modWriter.CreateMod();
        }

        void GenerateCities()
        {
            foreach (string cityId in entityManager.GetCityIds())
            {
                City city = cityGenerator.GenerateCity(cityId);
                entityManager.UpdateCity(city);
            }
        }

        void GenerateCountries()
        {
            IList<string> validCityIds = entityManager
                .GetCities()
                .Where(city =>
                    city.IsHabitable &&
                    entityManager.GetCountries().All(country =>
                        country.CapitalId != city.Id &&
                        country.Name != city.NameId))
                .GroupBy(x => x.NameId)
                .Select(g => g.GetRandomElement(rng.Randomiser))
                .Select(x => x.Id)
                .ToList();

            for (int i = 0; i < settings.RandomCountriesCount; i++)
            {
                string cityId = validCityIds.GetRandomElement(rng.Randomiser);
                Country country = countryGenerator.GenerateCountry(cityId);
                
                entityManager.AddCountry(country);
                validCityIds.Remove(cityId);
            }
        }
        
        void GenerateCapitals()
        {
            foreach (Country country in entityManager.GetCountries())
            {
                City capital = cityGenerator.GenerateCapital(country);
                entityManager.UpdateCity(capital);
            }
        }
    }
}
