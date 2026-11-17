using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service.Mapping
{
    static class CityMapping
    {
        internal static City ToServiceModel(this CityEntity dataObject)
        {
            City serviceModel = new City();
            serviceModel.Id = dataObject.Id;
            serviceModel.CultureId = dataObject.CultureId;
            serviceModel.ReligionId = dataObject.ReligionId;
            serviceModel.TradeGoodId = dataObject.TradeGoodId;
            serviceModel.CitizensCount = dataObject.CitizensCount;
            serviceModel.FreemenCount = dataObject.FreemenCount;
            serviceModel.TribesmenCount = dataObject.TribesmenCount;
            serviceModel.SlavesCount = dataObject.SlavesCount;
            serviceModel.CivilizationLevel = ParseLevel(dataObject.CivilizationLevel);
            serviceModel.BarbarianLevel = ParseLevel(dataObject.BarbarianLevel);
            serviceModel.NameId = dataObject.NameId;
            serviceModel.ProvinceId = dataObject.ProvinceId;
            serviceModel.ProvinceRank = dataObject.ProvinceRank;
            serviceModel.PreservedLines = dataObject.PreservedLines;

            return serviceModel;
        }

        internal static CityEntity ToDataObject(this City serviceModel)
        {
            CityEntity dataObject = new CityEntity();
            dataObject.Id = serviceModel.Id;
            dataObject.CultureId = serviceModel.CultureId;
            dataObject.ReligionId = serviceModel.ReligionId;
            dataObject.TradeGoodId = serviceModel.TradeGoodId;
            dataObject.CitizensCount = serviceModel.CitizensCount;
            dataObject.FreemenCount = serviceModel.FreemenCount;
            dataObject.TribesmenCount = serviceModel.TribesmenCount;
            dataObject.SlavesCount = serviceModel.SlavesCount;
            dataObject.CivilizationLevel = serviceModel.CivilizationLevel.ToString();
            dataObject.BarbarianLevel = serviceModel.BarbarianLevel.ToString();
            dataObject.NameId = serviceModel.NameId;
            dataObject.ProvinceId = serviceModel.ProvinceId;
            dataObject.ProvinceRank = serviceModel.ProvinceRank;
            dataObject.PreservedLines = serviceModel.PreservedLines;

            return dataObject;
        }

        internal static IEnumerable<City> ToServiceModels(this IEnumerable<CityEntity> dataObjects)
        {
            IEnumerable<City> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }

        static int ParseLevel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            if (int.TryParse(value, out int integerValue))
            {
                return integerValue;
            }

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double decimalValue))
            {
                return (int)decimalValue;
            }

            return 0;
        }
    }
}
