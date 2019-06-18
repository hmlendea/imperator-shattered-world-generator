using System;

using Microsoft.Extensions.DependencyInjection;

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
            int seed = GetSeed(args);

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddTransient<IRandomNumberGenerator, RandomNumberGenerator>(s => new RandomNumberGenerator(seed))
                .AddSingleton<IEntityLoader, EntitiesLoader>(s => new EntitiesLoader(gameDirectory))
                .AddSingleton<IEntityManager, EntityManager>()
                .AddSingleton<ICityGenerator, CityGenerator>()
                .AddSingleton<ICountryGenerator, CountryGenerator>()
                .AddSingleton<IModWriter, ModWriter>()
                .AddSingleton<IShatteredWorldGenerator, ShatteredWorldGenerator>()
                .BuildServiceProvider();

            IShatteredWorldGenerator generator = serviceProvider.GetService<IShatteredWorldGenerator>();

            Console.WriteLine($"Generating a shattered world using the seed '{seed}'...");
            generator.Generate(modName);
            Console.WriteLine($"Done!");
        }

        static int GetSeed(string[] args)
        {
            if (CliArgumentsReader.HasOption(args, SeedOptions))
            {
                return int.Parse(CliArgumentsReader.GetOptionValue(args, SeedOptions));
            }
            else
            {
                return new Random().Next();
            }
        }
    }
}
