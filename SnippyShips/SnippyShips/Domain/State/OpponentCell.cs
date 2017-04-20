using System.Drawing;
using Newtonsoft.Json;

namespace SnippyShips.Domain.State
{
    public class OpponentCell
    {
        [JsonProperty]
        public bool Damaged { get; set; }
        
        [JsonProperty]
        public bool Missed { get; set; }

        [JsonIgnore]
        public Point Point => new Point(X, Y);

        [JsonProperty]
        public int X { get; set; }

        [JsonProperty]
        public int Y { get; set; }

        public int Probability { get; set; }
    }
}
