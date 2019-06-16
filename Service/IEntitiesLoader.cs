using System.Collections.Generic;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IEntitiesLoader
    {
        IEnumerable<City> LoadCities();

        IEnumerable<string> LoadReligionIds();

        IEnumerable<string> LoadCultureIds();

        IEnumerable<string> LoadGovernmentIds();

        IEnumerable<string> LoadDiplomaticStanceIds();
    }
}
