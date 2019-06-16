using NuciCLI;

using ImperatorShatteredWorldGenerator.Service;

namespace ImperatorShatteredWorldGenerator
{
    class Program
    {
        static string[] ImperatorDirectoryPathOptions = new string[] { "-d", "--dir", "--game", "--imperator" };
        static string[] OutputModPathOptions = new string[] { "-o", "--out", "--output" };

        static void Main(string[] args)
        {
            string gameDirectory = CliArgumentsReader.GetOptionValue(args, ImperatorDirectoryPathOptions);
            string outputModPath = CliArgumentsReader.GetOptionValue(args, OutputModPathOptions);

            IEntitiesLoader entitiesLoader = new EntitiesLoader(gameDirectory);

            Generator generator = new Generator(entitiesLoader);

            generator.Generate();
            generator.Save(outputModPath);
        }
    }
}
