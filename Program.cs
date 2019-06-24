using System;

using Microsoft.Extensions.DependencyInjection;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {

        static void Main(string[] args)
        {
            GeneratorSettings settings = GeneratorSettings.LoadFromArguments(args);

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
    }
}
