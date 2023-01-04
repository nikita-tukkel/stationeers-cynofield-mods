using Assets.Scripts;
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
    class CircuitHousingUi : IThingCompleteUi
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public Type SupportedType() { return typeof(CircuitHousing); }

        private readonly ViewLayoutFactory lf;
        private readonly ViewLayoutFactory3d lf3d;
        private readonly BaseSkin skin;
        private readonly ICDataModel icdata = new ICDataModel();
        public CircuitHousingUi(ViewLayoutFactory lf, ViewLayoutFactory3d lf3d, BaseSkin skin)
        {
            this.lf = lf;
            this.lf3d = lf3d;
            this.skin = skin;
        }

        public GameObject RenderAnnotation(Thing thing, RectTransform parentRect)
        {
            var data = icdata.Snapshot(thing);
            if (data == null)
                return null;

            var presenter = parentRect.GetComponentInChildren<ICPresenter>();
            if (presenter == null)
            {
                //Log.Debug(() => $"{Utils.GetId(thing)} creating new annotation view");
                presenter = CreateAnnotationView(thing, parentRect).GetComponent<ICPresenter>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect); // Needed to process possible changes in text heights
            }

            presenter.Present(data);
            return presenter.gameObject;
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

            public RecordView Get(string thingId)
            {
                return views.GetOrAdd(thingId, (string _) =>
                {
                    var hr = db.Get(thingId, () => new TimeSeriesRecord());
                    return new RecordView(hr);
                });
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
                var registersStr = "";
                for (int i = 0; i < registers.Length; i++)
                {
                    var v = registers[i];
                    if (v != 0)
                        registersStr += $" r{i}={skin.MathDisplay(v)}";
                }

                return
$@"{obj.DisplayName}
<color=green><b>db={skin.MathDisplay(obj.Setting)}</b>{registersStr}</color>
";
            }
        }

        private GameObject CreateAnnotationView(Thing thing, RectTransform parent)
        {
            parent.gameObject.TryGetComponent(out ColorSchemeComponent colorScheme);
            var layout = lf3d.RootLayout(parent.gameObject, debug: false);
            var presenter = layout.gameObject.AddComponent<ICPresenter>();

            // When want to change parent resize behaviour:
            var parentFitter = parent.gameObject.GetComponent<ContentSizeFitter>();
            parentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            //parentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            {
                var view = lf3d.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = d.name.Current);
                view.value.margin = new Vector4(0.02f, 0.01f, 0.01f, 0);

                if (colorScheme != null)
                    colorScheme.Add(view.value);
            }
            {
                var view = lf3d.NameValuePair(layout.gameObject, "<color=green>db</color>", "0000");
                presenter.AddBinding((d) =>
                {
                    var v = d.db.Current;
                    view.value.text = skin.MathDisplay(v);
                    var lastChangeAge = d.db.ChangeAge();
                    var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 20f;
                    view.valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
                });

                if (colorScheme != null)
                    colorScheme.Add(view.value);

                view.name.margin = new Vector4(0.02f, 0.01f, 0, 0);
                view.name.color = new Color(0, 0.6f, 0, 0.2f);
                view.name.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
                view.value.margin = new Vector4(0.02f, 0.01f, 0.02f, 0);
            }

            for (var i = 0; i < 8; i++)
            {
                var n = i * 2;
                var m = n + 1;

                var hl = Utils.CreateGameObject<HorizontalLayoutGroup>(layout.gameObject);
                hl.padding = new RectOffset(0, 0, 0, 0);
                hl.spacing = 0.01f;
                hl.childAlignment = TextAnchor.UpperLeft;
                hl.childControlWidth = false;
                hl.childControlHeight = true;
                hl.childForceExpandWidth = false;
                hl.childForceExpandHeight = true;
                hl.childScaleWidth = false;
                hl.childScaleHeight = false;
                var hlfitter = hl.gameObject.AddComponent<ContentSizeFitter>();
                hlfitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                hlfitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                CreateForRegister3d(presenter, hl.gameObject, colorScheme, n);
                CreateForRegister3d(presenter, hl.gameObject, colorScheme, m);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent); // Needed to process possible changes in text heights
            return layout.gameObject;
        }

        private void CreateForRegister3d(ICPresenter presenter, GameObject parent,
         ColorSchemeComponent colorScheme, int registerNumber)
        {
            var view = lf3d.NameValuePair(parent, $"r{registerNumber}", "0000", visible: false);
            presenter.AddBinding((d) =>
            {
                view.visiblility.SetVisible(d.hasChip.Current);
                var regData = d.r[registerNumber];
                var v = regData.Current;
                view.value.text = skin.MathDisplay(v);
                var lastChangeAge = regData.ChangeAge();
                var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 40f;
                view.valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
            });

            if (colorScheme != null)
            {
                colorScheme.Add(view.name);
                colorScheme.Add(view.value);
            }

            view.name.margin = new Vector4(0.02f, 0.01f, 0, 0.01f);
            view.name.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
            view.value.margin = new Vector4(0.02f, 0.01f, 0.02f, 0.01f);
            view.value.fontStyle = FontStyles.Bold;
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
                hl.childControlHeight = true;
                hl.childForceExpandWidth = false;
                hl.childForceExpandHeight = true;
                hl.childScaleWidth = false;
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
