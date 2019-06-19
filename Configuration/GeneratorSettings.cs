using System;
using System.IO;

using NuciExtensions;

namespace ImperatorShatteredWorldGenerator.Configuration
{
    public sealed class GeneratorSettings
    {
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
    }
}
