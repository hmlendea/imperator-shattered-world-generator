using System.Collections.Generic;
using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ShatteredWorldGenerator : IShatteredWorldGenerator
    {
        const int RandomCountriesCount = 500;

        readonly IEntityManager entityManager;
        readonly ICityGenerator cityGenerator;
        readonly ICountryGenerator countryGenerator;
        readonly IModWriter modWriter;

        readonly IRandomNumberGenerator rng;

        public ShatteredWorldGenerator(
            IEntityManager entityManager,
            ICityGenerator cityGenerator,
            ICountryGenerator countryGenerator,
            IModWriter modWriter,
            IRandomNumberGenerator rng)
        {
            this.entityManager = entityManager;
            this.cityGenerator = cityGenerator;
            this.countryGenerator = countryGenerator;
            this.modWriter = modWriter;
            this.rng = rng;
        }

        public void Generate(string modDirectory)
        {
            GenerateCities();
            GenerateCountries();
            GenerateCapitals();

            modWriter.CreateMod(modDirectory);
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

            for (int i = 0; i < RandomCountriesCount; i++)
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
