using System;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.utils;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class CircuitHousingUi : IThingDescriber, IThingDetailsRenderer
    {
        Type IThingDescriber.SupportedType() { return typeof(CircuitHousing); }

        private readonly Fonts2d fonts2d;
        public CircuitHousingUi(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
        }

        public string Describe(Thing thing)
        {
            var obj = thing as CircuitHousing;
            var chip = obj._ProgrammableChipSlot.Occupant as ProgrammableChip;
            if (chip == null)
            {
                return
$@"{obj.DisplayName}
<color=red><b>db={obj.Setting}</b>
no chip</color>";
            }
            else
            {
                var registers = GetRegisters(chip);
                return
$@"{obj.DisplayName}
<color=green><b>db={obj.Setting}</b><mspace=1em> </mspace>r15={registers[15]}</color>
{DisplayRegisters(registers)}
";
            }
        }

        public GameObject RenderDetails(
            Thing thing,
            RectTransform parentRect,
            GameObject poolreuse)
        {
            GameObject view = null;
            TextMeshProUGUI text = null;
            if (poolreuse != null)
                poolreuse.TryGetComponent(out text);
            if (text == null)
            {
                (view, text) = CreateDetailsView(parentRect);
            }
            text.text = Describe(thing);
            return view;
        }

        private (GameObject, TextMeshProUGUI) CreateDetailsView(RectTransform parent)
        {
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            var layoutTransform = layout.gameObject.AddComponent<RectTransform>();

            var text = Utils.CreateGameObject<TextMeshProUGUI>(layout);
            text.rectTransform.sizeDelta = new Vector2(parent.sizeDelta.x, 0);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(2f, 2f, 2f, 2f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;
            fonts2d.SetFont.superstar(20, text);

            var textFitter = text.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            text.color = new Color(1f, 1f, 1f, 1f);
            return (layout.gameObject, text);
        }

        private string DisplayRegisters(double[] registers)
        {
            string result = "";
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                if (registers[i] == 0)
                    continue;
                count++;
                result += $"r{i}={Math.Round(registers[i], 2)}";
                if (count > 0 && count % 2 == 0)
                    result += "\n";
                else
                    result += "<mspace=1em> </mspace>";
            }
            return result;
        }

        private double[] GetRegisters(ProgrammableChip chip)
        {
            return Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        }
    }
}
