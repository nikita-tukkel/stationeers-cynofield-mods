
using Assets.Scripts.Objects;

namespace cynofield.mods.utils
{
    public class Utils
    {
        public string GetId(Thing thing) { return thing == null ? "" : thing.ReferenceId.ToString(); }
    }
}
