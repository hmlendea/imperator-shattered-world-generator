using System.Collections.Generic;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public interface IEntityManager
    {
        City GetCity(string id);

        IEnumerable<City> GetCities();

        IEnumerable<string> GetCityIds();

        void UpdateCity(City city);

        void AddCountry(Country country);

        Country GetCountry(string id);

        IEnumerable<Country> GetCountries();

        IEnumerable<string> GetCultureIds();

        IEnumerable<string> GetReligionIds();

        IEnumerable<string> GetGovernmentIds();

        IEnumerable<string> GetDiplomaticStanceIds();
    }
}
