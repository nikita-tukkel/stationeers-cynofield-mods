using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;

namespace cynofield.mods.ui.things
{
    class CableUi : IThingDescriber
    {
        public Type SupportedType() { return typeof(Cable); }

        public string Describe(Thing thing)
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
