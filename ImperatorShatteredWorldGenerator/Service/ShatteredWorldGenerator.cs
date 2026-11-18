using System.Collections.Generic;
using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class ShatteredWorldGenerator(
        IEntityManager entityManager,
        ICityGenerator cityGenerator,
        ICountryGenerator countryGenerator,
        IModWriter modWriter,
        IRandomNumberGenerator rng,
        GeneratorSettings settings) : IShatteredWorldGenerator
    {
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
            if (settings.RandomCountriesCount == 0)
            {
                return;
            }

            IList<string> validCityIds = entityManager
                .GetCities()
                .Where(city =>
                    city.IsHabitable &&
                    entityManager.GetCountries().All(country =>
                        country.CapitalId != city.Id &&
                        country.Name != city.NameId))
                .GroupBy(x => x.NameId)
                .Select(g => g.ToList().GetRandomElement(rng.Randomiser))
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
                if (string.IsNullOrWhiteSpace(country.CapitalId) || entityManager.GetCity(country.CapitalId) is null)
                {
                    continue;
                }

                City capital = cityGenerator.GenerateCapital(country);
                entityManager.UpdateCity(capital);
            }
        }
    }
}
