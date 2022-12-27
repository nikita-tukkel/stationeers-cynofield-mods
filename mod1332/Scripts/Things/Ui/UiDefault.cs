using System;
using Assets.Scripts.Objects;

namespace cynofield.mods.things.ui
{
    class UiDefault : IThingArDescriber
    {
        Type IThingArDescriber.SupportedType() { return null; }

        string IThingArDescriber.Describe(Thing thing)
        {
            return
$@"{thing.DisplayName}
please <color=red><b>don't</b></color> play with me";
        }
    }
}
