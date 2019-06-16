using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;

namespace ImperatorShatteredWorldGenerator
{
    public sealed class Generator
    {
        readonly IRepository<CityEntity> vanillaCityRepository;
        readonly IRepository<CityEntity> modCityRepository;

        const int CityCitizensCount = 0;
        const int CityFreemenCount = 0;
        const int CityTribesmenCount = 3;
        const int CitySlavesCount = 0;

        public Generator(
            IRepository<CityEntity> vanillaCityRepository,
            IRepository<CityEntity> modCityRepository)
        {
            this.vanillaCityRepository = vanillaCityRepository;
            this.modCityRepository = modCityRepository;
        }

        public void Generate()
        {
            ProcessCities();

            modCityRepository.ApplyChanges();
        }

        void ProcessCities()
        {
            foreach (CityEntity city in vanillaCityRepository.GetAll())
            {
                Console.WriteLine(city.Id + " " + city.NameId);
                SetCityPopulation(city);
                modCityRepository.Add(city);
            }
        }

        void SetCityPopulation(CityEntity city)
        {
            if (city.CitizensCount > CityCitizensCount)
            {
                city.CitizensCount = CityCitizensCount;
            }

            if (city.FreemenCount > CityFreemenCount)
            {
                city.FreemenCount = CityFreemenCount;
            }

            if (city.TribesmenCount > CityTribesmenCount)
            {
                city.TribesmenCount = CityTribesmenCount;
            }

            if (city.SlavesCount > CitySlavesCount)
            {
                city.SlavesCount = CitySlavesCount;
            }
        }
    }
}
