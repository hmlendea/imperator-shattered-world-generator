using System;

using Microsoft.Extensions.DependencyInjection;

using NuciCLI;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        static string[] ImperatorDirectoryPathOptions = new string[] { "-d", "--dir", "--game", "--imperator" };
        static string[] ModNameOptions = new string[] { "-n", "--name" };
        static string[] SeedOptions = new string[] { "-s", "--seed" };
        static string[] CapitalPopulationOptions = new string[] { "--capital-population", "--capital-pops" };
        static string[] CityPopulationMinOptions = new string[] { "--city-population-min", "--city-pops-min" };
        static string[] CityPopulationMaxOptions = new string[] { "--city-population-max", "--city-pops-max" };
        static string[] CityCivilisationLevelMinOptions = new string[] { "--city-civilisation-min", "--city-civ-min" };
        static string[] CityCivilisationLevelMaxOptions = new string[] { "--city-civilisation-max", "--city-civ-max" };
        static string[] CityBarbarianLevelMinOptions = new string[] { "--city-barbarian-min", "--city-barb-min" };
        static string[] CityBarbarianLevelMaxOptions = new string[] { "--city-barbarian-max", "--city-barb-max" };
        static string[] CountryCentralisationLevelMinOptions = new string[] { "--country-centralisation-min", "--country-cent-min" };
        static string[] CountryCentralisationLevelMaxOptions = new string[] { "--country-centralisation-max", "--country-cent-max" };
        static string[] RandomCountriesCountOptions = new string[] { "--random-countries", "--new-countries", "--rng-countries" };

        static void Main(string[] args)
        {
            GeneratorSettings settings = LoadSettings(args);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(settings)
                .AddTransient<IRandomNumberGenerator, RandomNumberGenerator>()
                .AddSingleton<IEntityLoader, EntitiesLoader>()
                .AddSingleton<IEntityManager, EntityManager>()
                .AddSingleton<ICityGenerator, CityGenerator>()
                .AddSingleton<ICountryGenerator, CountryGenerator>()
                .AddSingleton<IModWriter, ModWriter>()
                .AddSingleton<IShatteredWorldGenerator, ShatteredWorldGenerator>()
                .BuildServiceProvider();

            IShatteredWorldGenerator generator = serviceProvider.GetService<IShatteredWorldGenerator>();

            Console.WriteLine($"Generating a shattered world using the seed '{settings.Seed}'...");
            generator.Generate();
            Console.WriteLine($"Done!");
        }

        static GeneratorSettings LoadSettings(string[] args)
        {
            GeneratorSettings settings = new GeneratorSettings();

            if (CliArgumentsReader.HasOption(args, SeedOptions))
            {
                settings.Seed = int.Parse(CliArgumentsReader.GetOptionValue(args, SeedOptions));
            }

            settings.GameDirectoryPath = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            settings.ModName = CliArgumentsReader.GetOptionValue(args, ModNameOptions);

            if (CliArgumentsReader.HasOption(args, CapitalPopulationOptions))
            {
                settings.CapitalPopulation = int.Parse(CliArgumentsReader.GetOptionValue(args, CapitalPopulationOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityPopulationMinOptions))
            {
                settings.CityPopulationMin = int.Parse(CliArgumentsReader.GetOptionValue(args, CityPopulationMinOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityPopulationMaxOptions))
            {
                settings.CityPopulationMax = int.Parse(CliArgumentsReader.GetOptionValue(args, CityPopulationMaxOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityCivilisationLevelMinOptions))
            {
                settings.CityCivilisationLevelMin = int.Parse(CliArgumentsReader.GetOptionValue(args, CityCivilisationLevelMinOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityCivilisationLevelMaxOptions))
            {
                settings.CityCivilisationLevelMax = int.Parse(CliArgumentsReader.GetOptionValue(args, CityCivilisationLevelMaxOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityBarbarianLevelMinOptions))
            {
                settings.CityBarbarianLevelMin = int.Parse(CliArgumentsReader.GetOptionValue(args, CityBarbarianLevelMinOptions));
            }

            if (CliArgumentsReader.HasOption(args, CityBarbarianLevelMaxOptions))
            {
                settings.CityBarbarianLevelMax = int.Parse(CliArgumentsReader.GetOptionValue(args, CityBarbarianLevelMaxOptions));
            }

            if (CliArgumentsReader.HasOption(args, CountryCentralisationLevelMinOptions))
            {
                settings.CountryCentralisationLevelMin = int.Parse(CliArgumentsReader.GetOptionValue(args, CountryCentralisationLevelMinOptions));
            }

            if (CliArgumentsReader.HasOption(args, CountryCentralisationLevelMaxOptions))
            {
                settings.CountryCentralisationLevelMax = int.Parse(CliArgumentsReader.GetOptionValue(args, CountryCentralisationLevelMaxOptions));
            }

            if (CliArgumentsReader.HasOption(args, RandomCountriesCountOptions))
            {
                settings.RandomCountriesCount = int.Parse(CliArgumentsReader.GetOptionValue(args, RandomCountriesCountOptions));
            }

            return settings;
        }
    }
}
