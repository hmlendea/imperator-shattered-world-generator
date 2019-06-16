using System.Collections.Generic;

namespace ImperatorShatteredWorldGenerator.Service.Models
{
    public sealed class Country
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int ColourRed { get; set; }

        public int ColourGreen { get; set; }

        public int ColourBlue { get; set; }

        public string CultureId { get; set; }

        public string ReligionId { get; set; }

        public string GovernmentId { get; set; }

        public string DiplomaticStanceId { get; set; }
        
        public string CapitalId { get; set; }

        public IEnumerable<string> ShipNames { get; set; }

        public Country()
        {
            ShipNames = new List<string> { "Boaty", "McBoatface", "Boatface", "Shippy", "McShipface", "Shipface" };
        }
    }
}
