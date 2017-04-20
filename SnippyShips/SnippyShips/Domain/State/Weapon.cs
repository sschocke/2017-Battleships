using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnippyShips.Domain.State
{
    public class Weapon
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WeaponType WeaponType;
    }

    public enum WeaponType
    {
        SingleShot
    }
}
