using NuciExtensions;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class CityGenerator : ICityGenerator
    {
        const int CapitalPopulation = 10;

        const int CityPopulationMin = 4;
        const int CityPopulationMax = 12;

        const int CityCivilizationMin = 0;
        const int CityCivilizationMax = 20;

        const int CityBarbarianLevelMin = 0;
        const int CityBarbarianLevelMax = 0;

        readonly IEntityManager entityManager;
        readonly IRandomNumberGenerator rng;

        public CityGenerator(
            IEntityManager entityManager,
            IRandomNumberGenerator rng)
        {
            this.entityManager = entityManager;
            this.rng = rng;
        }

        public City GenerateCapital(Country country)
        {
            City city = GenerateCity(country.CapitalId);

            SetCityPopulation(city, CapitalPopulation);

            city.CultureId = country.CultureId;
            city.ReligionId = country.ReligionId;

            return city;
        }

        public City GenerateCity(string id)
        {
            City city = entityManager.GetCity(id);

            SetCityPopulation(city, CityPopulationMin, CityPopulationMax);

            city.CultureId = entityManager.GetCultureIds().GetRandomElement(rng.Randomiser);
            city.ReligionId = entityManager.GetReligionIds().GetRandomElement(rng.Randomiser);

            city.CivilizationLevel = rng.Get(CityCivilizationMin, CityCivilizationMax);
            city.BarbarianLevel = rng.Get(CityBarbarianLevelMin, CityBarbarianLevelMax);

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
