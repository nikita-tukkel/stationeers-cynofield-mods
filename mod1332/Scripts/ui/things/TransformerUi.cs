using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.styles;

namespace cynofield.mods.ui.things
{
    class TransformerUi : IThingDescriber
    {
        private readonly BaseSkin skin;
        public TransformerUi(BaseSkin skin)
        {
            this.skin = skin;
        }

        public Type SupportedType() { return typeof(Transformer); }

        public string Describe(Thing thing)
        {
            var obj = thing as Transformer;
            var color = obj.Powered ? "green" : "red";
            return
$@"{obj.DisplayName}
<color={color}><b>{obj.Setting}</b></color>
{skin.PowerDisplay(obj.UsedPower)}
{skin.PowerDisplay(obj.AvailablePower)}";
        }
    }
}
