using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface ICityGenerator
    {
        City GenerateCapital(Country country);

        City GenerateCity(string id);
    }
}
