using System;
using System.IO;

using NuciCLI;
using NuciExtensions;

namespace ImperatorShatteredWorldGenerator.Configuration
{
    public sealed class GeneratorSettings
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

        public int Seed { get; set; }

        public string GameDirectoryPath { get; set; }

        public string ModName { get; set; }

        public string ModId
            =>  ModName
                .RemovePunctuation()
                .RemoveDiacritics()
                .Replace(" ", "-")
                .ToLower();

        public string ModDirectoryPath
            => Path.Combine(Environment.CurrentDirectory, "out", ModId);

        public string ModFilePath
            => ModDirectoryPath + ".mod";
        
        public int CapitalPopulation { get; set; }
        public int CityPopulationMin { get; set; }
        public int CityPopulationMax { get; set; }

        public int CityCivilisationLevelMin { get; set; }
        public int CityCivilisationLevelMax { get; set; }

        public int CityBarbarianLevelMin { get; set; }
        public int CityBarbarianLevelMax { get; set; }

        public int CountryCentralisationLevelMin { get; set; }
        public int CountryCentralisationLevelMax { get; set; }

        public int RandomCountriesCount { get; set; }

        public GeneratorSettings()
        {
            Seed = new Random().Next();

            CapitalPopulation = 10;

            CityPopulationMin = 4;
            CityPopulationMax = 12;

            CityCivilisationLevelMin = 0;
            CityCivilisationLevelMax = 20;

            CityBarbarianLevelMin = 0;
            CityBarbarianLevelMax = 0;

            CountryCentralisationLevelMin = 0;
            CountryCentralisationLevelMax = 100;
        
            RandomCountriesCount = 500;
        }

        public static GeneratorSettings LoadFromArguments(string[] args)
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
