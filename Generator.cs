using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Mapping;
using ImperatorShatteredWorldGenerator.Models;

namespace ImperatorShatteredWorldGenerator
{
    public sealed class Generator
    {
        const int CityPopulationCount = 4;

        readonly IRepository<CityEntity> vanillaCityRepository;

        readonly Random random;
        
        IList<City> cities;

        public Generator(IRepository<CityEntity> vanillaCityRepository)
        {
            this.vanillaCityRepository = vanillaCityRepository;
            
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

        void EnsureFileCompatibility(string filePath)
        {
            string content = File.ReadAllText(filePath);
            content = content.Replace("\n", "\r\n");

            File.WriteAllText(filePath, content);
        }

        void LoadEntities()
        {
            cities = vanillaCityRepository.GetAll().ToServiceModels().ToList();
        }

        void ProcessCities()
        {
            foreach (City city in cities)
            {
                SetCityPopulation(city);
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
