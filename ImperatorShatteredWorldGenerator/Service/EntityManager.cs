using System.Collections.Generic;
using System.Linq;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class EntityManager : IEntityManager
    {
        readonly IEntityLoader entityLoader;

        IDictionary<string, City> cities;
        IDictionary<string, Country> countries;

        IEnumerable<string> cultureIds;
        IEnumerable<string> religionIds;
        IEnumerable<string> governmentIds;
        IEnumerable<string> diplomaticStanceIds;

        public EntityManager(IEntityLoader entityLoader)
        {
            this.entityLoader = entityLoader;
        }

        public City GetCity(string id)
        {
            if (cities is null)
            {
                cities = entityLoader.LoadCities().ToDictionary(x => x.Id, x => x);
            }

            if (!cities.ContainsKey(id))
            {
                return null;
            }

            return cities[id];
        }

        public IEnumerable<City> GetCities()
        {
            if (cities is null)
            {
                cities = entityLoader.LoadCities().ToDictionary(x => x.Id, x => x);
            }

            return cities.Values;
        }

        public IEnumerable<string> GetCityIds()
        {
            return GetCities().Select(x => x.Id).ToList();
        }

        public void UpdateCity(City city)
        {
            cities[city.Id] = city;
        }

        public void AddCountry(Country country)
        {
            countries.Add(country.Id, country);
        }

        public Country GetCountry(string id)
        {
            if (countries is null)
            {
                countries = entityLoader.LoadCountries().ToDictionary(x => x.Id, x => x);
            }

            if (!countries.ContainsKey(id))
            {
                return null;
            }

            return countries[id];
        }

        public IEnumerable<Country> GetCountries()
        {
            if (countries is null)
            {
                countries = entityLoader.LoadCountries().ToDictionary(x => x.Id, x => x);
            }

            return countries.Values;
        }

        public IEnumerable<string> GetCultureIds()
        {
            if (cultureIds is null)
            {
                cultureIds = entityLoader.LoadCultureIds().ToList();
            }

            return cultureIds;
        }

        public IEnumerable<string> GetReligionIds()
        {
            if (religionIds is null)
            {
                religionIds = entityLoader.LoadReligionIds().ToList();
            }

            return religionIds;
        }

        public IEnumerable<string> GetGovernmentIds()
        {
            if (governmentIds is null)
            {
                governmentIds = entityLoader.LoadGovernmentIds().ToList();
            }

            return governmentIds;
        }

        public IEnumerable<string> GetDiplomaticStanceIds()
        {
            if (diplomaticStanceIds is null)
            {
                diplomaticStanceIds = entityLoader.LoadDiplomaticStanceIds().ToList();
            }

            return diplomaticStanceIds;
        }
    }
}
