using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;

namespace cynofield.mods.ui.things
{
    class TransformerUi : IThingDescriber
    {
        Type IThingDescriber.SupportedType() { return typeof(Transformer); }

        string IThingDescriber.Describe(Thing thing)
        {
            var obj = thing as Transformer;
            var color = obj.Powered ? "green" : "red";
            return
$@"{obj.DisplayName}
<color={color}><b>{obj.Setting}</b></color>
{utils.PowerDisplay(obj.UsedPower)}
{utils.PowerDisplay(obj.AvailablePower)}";
        }

        private readonly UiUtils utils = new UiUtils();
    }
}
