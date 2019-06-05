using NuciDAL.Repositories;

using ImperatorShatteredWorldGenerator.DataAccess.DataObjects;

namespace ImperatorShatteredWorldGenerator
{
    public sealed class Generator
    {
        readonly IRepository<CityEntity> cityRepository;

        public Generator(IRepository<CityEntity> cityRepository)
        {
            this.cityRepository = cityRepository;
        }

        public void Generate()
        {

        }
    }
}
