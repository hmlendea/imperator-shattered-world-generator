using System.Collections.Generic;
using System.Linq;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Models;

namespace ImperatorShatteredWorldGenerator.Mapping
{
    static class CityMapping
    {
        internal static City ToServiceModel(this CityEntity dataObject)
        {
            City serviceModel = new City();
            serviceModel.Id = dataObject.Id;
            serviceModel.CultureId = dataObject.CultureId;
            serviceModel.ReligionId = dataObject.ReligionId;
            serviceModel.NameId = dataObject.NameId;
            serviceModel.ProvinceId = dataObject.ProvinceId;

            return serviceModel;
        }

        internal static IEnumerable<City> ToServiceModels(this IEnumerable<CityEntity> dataObjects)
        {
            IEnumerable<City> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }
    }
}
