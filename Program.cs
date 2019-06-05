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

            string provinceSetupFilePath = Path.Combine(imperatorDirectoryPath, "game", "common", "province_setup.csv");
            CsvRepository<CityEntity> cityRepository = new CsvRepository<CityEntity>(provinceSetupFilePath);

            Generator generator = new Generator(cityRepository);
        }
    }
}
