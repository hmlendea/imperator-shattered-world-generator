using System.Collections.Generic;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IModWriter
    {
        void CreateMod(string path, IEnumerable<City> cities, IEnumerable<Country> countries);
    }
}
