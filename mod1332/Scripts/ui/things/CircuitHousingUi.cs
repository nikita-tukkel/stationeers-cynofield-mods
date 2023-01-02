using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class CircuitHousingUi : IThingDescriber, IThingDetailsRenderer
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        Type IThingDescriber.SupportedType() { return typeof(CircuitHousing); }

        private readonly BaseSkin skin;
        private readonly HistoricalData<ICHistoricalData> allhistory = new HistoricalData<ICHistoricalData>(10);
        public CircuitHousingUi(BaseSkin skin)
        {
            this.skin = skin;
        }

        public class ICHistoricalData
        {
            public double db;
            public double[] registers;

            public override bool Equals(object obj)
            {
                return obj is ICHistoricalData data &&
                       db == data.db &&
                       EqualityComparer<double[]>.Default.Equals(registers, data.registers);
            }

#pragma warning disable IDE0070
            public override int GetHashCode()
            {
                int hash = 17;
                hash = hash * 31 + db.GetHashCode();
                hash = hash * 31 + registers.GetHashCode();
                return hash;
            }

            public override String ToString()
            {
                if (registers == null || registers.Length < 15)
                {
                    return $"{db}";
                }
                else
                {
                    return $"{db}/{registers[15]}/{registers[14]}";
                }
            }
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
            // don't use cached object for now... :-(
            //history

            var thingc = thing as CircuitHousing;
            var chip = thingc._ProgrammableChipSlot.Occupant as ProgrammableChip;
            var data = ReadData(thingc, chip);

            var history = allhistory.Get(Utils.GetId(thing));
            var last = history.Last();
            history.Add(data);
            var changes = history.ChangesInLast(5);
            var size = history.Size();
            //Log.Debug(() => $"{Utils.GetId(thing)} changes {changes}, size {size}");

            GameObject view = CreateDetailsView(thing, last, data, parentRect);
            return view;
        }

        private ICHistoricalData ReadData(CircuitHousing thing, ProgrammableChip chip)
        {
            return new ICHistoricalData()
            {
                db = thing.Setting,
                registers = chip == null ? null : ((double[])GetRegisters(chip).Clone())
            };
        }

        private GameObject CreateDetailsView(Thing thing,
            ICHistoricalData prev, ICHistoricalData curr, RectTransform parent)
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

            Text1(layout.gameObject, thing.DisplayName);
            NameValuePair(layout.gameObject, $"<color=green>db</color>", $"{skin.MathDisplay(curr.db)}");
            if (curr.registers == null)
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

                    double v, p;
                    v = curr.registers[n];
                    p = prev == null ? 0 : prev.registers[n];
                    if (v == p)
                        NameValuePair2(hl.gameObject, $"r{n}", $"{skin.MathDisplay(v)}");
                    else
                        NameValuePair2(hl.gameObject, $"r{n}", $"{skin.MathDisplay(v)}", valueBkgd: new Color(0, 1, 0, 0.2f));
                    v = curr.registers[m];
                    p = prev == null ? 0 : prev.registers[m];
                    if (v == p)
                        NameValuePair2(hl.gameObject, $"r{m}", $"{skin.MathDisplay(v)}");
                    else
                        NameValuePair2(hl.gameObject, $"r{m}", $"{skin.MathDisplay(v)}", valueBkgd: new Color(0, 1, 0, 0.2f));
                }
            }
            return layout.gameObject;
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

        private GameObject NameValuePair2(GameObject parent, string name, string value, Color valueBkgd = default)
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
            Text2(layout.gameObject, value, bkgd: valueBkgd, width: 80);

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
            skin.skin2d.MainFont(tmp);
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
