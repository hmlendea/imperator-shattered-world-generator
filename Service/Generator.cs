using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;
using NuciExtensions;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Mapping;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class Generator
    {
        const int CityPopulationCount = 4;
        
        readonly IEntitiesLoader entitiesLoader;

        readonly Random random;

        IList<City> cities;
        IList<string> religionIds;
        IList<string> cultureIds;

        public Generator(IEntitiesLoader entitiesLoader, string modDirectory)
        {
            this.entitiesLoader = entitiesLoader;

            random = new Random();

            LoadEntities();
        }

        public void Generate()
        {
            ProcessCities();
        }

        public void Save(string modDirectory)
        {
            string modCommonDirectory = Path.Combine(modDirectory, "common");
            string modProvinceSetupFilePath = Path.Combine(modCommonDirectory, "province_setup.csv");

            if (Directory.Exists(modDirectory))
            {
                Directory.Delete(modDirectory, true);
            }
            
            Directory.CreateDirectory(modCommonDirectory);

            IRepository<CityEntity> modCityRepository = new CsvRepository<CityEntity>(modProvinceSetupFilePath);

            foreach (City city in cities)
            {
                modCityRepository.Add(city.ToDataObject());
            }

            modCityRepository.ApplyChanges();
        }

        void LoadEntities()
        {
            cities = entitiesLoader.LoadCities().ToList();
            religionIds = entitiesLoader.LoadReligionIds().ToList();
            cultureIds = entitiesLoader.LoadCultureIds().ToList();
        }

        void ProcessCities()
        {
            foreach (City city in cities)
            {
                SetCityPopulation(city);
                city.ReligionId = religionIds.GetRandomElement();
                city.CultureId = cultureIds.GetRandomElement();
            }
        }

        void SetCityPopulation(City city)
        {
            if (city.TotalPopulation <= CityPopulationCount)
            {
                return;
            }

            city.CitizensCount = 0;
            city.FreemenCount = 0;
            city.TribesmenCount = 0;
            city.SlavesCount = 0;

            for (int i = 0; i < CityPopulationCount; i++)
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
