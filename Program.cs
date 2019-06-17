using System;

using NuciCLI;

using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        static string[] ImperatorDirectoryPathOptions = new string[] { "-d", "--dir", "--game", "--imperator" };
        static string[] OutputModPathOptions = new string[] { "-o", "--out", "--output" };
        static string[] SeedOptions = new string[] { "-s", "--seed" };

        static void Main(string[] args)
        {
            string gameDirectory = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            string outputModPath = CliArgumentsReader.GetOptionValue(args, OutputModPathOptions);
            int seed;

            if (CliArgumentsReader.HasOption(args, SeedOptions))
            {
                seed = int.Parse(CliArgumentsReader.GetOptionValue(args, SeedOptions));
            }
            else
            {
                seed = new Random().Next();
            }

            IEntitiesLoader entitiesLoader = new EntitiesLoader(gameDirectory);

            Generator generator = new Generator(entitiesLoader, seed);

            generator.Generate();
            generator.Save(outputModPath);
        }
    }
}
