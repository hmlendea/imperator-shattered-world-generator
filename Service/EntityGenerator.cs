using System;
using System.Collections.Generic;
using System.Linq;

using NuciExtensions;

using ImperatorShatteredWorldGenerator.Service.Models;

namespace ImperatorShatteredWorldGenerator.Service
{
    public sealed class EntityGenerator : IEntityGenerator
    {
        readonly static string AllowedCapitalIdCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        readonly static string[] DisallowedCountryIds = { "REB", "PIR", "BAR", "MER" };

        readonly IRandomNumberGenerator rng;

        public EntityGenerator(IRandomNumberGenerator rng)
        {
            this.rng = rng;
        }

        public string GenerateCountryId(IEnumerable<Country> countries, string name)
        {
            string normalisedCapitalName = name
                .RemovePunctuation()
                .Replace(" ", "")
                .ToUpper();
            
            string id = GenerateCapitalIdBasedOnName(countries, name);

            if (id is null)
            {
                id = GenerateRandomCapitalId(countries);
            }

            return id;
        }

        string GenerateCapitalIdBasedOnName(IEnumerable<Country> countries, string name)
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

                        if (IsCapitalIdValid(countries, id))
                        {
                            return id;
                        }
                    }
                }
            }

            return null;
        }

        string GenerateRandomCapitalId(IEnumerable<Country> countries)
        {
            string id = null;

            while (!IsCapitalIdValid(countries, id))
            {
                id = string.Empty;

                for (int i = 0; i < 3; i++)
                {
                    id += AllowedCapitalIdCharacters.GetRandomElement(rng.Randomiser);
                }
            }

            return id;
        }

        bool IsCapitalIdValid(IEnumerable<Country> countries, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return
                !DisallowedCountryIds.Contains(id) &&
                countries.All(x => x.Id != id) &&
                id.All(AllowedCapitalIdCharacters.Contains);
        }
    }
}
