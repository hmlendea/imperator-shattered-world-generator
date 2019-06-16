using System.Collections.Generic;

using ImperatorShatteredWorldGenerator.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IEntitiesLoader
    {
        IEnumerable<City> LoadCities();

        IEnumerable<string> LoadReligionIds();
    }
}
