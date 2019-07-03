using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service.Generators
{
    public interface ICountryGenerator
    {
        Country GenerateCountry(string capitalCityId);
    }
}
