using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NuciCLI;
using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        static string[] ImperatorDirectoryPathOptions = new string[] { "-d", "--dir", "--game", "--imperator" };
        static string[] OutputModNameOptions = new string[] { "-o", "--out", "--output" };

        static void Main(string[] args)
        {
            string imperatorDirectoryPath = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            string outputModName = CliArgumentsReader.GetOptionValue(args, OutputModNameOptions);

            string vanillaProvinceSetupFilePath = Path.Combine(imperatorDirectoryPath, "game", "common", "province_setup.csv");

            string modCommonDirectory = Path.Combine(outputModName, "common");
            string modProvinceSetupFilePath = Path.Combine(modCommonDirectory, "province_setup.csv");

            if (!Directory.Exists(modCommonDirectory))
            {
                Directory.CreateDirectory(modCommonDirectory);
            }

            if (!File.Exists(modProvinceSetupFilePath))
            {
                File.Create(modProvinceSetupFilePath);
            }

            CsvRepository<CityEntity> vanillaCityRepository = new CsvRepository<CityEntity>(vanillaProvinceSetupFilePath);
            CsvRepository<CityEntity> modCityRepository = new CsvRepository<CityEntity>(modProvinceSetupFilePath);

            Generator generator = new Generator(vanillaCityRepository, modCityRepository);

            generator.Generate();
        }
    }
}
