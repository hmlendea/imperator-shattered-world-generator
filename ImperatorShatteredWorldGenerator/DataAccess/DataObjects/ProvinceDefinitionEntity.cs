using NuciDAL.DataObjects;
using NuciDAL.IO;

namespace ImperatorShatteredWorldGenerator.DataAccess.DataObjects
{
    public sealed class ProvinceDefinitionEntity : EntityBase
    {
        [CsvColumn(2)]
        public string Red { get; set; }

        [CsvColumn(3)]
        public string Green { get; set; }

        [CsvColumn(4)]
        public string Blue { get; set; }

        [CsvColumn(5)]
        public string Name { get; set; }
    }
}