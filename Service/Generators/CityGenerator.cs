using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service.Generators
{
    public sealed class CityGenerator : ICityGenerator
    {
        readonly IEntityManager entityManager;
        readonly IRandomNumberGenerator rng;
        readonly GeneratorSettings settings;

        public CityGenerator(
            IEntityManager entityManager,
            IRandomNumberGenerator rng,
            GeneratorSettings settings)
        {
            this.entityManager = entityManager;
            this.rng = rng;
            this.settings = settings;
        }

        public City GenerateCapital(Country country)
        {
            City city = GenerateCity(country.CapitalId);

            SetCityPopulation(city, settings.CapitalPopulation);

            city.CultureId = country.CultureId;
            city.ReligionId = country.ReligionId;

            return city;
        }

        public City GenerateCity(string id)
        {
            City city = entityManager.GetCity(id);

            SetCityPopulation(city, settings.CityPopulationMin, settings.CityPopulationMax);

            city.CultureId = entityManager.GetCultureIds().GetRandomElement(rng.Randomiser);
            city.ReligionId = entityManager.GetReligionIds().GetRandomElement(rng.Randomiser);

            city.CivilizationLevel = rng.Get(settings.CityBarbarianLevelMin, settings.CityCivilisationLevelMax);
            city.BarbarianLevel = rng.Get(settings.CityBarbarianLevelMin, settings.CityBarbarianLevelMax);

            return city;
        }

        void SetCityPopulation(City city, int amountMin, int amountMax)
            => SetCityPopulation(city, rng.Get(amountMin, amountMax));

        void SetCityPopulation(City city, int amount)
        {
            if (city.TotalPopulation == amount)
            {
                return;
            }

            city.CitizensCount = 0;
            city.FreemenCount = 0;
            city.TribesmenCount = 0;
            city.SlavesCount = 0;

            for (int i = 0; i < amount; i++)
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
