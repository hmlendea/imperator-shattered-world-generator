using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using NuciCLI.Arguments;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        private static readonly string[] ImperatorDirectoryPathOptions = ["dir", "game", "imperator"];
        private static readonly string[] ModNameOptions = ["name"];
        private static readonly string[] SeedOptions = ["seed"];
        private static readonly string[] CapitalPopulationOptions = ["capital-population", "capital-pops"];
        private static readonly string[] CityPopulationMinOptions = ["city-population-min", "city-pops-min"];
        private static readonly string[] CityPopulationMaxOptions = ["city-population-max", "city-pops-max"];
        private static readonly string[] CityCivilisationLevelMinOptions = ["city-civilisation-min", "city-civ-min"];
        private static readonly string[] CityCivilisationLevelMaxOptions = ["city-civilisation-max", "city-civ-max"];
        private static readonly string[] CityBarbarianLevelMinOptions = ["city-barbarian-min", "city-barb-min"];
        private static readonly string[] CityBarbarianLevelMaxOptions = ["city-barbarian-max", "city-barb-max"];
        private static readonly string[] CountryCentralisationLevelMinOptions = ["country-centralisation-min", "country-cent-min"];
        private static readonly string[] CountryCentralisationLevelMaxOptions = ["country-centralisation-max", "country-cent-max"];
        private static readonly string[] RandomCountriesCountOptions = ["random-countries", "new-countries", "rng-countries"];

        private static readonly IDictionary<string, string> ShortOptionAliases = new Dictionary<string, string>
        {
            { "-d", "--dir" },
            { "-n", "--name" },
            { "-s", "--seed" }
        };

        private static void Main(string[] args)
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

        private static GeneratorSettings LoadSettings(string[] args)
        {
            ArgumentParser parser = new ArgumentParser();

            RegisterArguments(parser, ImperatorDirectoryPathOptions);
            RegisterArguments(parser, ModNameOptions);
            RegisterArguments(parser, SeedOptions);
            RegisterArguments(parser, CapitalPopulationOptions);
            RegisterArguments(parser, CityPopulationMinOptions);
            RegisterArguments(parser, CityPopulationMaxOptions);
            RegisterArguments(parser, CityCivilisationLevelMinOptions);
            RegisterArguments(parser, CityCivilisationLevelMaxOptions);
            RegisterArguments(parser, CityBarbarianLevelMinOptions);
            RegisterArguments(parser, CityBarbarianLevelMaxOptions);
            RegisterArguments(parser, CountryCentralisationLevelMinOptions);
            RegisterArguments(parser, CountryCentralisationLevelMaxOptions);
            RegisterArguments(parser, RandomCountriesCountOptions);

            ArgumentsCollection arguments = parser.ParseArgs(NormaliseShortArguments(args));
            GeneratorSettings settings = new GeneratorSettings();

            settings.GameDirectoryPath = GetRequiredArgumentValue(arguments, ImperatorDirectoryPathOptions);
            settings.ModName = GetRequiredArgumentValue(arguments, ModNameOptions);
            settings.Seed = GetIntegerArgumentValue(arguments, settings.Seed, SeedOptions);
            settings.CapitalPopulation = GetIntegerArgumentValue(arguments, settings.CapitalPopulation, CapitalPopulationOptions);
            settings.CityPopulationMin = GetIntegerArgumentValue(arguments, settings.CityPopulationMin, CityPopulationMinOptions);
            settings.CityPopulationMax = GetIntegerArgumentValue(arguments, settings.CityPopulationMax, CityPopulationMaxOptions);
            settings.CityCivilisationLevelMin = GetIntegerArgumentValue(arguments, settings.CityCivilisationLevelMin, CityCivilisationLevelMinOptions);
            settings.CityCivilisationLevelMax = GetIntegerArgumentValue(arguments, settings.CityCivilisationLevelMax, CityCivilisationLevelMaxOptions);
            settings.CityBarbarianLevelMin = GetIntegerArgumentValue(arguments, settings.CityBarbarianLevelMin, CityBarbarianLevelMinOptions);
            settings.CityBarbarianLevelMax = GetIntegerArgumentValue(arguments, settings.CityBarbarianLevelMax, CityBarbarianLevelMaxOptions);
            settings.CountryCentralisationLevelMin = GetIntegerArgumentValue(arguments, settings.CountryCentralisationLevelMin, CountryCentralisationLevelMinOptions);
            settings.CountryCentralisationLevelMax = GetIntegerArgumentValue(arguments, settings.CountryCentralisationLevelMax, CountryCentralisationLevelMaxOptions);
            settings.RandomCountriesCount = GetIntegerArgumentValue(arguments, settings.RandomCountriesCount, RandomCountriesCountOptions);

            return settings;
        }

        private static void RegisterArguments(ArgumentParser parser, params string[] names)
        {
            foreach (string name in names)
            {
                parser.AddArgument(name);
            }
        }

        private static string[] NormaliseShortArguments(string[] args)
        {
            List<string> normalisedArguments = new List<string>();

            foreach (string argument in args)
            {
                string name = argument;
                string value = null;

                int separatorIndex = argument.IndexOf('=');

                if (separatorIndex >= 0)
                {
                    name = argument.Substring(0, separatorIndex);
                    value = argument.Substring(separatorIndex + 1);
                }

                if (ShortOptionAliases.TryGetValue(name, out string alias))
                {
                    name = alias;
                }

                normalisedArguments.Add(name);

                if (value is not null)
                {
                    normalisedArguments.Add(value);
                }
            }

            return [.. normalisedArguments];
        }

        private static string GetArgumentValue(ArgumentsCollection arguments, params string[] names)
        {
            foreach (string name in names)
            {
                string value = arguments[name] as string;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string GetRequiredArgumentValue(ArgumentsCollection arguments, params string[] names)
        {
            string value = GetArgumentValue(arguments, names);

            if (value is null)
            {
                throw new ArgumentException($"Missing required argument: --{names[0]}");
            }

            return value;
        }

        private static int GetIntegerArgumentValue(ArgumentsCollection arguments, int defaultValue, params string[] names)
        {
            string value = GetArgumentValue(arguments, names);

            if (value is null)
            {
                return defaultValue;
            }

            return int.Parse(value);
        }
    }
}
