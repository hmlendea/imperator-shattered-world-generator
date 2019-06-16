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
        static string[] OutputModPathOptions = new string[] { "-o", "--out", "--output" };

        static void Main(string[] args)
        {
            string imperatorDirectoryPath = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            string outputModPath = CliArgumentsReader.GetOptionValue(args, OutputModPathOptions);

            string vanillaProvinceSetupFilePath = Path.Combine(imperatorDirectoryPath, "game", "common", "province_setup.csv");

            CsvRepository<CityEntity> vanillaCityRepository = new CsvRepository<CityEntity>(vanillaProvinceSetupFilePath);

            Generator generator = new Generator(vanillaCityRepository);

            generator.Generate();
            generator.Save(outputModPath);
        }
    }
}
