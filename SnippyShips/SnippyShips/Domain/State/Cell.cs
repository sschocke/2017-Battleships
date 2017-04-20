using System.Drawing;
using Newtonsoft.Json;

namespace SnippyShips.Domain.State
{
    public class Cell
    {
        [JsonProperty]
        public bool Occupied { get; set; }
        
        [JsonProperty]
        public bool Hit { get; set; }

        [JsonIgnore]
        public Point Point => new Point(X, Y);

        [JsonProperty]
        public int X { get; set; }

        [JsonProperty]
        public int Y { get; set; }
    }
}
