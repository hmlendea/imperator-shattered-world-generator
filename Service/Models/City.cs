namespace ImperatorShatteredWorldGenerator.Service.Models
{
    public sealed class City
    {
        public string Id { get; set; }

        public string CultureId { get; set; }

        public string ReligionId { get; set; }

        public string TradeGoodId { get; set; }

        public int CitizensCount { get; set; }

        public int FreemenCount { get; set; }

        public int TribesmenCount { get; set; }

        public int SlavesCount { get; set; }

        public int CivilizationLevel { get; set; }

        public int BarbarianLevel { get; set; }

        public string NameId { get; set; }

        public string ProvinceId { get; set; }

        public int TotalPopulation => CitizensCount + FreemenCount + TribesmenCount + SlavesCount;

        public bool IsHabitable =>
            !string.IsNullOrWhiteSpace(CultureId) &&
            !string.IsNullOrWhiteSpace(ReligionId) &&
            !string.IsNullOrWhiteSpace(TradeGoodId) &&
            !string.IsNullOrWhiteSpace(NameId) &&
            !string.IsNullOrWhiteSpace(ProvinceId);
            
    }
}
