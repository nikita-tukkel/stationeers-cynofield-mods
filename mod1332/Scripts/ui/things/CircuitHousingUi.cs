using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using HarmonyLib;
using System;
using System.Collections.Concurrent;
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

        private readonly ViewLayoutFactory lf;
        private readonly BaseSkin skin;
        private readonly ICDataModel icdata = new ICDataModel();
        public CircuitHousingUi(ViewLayoutFactory lf, BaseSkin skin)
        {
            this.lf = lf;
            this.skin = skin;
        }

        public GameObject RenderDetails(
            Thing thing,
            RectTransform parentRect,
            GameObject poolreuse)
        {
            var data = icdata.Snapshot(thing);
            if (data == null)
                return null;

            ICPresenter presenter = null;
            if (poolreuse != null)
                poolreuse.TryGetComponent(out presenter);

            if (presenter == null)
                presenter = CreateDetailsView(thing, parentRect).GetComponent<ICPresenter>();

            presenter.Present(data);
            return presenter.gameObject;
        }

        public class ICDataModel
        {
            private readonly TimeSeriesDb db = new TimeSeriesDb();
            private readonly ConcurrentDictionary<string, RecordView> views = new ConcurrentDictionary<string, RecordView>();

            public class RecordView
            {
#pragma warning disable IDE1006
                public readonly TimeSeriesBuffer<string> name;
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
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
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
                var chip = thingc._ProgrammableChipSlot.Occupant as ProgrammableChip;
                var thingId = Utils.GetId(thing);
                //Log.Debug(() => $"thingId={thingId}");
                var now = Time.time;
                var data = Get(thingId);
                data.name.Add(thingc.DisplayName, now);
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

        public class ICPresenter : PresenterBase<ICDataModel.RecordView>
        { }

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

        private GameObject CreateDetailsView(Thing thing, RectTransform parent)
        {
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            var presenter = layout.gameObject.AddComponent<ICPresenter>();

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

            {
                var view = lf.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = d.name.Current);
            }
            {
                var view = NameValuePair2(layout.gameObject, "<color=green>db</color>", "0000");
                presenter.AddBinding((d) =>
                {
                    var v = d.db.Current;
                    view.value.text = skin.MathDisplay(v);
                    var lastChangeAge = d.db.ChangeAge();
                    var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 10f;
                    view.valueBkgd.color = new Color(0, 0.5f, 0, alpha);
                });
            }
            {
                var view = Text2(layout.gameObject, "NO CHIP", Color.red, visible: false);
                presenter.AddBinding((d) => view.visiblility.SetVisible(!d.hasChip.Current));
            }

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

                CreateForRegister(presenter, hl.gameObject, n);
                CreateForRegister(presenter, hl.gameObject, m);
            }

            return layout.gameObject;
        }

        private void CreateForRegister(ICPresenter presenter, GameObject parent, int registerNumber)
        {
            var view = NameValuePair2(parent, $"r{registerNumber}", "0000", visible: false);
            presenter.AddBinding((d) =>
            {
                view.visiblility.SetVisible(d.hasChip.Current);
                var regData = d.r[registerNumber];
                var v = regData.Current;
                view.value.text = skin.MathDisplay(v);
                var lastChangeAge = regData.ChangeAge();
                var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 10f;
                view.valueBkgd.color = new Color(0, 0.5f, 0, alpha);
            });
        }

        private NameValuePairView NameValuePair2(GameObject parent, string name, string value,
            Color valueBkgd = default, bool visible = true)
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
            var hpool = layout.gameObject.AddComponent<HiddenPoolComponent>();
            hpool.SetVisible(visible);

            var nameView = Text2(layout.gameObject, name, width: 25);
            var valueView = Text2(layout.gameObject, value, bkgd: valueBkgd, width: 80);

            return new NameValuePairView(layout, name: nameView.value, value: valueView);
        }

        private ValueView Text2(GameObject parent, string text,
            Color bkgd = default, int width = 0, bool visible = true)
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

            var hpool = layout.gameObject.AddComponent<HiddenPoolComponent>();
            hpool.SetVisible(visible);

            var tmp = lf.Text1(layout.gameObject, text, width);
            RawImage img = null;
            if (bkgd != null)
            {
                img = layout.gameObject.AddComponent<RawImage>();
                img.rectTransform.sizeDelta = size;
                img.color = bkgd;
            }
            return new ValueView(layout, tmp.value, img);
        }

        internal static double[] GetRegisters(ProgrammableChip chip)
        {
            return Traverse.Create(chip).Field("_Registers").GetValue() as double[];
        }
    }
}
