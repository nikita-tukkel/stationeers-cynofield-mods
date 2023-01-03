using System.Collections.Concurrent;
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
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        Type IThingDescriber.SupportedType() { return typeof(CircuitHousing); }

        private readonly BaseSkin skin;
        private readonly ICDataModel icdata = new ICDataModel();
        public CircuitHousingUi(BaseSkin skin)
        {
            this.skin = skin;
        }

        public class ICDataModel
        {
            private readonly TimeSeriesDb db = new TimeSeriesDb();
            private readonly ConcurrentDictionary<string, RecordView> views = new ConcurrentDictionary<string, RecordView>();

            public class RecordView
            {
#pragma warning disable IDE1006
                public readonly TimeSeriesBuffer<double> db;
                public readonly TimeSeriesBuffer<double> ra;
                public readonly TimeSeriesBuffer<double> sp;
                public readonly TimeSeriesBuffer<bool> hasChip;
                public readonly TimeSeriesBuffer<double>[] r;

                public RecordView(TimeSeriesRecord tsr)
                {
                    var resolutionSeconds = 0.5f;
                    var historyDepthSeconds = 120;
                    var bufferSize = Mathf.RoundToInt(historyDepthSeconds / resolutionSeconds);
                    db = tsr.Add("db", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    ra = tsr.Add("ra", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    sp = tsr.Add("sp", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    hasChip = tsr.Add("hasChip", new TimeSeriesBuffer<bool>(new bool[5], 1));
                    r = new TimeSeriesBuffer<double>[16];
                    for (var i = 0; i < 16; i++)
                        r[i] = tsr.Add($"r{i}", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                }
            }

            public RecordView Get(string thingId)
            {
                return views.GetOrAdd(thingId, (string _) =>
                {
                    var hr = db.Get(thingId, () => new TimeSeriesRecord());
                    return new RecordView(hr);
                });
            }

            public RecordView Snapshot(Thing thing)
            {
                var thingc = thing as CircuitHousing;
                if (thingc == null)
                    return null;
                var thingId = Utils.GetId(thing);
                var chip = thingc._ProgrammableChipSlot.Occupant as ProgrammableChip;
                var now = Time.time;
                var data = Get(thingId);
                data.db.Add(thingc.Setting, now);
                data.hasChip.Add(chip != null, now);
                if (chip != null)
                {
                    var registers = GetRegisters(chip);
                    for (var i = 0; i < 16; i++)
                    {
                        data.r[i].Add(registers[i], now);
                    }
                }
                return data;
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

            var data = icdata.Snapshot(thing);
            if (data == null)
                return null;
            GameObject view = CreateDetailsView(thing, data, parentRect);
            return view;
        }

        private GameObject CreateDetailsView(Thing thing,
            ICDataModel.RecordView data, RectTransform parent)
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
            //NameValuePair(layout.gameObject, $"<color=green>db</color>", $"{skin.MathDisplay(data.db.Current)}");
            NameValuePair3(layout.gameObject, $"<color=green>db</color>", data.db);
            if (data.hasChip.Current)
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

                    NameValuePair3(hl.gameObject, $"r{n}", data.r[n]);
                    NameValuePair3(hl.gameObject, $"r{m}", data.r[m]);
                }
            }
            else
            {
                Text2(layout.gameObject, "NO CHIP", Color.red);
            }

            return layout.gameObject;
        }

        private void NameValuePair3(GameObject parent, string name, TimeSeriesBuffer<double> value)
        {
            var v = value.Current;
            var lastChangeAge = value.ChangeAge();
            if (lastChangeAge >= 0 && lastChangeAge < 10)
            {
                var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 10f;
                NameValuePair2(parent, name, $"{skin.MathDisplay(v)}", valueBkgd: new Color(0, 0.5f, 0, alpha));
            }
            else
            {
                NameValuePair2(parent, name, $"{skin.MathDisplay(v)}");
            }
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
            return tmp;
        }

        internal static double[] GetRegisters(ProgrammableChip chip)
        {
            return Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        }
    }
}
