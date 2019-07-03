using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Configuration;
using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service.Generators
{
    public sealed class CountryGenerator : ICountryGenerator
    {
        readonly static string AllowedCapitalIdCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        readonly static string[] DisallowedCountryIds = { "REB", "PIR", "BAR", "MER" };

        readonly IEntityManager entityManager;
        readonly IRandomNumberGenerator rng;
        readonly GeneratorSettings settings;

        public CountryGenerator(
            IEntityManager entityManager,
            IRandomNumberGenerator rng,
            GeneratorSettings settings)
        {
            this.entityManager = entityManager;
            this.rng = rng;
            this.settings = settings;
        }

        public Country GenerateCountry(string capitalCityId)
        {
            City capital = entityManager.GetCity(capitalCityId);
            Country country = new Country();

            country.Id = GenerateCountryId(capital.NameId);
            country.Name = capital.NameId;

            country.CultureId = capital.CultureId;
            country.ReligionId = capital.ReligionId;

            country.GovernmentId = entityManager.GetGovernmentIds().GetRandomElement(rng.Randomiser);
            country.DiplomaticStanceId = entityManager.GetDiplomaticStanceIds().GetRandomElement(rng.Randomiser);
            country.CentralisationLevel = rng.Get(settings.CountryCentralisationLevelMin, settings.CountryCentralisationLevelMax);
            country.CapitalId = capital.Id;

            country.ColourRed = rng.Get(0, 255);
            country.ColourGreen = rng.Get(0, 255);
            country.ColourBlue = rng.Get(0, 255);

            return country;
        }

        string GenerateCountryId(string name)
        {
            string normalisedCapitalName = name
                .RemovePunctuation()
                .Replace(" ", "")
                .ToUpper();
            
            string id = GenerateCapitalIdBasedOnName(name);

            if (id is null)
            {
                id = GenerateRandomCapitalId();
            }

            return id;
        }

        string GenerateCapitalIdBasedOnName(string name)
        {
            string normalisedName = name
                .RemovePunctuation()
                .Replace(" ", "")
                .ToUpper();
            
            if (normalisedName.Length < 3)
            {
                return null;
            }

            for (int i = 0; i < normalisedName.Length; i++)
            {
                for (int j = i + 1; j < normalisedName.Length; j++)
                {
                    for (int k = j + 1; k < normalisedName.Length; k++)
                    {
                        string id = $"{normalisedName[i]}{normalisedName[j]}{normalisedName[k]}";

                        if (IsCapitalIdValid(id))
                        {
                            return id;
                        }
                    }
                }
            }

            return null;
        }

        string GenerateRandomCapitalId()
        {
            string id = null;

            while (!IsCapitalIdValid(id))
            {
                id = string.Empty;

                for (int i = 0; i < 3; i++)
                {
                    id += AllowedCapitalIdCharacters.GetRandomElement(rng.Randomiser);
                }
            }

            return id;
        }

        bool IsCapitalIdValid(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return
                !DisallowedCountryIds.Contains(id) &&
                entityManager.GetCountry(id) is null &&
                id.All(AllowedCapitalIdCharacters.Contains);
        }
    }
}
