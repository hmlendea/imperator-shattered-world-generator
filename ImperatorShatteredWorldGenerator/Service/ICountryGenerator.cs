using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface ICountryGenerator
    {
        Country GenerateCountry(string capitalCityId);
    }
}
