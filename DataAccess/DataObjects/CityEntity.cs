using NuciDAL.DataObjects;

namespace ImperatorShatteredWorldGenerator.DataAccess.DataObjects
{
    public sealed class CityEntity : EntityBase
    {
        public string CultureId { get; set; }

        public string ReligionId { get; set; }

        public string TradeGoodId { get; set; }

        public int CitizensCount { get; set; }

        public int FreemenCount { get; set; }

        public int TribesmenCount { get; set; }

        public int SlavesCount { get; set; }

        public string CivilizationLevel { get; set; }

        public string BarbarianLevel { get; set; }

        public string NameId { get; set; }

        public string ProvinceId { get; set; }
    }
}
