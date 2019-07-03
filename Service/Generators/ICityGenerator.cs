using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service.Generators
{
    public interface ICityGenerator
    {
        City GenerateCapital(Country country);

        City GenerateCity(string id);
    }
}
