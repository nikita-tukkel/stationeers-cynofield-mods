using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.styles;

namespace cynofield.mods.ui.things
{
    class CableUi : IThingDescriber
    {
        private readonly BaseSkin skin;
        public CableUi(BaseSkin skin)
        {
            this.skin = skin;
        }

        public Type SupportedType() { return typeof(Cable); }

        public string Describe(Thing thing)
        {
            var obj = thing as Cable;
            var net = obj.CableNetwork;
            return
$@"network: {net.DisplayName}
{skin.PowerDisplay(net.CurrentLoad)} / {skin.PowerDisplay(net.PotentialLoad)}";
        }
    }
}
