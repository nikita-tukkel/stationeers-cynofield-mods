using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;

namespace cynofield.mods.ui.things
{
    class CableUi : IThingArDescriber
    {
        Type IThingArDescriber.SupportedType() { return typeof(Cable); }

        string IThingArDescriber.Describe(Thing thing)
        {
            var obj = thing as Cable;
            var net = obj.CableNetwork;
            return
$@"network: {net.DisplayName}
{utils.PowerDisplay(net.CurrentLoad)} / {utils.PowerDisplay(net.PotentialLoad)}";
        }

        private readonly UiUtils utils = new UiUtils();
    }
}
