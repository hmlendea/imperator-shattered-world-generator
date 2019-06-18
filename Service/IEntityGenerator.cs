using System.Collections.Generic;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IEntityGenerator
    {
        string GenerateCountryId(IEnumerable<Country> countries, string name);
    }
}
