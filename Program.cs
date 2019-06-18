using System;

using NuciCLI;

using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        static string[] ImperatorDirectoryPathOptions = new string[] { "-d", "--dir", "--game", "--imperator" };
        static string[] ModNameOptions = new string[] { "-n", "--name" };
        static string[] SeedOptions = new string[] { "-s", "--seed" };

        static void Main(string[] args)
        {
            string gameDirectory = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            string modName = CliArgumentsReader.GetOptionValue(args, ModNameOptions);

            int seed;

            if (CliArgumentsReader.HasOption(args, SeedOptions))
            {
                seed = int.Parse(CliArgumentsReader.GetOptionValue(args, SeedOptions));
            }
            else
            {
                seed = new Random().Next();
            }

            IRandomNumberGenerator rng = new RandomNumberGenerator(seed);
            IEntityGenerator entityGenerator = new EntityGenerator(rng);
            IEntitiesLoader entitiesLoader = new EntitiesLoader(gameDirectory);
            IModWriter modWriter = new ModWriter();

            Generator generator = new Generator(entityGenerator, entitiesLoader, modWriter, rng);

            Console.WriteLine($"Generating a shattered world using the seed '{rng.Seed}'...");
            generator.Generate(modName);
            Console.WriteLine($"Done!");
        }
    }
}
