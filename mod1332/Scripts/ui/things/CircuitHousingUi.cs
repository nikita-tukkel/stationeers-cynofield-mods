using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using HarmonyLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                    data.sp.Add(registers[16], now);
                    data.ra.Add(registers[17], now);
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
            parentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            float formWidth = 1.15f;
            float formPadding = 0.02f;
            float columnWidth = (formWidth - 2 * formPadding) / 2;
            float nameWidth = 0.18f;
            float valueWidth = columnWidth - nameWidth;

            layout.spacing = 0.005f;
            {
                var view = lf3d.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = d.name.Current);
                view.value.margin = new Vector4(formPadding, formPadding, 0.01f, 0);

                if (colorScheme != null)
                    colorScheme.Add(view.value);
            }
            {
                var dbRow = CreateRow(layout.gameObject);
                var view = lf3d.NameValuePair(dbRow.gameObject, "<color=green>db</color>", "0000",
                    nameWidth: nameWidth, valueWidth: valueWidth);
                presenter.AddBinding((d) =>
                {
                    var v = d.db.Current;
                    view.value.text = skin.MathDisplay(v);
                    var lastChangeAge = d.db.ChangeAge;
                    var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 20f;
                    view.valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
                });

                if (colorScheme != null)
                    colorScheme.Add(view.value);

                view.name.margin = new Vector4(formPadding, 0.01f, 0, 0);
                view.name.color = new Color(0, 0.6f, 0, 0.2f);
                view.name.fontStyle = FontStyles.UpperCase | FontStyles.Bold;

                CreateForRegister3d(presenter, dbRow.gameObject, colorScheme, 17,
                    formPadding: formPadding, nameWidth: nameWidth, valueWidth: valueWidth);
            }
            var rowsVisibility = new List<HiddenPoolComponent>();
            for (var i = 0; i < 8; i++)
            {
                var n = i * 2;
                var m = n + 1;

                var hl = CreateRow(layout.gameObject);
                var hpool = hl.GetOrAddComponent<HiddenPoolComponent>();
                hpool.SetVisible(true);
                rowsVisibility.Add(hpool);

                CreateForRegister3d(presenter, hl.gameObject, colorScheme, n,
                    formPadding: formPadding, nameWidth: nameWidth, valueWidth: valueWidth);
                CreateForRegister3d(presenter, hl.gameObject, colorScheme, m,
                    formPadding: formPadding, nameWidth: nameWidth, valueWidth: valueWidth);
            }
            HiddenPoolComponent bottomPaddingVisibility;
            {
                var bottomPadding = lf3d.Text1(layout.gameObject, " ");
                bottomPadding.value.fontSize = 0.0001f;
                bottomPadding.value.margin = new Vector4(formWidth, formPadding, 0, 0);
                bottomPaddingVisibility = bottomPadding.value.GetOrAddComponent<HiddenPoolComponent>();
                bottomPaddingVisibility.SetVisible(true);
            }
            {
                presenter.AddBinding((d) =>
                {
                    foreach (var row in rowsVisibility)
                    {
                        row.Hide();
                    }
                    bottomPaddingVisibility.Hide();
                    for (var i = 0; i < 8; i++)
                    {
                        var n = i * 2;
                        var m = n + 1;
                        if (d.r[n].Current != 0 || d.r[m].Current != 0
                         || d.r[n].ChangeAge < 10 || d.r[m].ChangeAge < 10)
                            rowsVisibility[i].Show();
                    }
                    // if (d.sp.Current != 0 || d.ra.Current != 0
                    //      || d.sp.ChangeAge < 10 || d.ra.ChangeAge < 10)
                    //     rowsVisibility[8].Show();
                    bottomPaddingVisibility.Show();
                });
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent); // Needed to process possible changes in text heights
            return layout.gameObject;
        }

        private HorizontalLayoutGroup CreateRow(GameObject parent)
        {
            var hl = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            hl.padding = new RectOffset(0, 0, 0, 0);
            hl.spacing = 0.01f;
            hl.childAlignment = TextAnchor.UpperLeft;
            hl.childControlWidth = false;
            hl.childControlHeight = false;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childScaleWidth = false;
            hl.childScaleHeight = false;
            // var hlfitter = hl.gameObject.AddComponent<ContentSizeFitter>();
            // hlfitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            // hlfitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            return hl;
        }

        private void CreateForRegister3d(ICPresenter presenter, GameObject parent,
            ColorSchemeComponent colorScheme, int registerNumber,
            float formPadding,
            float nameWidth, float valueWidth)
        {
            var name = $"r{registerNumber}";
            switch (registerNumber)
            {
                case 16: name = "sp"; break;
                case 17: name = "ra"; break;
            }

            var view = lf3d.NameValuePair(parent, name, "0000", visible: false, nameWidth: nameWidth, valueWidth: valueWidth);
            presenter.AddBinding((d) =>
            {
                view.visiblility.SetVisible(d.hasChip.Current);
                TimeSeriesBuffer<double> regData;
                switch (registerNumber)
                {
                    case 16: regData = d.sp; break;
                    case 17: regData = d.ra; break;
                    default: regData = d.r[registerNumber]; break;
                }

                var v = regData.Current;
                //view.value.text = "99999,999";
                view.value.text = skin.MathDisplay(v);
                var lastChangeAge = regData.ChangeAge;
                var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 40f;
                view.valueBkgd.color = new Color(0.1f, 0.5f, 0.1f, alpha);
            });

            if (colorScheme != null)
            {
                colorScheme.Add(view.name);
                colorScheme.Add(view.value);
            }

            view.name.margin = new Vector4(formPadding, 0.01f, 0, 0);
            view.name.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
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
                    var lastChangeAge = d.db.ChangeAge;
                    var alpha = (10 - Mathf.Clamp(lastChangeAge, 0, 10)) / 10f;
                    view.valueBkgd.color = new Color(0, 0.5f, 0, alpha);
                });
            }
            {
                var view = Text2(layout.gameObject, "NO CHIP", Color.red, visible: false);
                presenter.AddBinding((d) => view.visiblility.SetVisible(!d.hasChip.Current));
            }

            for (var i = 0; i < 9; i++)
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
            var name = $"r{registerNumber}";
            switch (registerNumber)
            {
                case 16: name = "sp"; break;
                case 17: name = "ra"; break;
            }
            var view = NameValuePair2(parent, name, "0000", visible: false);
            presenter.AddBinding((d) =>
            {
                view.visiblility.SetVisible(d.hasChip.Current);
                TimeSeriesBuffer<double> regData;
                switch (registerNumber)
                {
                    case 16: regData = d.sp; break;
                    case 17: regData = d.ra; break;
                    default: regData = d.r[registerNumber]; break;
                }
                var v = regData.Current;
                view.value.text = skin.MathDisplay(v);
                var lastChangeAge = regData.ChangeAge;
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
