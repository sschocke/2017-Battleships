using Newtonsoft.Json;
using SnippyShips.Domain.Command.Ship;

namespace SnippyShips.Domain.State
{
    public class OpponentShip
    {
        [JsonProperty]
        public bool Destroyed { get; set; }
        [JsonProperty]
        public ShipType ShipType { get; set; }
    }
}
