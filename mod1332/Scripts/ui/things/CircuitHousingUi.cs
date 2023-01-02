using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class CircuitHousingUi : IThingDescriber, IThingDetailsRenderer
    {
        Type IThingDescriber.SupportedType() { return typeof(CircuitHousing); }

        private readonly BaseSkin skin;
        private readonly Fonts2d fonts2d;

        public CircuitHousingUi(BaseSkin skin, Fonts2d fonts2d)
        {
            this.skin = skin;
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
<color=red><b>db={skin.MathDisplay(obj.Setting)}</b>
NO CHIP</color>";
            }
            else
            {
                var registers = GetRegisters(chip);
                return
$@"{obj.DisplayName}
<color=green><b>db={skin.MathDisplay(obj.Setting)}</b> r15={registers[15]}</color>
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
                (view, text) = CreateDetailsView(thing, parentRect);
            }
            //text.text = Describe(thing);
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(text.gameObject.GetComponent<RectTransform>());
            foreach (RectTransform rt in view.GetComponentInChildren<RectTransform>())
            {
                if (rt != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
            return view;
        }

        private (GameObject, TextMeshProUGUI) CreateDetailsView(Thing thing, RectTransform parent)
        {
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            layout.padding = new RectOffset(1, 1, 1, 1);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childScaleWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.childScaleHeight = false;
            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var thingc = thing as CircuitHousing;
            var chip = thingc._ProgrammableChipSlot.Occupant as ProgrammableChip;
            double[] registers = GetRegisters(chip);

            var text = Text1(layout.gameObject, thingc.DisplayName);
            NameValuePair(layout.gameObject, $"<color=green>db</color>", $"{skin.MathDisplay(thingc.Setting)}");
            if (chip == null)
            {
                Text2(layout.gameObject, "NO CHIP", Color.red);
            }
            else
            {
                for (var i = 0; i < 8; i++)
                {
                    var n = i * 2;
                    var m = n + 1;

                    var hl = Utils.CreateGameObject<HorizontalLayoutGroup>(layout.gameObject);
                    hl.padding = new RectOffset(1, 1, 1, 1);
                    hl.spacing = 20;
                    hl.childAlignment = TextAnchor.UpperLeft;
                    hl.childControlWidth = false;
                    hl.childForceExpandWidth = false;
                    hl.childScaleWidth = false;
                    hl.childControlHeight = true;
                    hl.childForceExpandHeight = true;
                    hl.childScaleHeight = false;
                    var hlfitter = hl.gameObject.AddComponent<ContentSizeFitter>();
                    hlfitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    hlfitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    NameValuePair(hl.gameObject, $"r{n}", $"{skin.MathDisplay(registers[n])}");
                    NameValuePair2(hl.gameObject, $"r{m}", $"{skin.MathDisplay(registers[m])}");
                }
            }
            return (layout.gameObject, text.GetComponent<TextMeshProUGUI>());
        }

        private GameObject NameValuePair(GameObject parent, string name, string value)
        {
            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            layout.padding = new RectOffset(1, 1, 1, 1);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childScaleWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.childScaleHeight = false;
            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text1(layout.gameObject, name, width: 25);
            Text1(layout.gameObject, value, width: 80);

            return layout.gameObject;
        }

        private GameObject NameValuePair2(GameObject parent, string name, string value)
        {
            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            layout.padding = new RectOffset(1, 1, 1, 1);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
            layout.childScaleWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.childScaleHeight = false;
            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Text2(layout.gameObject, name, width: 25);
            Text2(layout.gameObject, value, bkgd: Color.red, width: 80);

            return layout.gameObject;
        }

        private GameObject Text1(GameObject parent, string text, int width = 0)
        {
            var tmp = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            tmp.rectTransform.sizeDelta = new Vector2(width, 0);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.margin = new Vector4(0f, 0f, 0f, 0f);
            tmp.richText = true;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.enableWordWrapping = false;
            skin.MainFont(tmp);
            tmp.color = new Color(1f, 1f, 1f, 1f);
            tmp.text = text;
            var nameFitter = tmp.gameObject.AddComponent<ContentSizeFitter>();
            if (width <= 0)
                nameFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            else
                nameFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            nameFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return tmp.gameObject;
        }

        private GameObject Text2(GameObject parent, string text, Color bkgd = default, int width = 0)
        {
            var size = new Vector2(width, 0);

            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            var layoutRect = layout.GetComponent<RectTransform>();
            layoutRect.sizeDelta = size;
            layout.padding = new RectOffset(4, 4, 1, 1);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = true;
            layout.childScaleWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = true;
            layout.childScaleHeight = false;
            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            if (width <= 0)
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            else
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var tmp = Text1(layout.gameObject, text, width);
            if (bkgd != null)
            {
                var img = layout.gameObject.AddComponent<RawImage>();
                img.rectTransform.sizeDelta = size;
                img.color = bkgd;
            }
            return tmp.gameObject;
        }

        private double[] GetRegisters(ProgrammableChip chip)
        {
            return Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        }
    }
}
