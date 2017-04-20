using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SnippyShips.Domain.Command.Ship;

namespace SnippyShips.Domain.State
{
    public class Ship
    {
        [JsonProperty]
        public bool Destroyed { get; set; }
        [JsonProperty]
        public bool Placed { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ShipType ShipType { get; set; }

        [JsonProperty]
        public List<Weapon> Weapons { get; set; }
        [JsonProperty]
        public List<Cell> Cells { get; set; }

        public static int Size(ShipType ship)
        {
            switch (ship)
            {
                case ShipType.Battleship:
                    return 4;
                case ShipType.Carrier:
                    return 5;
                case ShipType.Cruiser:
                    return 3;
                case ShipType.Destroyer:
                    return 2;
                case ShipType.Submarine:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ship), ship.ToString() + " is not a valid type of ship");
            }
        }
    }
}
