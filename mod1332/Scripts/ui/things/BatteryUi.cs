using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class BatteryUi : IThingDescriber, IThingDetailsRenderer, IThingWatcher, IThingWatcherProvider
    {
        private readonly BatteryDataModel dataModel = new BatteryDataModel();
        private readonly ViewLayoutFactory lf;
        private readonly BaseSkin skin;
        public BatteryUi(ViewLayoutFactory lf, BaseSkin skin)
        {
            this.lf = lf;
            this.skin = skin;
        }

        public Type SupportedType() { return typeof(Battery); }

        public string Describe(Thing thing)
        {
            var obj = thing as Battery;
            var charge = obj.GetLogicValue(LogicType.Charge);
            var chargeMax = obj.GetLogicValue(LogicType.Maximum);
            return $"{obj.DisplayName} {skin.PowerDisplay(charge)} / {skin.PowerDisplay(chargeMax)}";
        }

        public class BatteryDataModel
        {
            private readonly TimeSeriesDb db = new TimeSeriesDb();
            private readonly ConcurrentDictionary<string, RecordView> views = new ConcurrentDictionary<string, RecordView>();

            public class RecordView
            {
#pragma warning disable IDE1006
                public readonly TimeSeriesBuffer<string> name;
                public readonly TimeSeriesBuffer<string> description;
                public readonly TimeSeriesBuffer<double> stored;
                public readonly TimeSeriesBuffer<double> discharge;
                public readonly TimeSeriesBuffer<double> charge;
                public readonly double storageMax;

                public RecordView(TimeSeriesRecord tsr, double chargeMax)
                {
                    var resolutionSeconds = 0.5f;
                    var historyDepthSeconds = 120;
                    var bufferSize = Mathf.RoundToInt(historyDepthSeconds / resolutionSeconds);
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
                    description = tsr.Add("description", new TimeSeriesBuffer<string>(new string[2], 1));
                    stored = tsr.Add("stored", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    discharge = tsr.Add("discharge", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    charge = tsr.Add("charge", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    this.storageMax = chargeMax;
                }
            }

            public RecordView Get(Battery thing)
            {
                var thingId = Utils.GetId(thing);
                return views.GetOrAdd(thingId, (string _) =>
                {
                    var hr = db.Get(thingId, () => new TimeSeriesRecord());
                    return new RecordView(hr, thing.GetLogicValue(LogicType.Maximum));
                });
            }

            public RecordView Snapshot(Thing thing, string description)
            {
                var obj = thing as Battery;
                if (obj == null)
                    return null;

                var now = Time.time;
                var data = Get(obj);
                data.name.Add(thing.DisplayName, now);
                if (description != null)
                    data.description.Add(description, now);
                data.stored.Add(obj.GetLogicValue(LogicType.Charge), now);
                data.discharge.Add(obj.GetLogicValue(LogicType.PowerActual), now);
                data.charge.Add(obj.GetLogicValue(LogicType.PowerPotential), now);
                return data;
            }
        }

        public class BatteryPresenter : PresenterBase<BatteryDataModel.RecordView>
        { }

        public GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag)
        {
            var description = Describe(thing);
            var data = dataModel.Snapshot(thing, description);
            var presenter = parentRect.GetComponentInChildren<BatteryPresenter>();

            if (presenter == null)
            {
                presenter = parentRect.GetOrAddComponent<BatteryPresenter>();
                {
                    var view = lf.Text1(parentRect.gameObject, $"{description}");
                    presenter.AddBinding((d) => view.value.text = d.name.Current);
                }
                var hl = CreateRow(parentRect.gameObject);
                {
                    var stor = lf.Text2(hl.gameObject, "0000", width: 60);
                    stor.value.margin = new Vector4(5, 0, 5, 0);
                    BindingStorage(presenter, stor);

                    var delta = lf.Text2(hl.gameObject, "0000", width: 100);
                    delta.value.margin = new Vector4(5, 0, 0, 0);
                    BindingStorageDelta(presenter, delta);
                }
            }

            presenter.Present(data);
            return presenter.gameObject;
        }

        public GameObject RenderDetails(Thing thing, RectTransform parentRect, GameObject poolreuse)
        {
            var description = Describe(thing);
            var data = dataModel.Snapshot(thing, description);
            if (data == null)
                return null;

            BatteryPresenter presenter = null;
            if (poolreuse != null)
                poolreuse.TryGetComponent(out presenter);

            if (presenter == null)
                presenter = CreateDetailsView(thing, parentRect).GetComponent<BatteryPresenter>();

            presenter.Present(data);
            return presenter.gameObject;
        }

        private GameObject CreateDetailsView(Thing thing, RectTransform parentRect)
        {
            var layout = lf.RootLayout(parentRect.gameObject, debug: false);
            layout.spacing = 1;
            var presenter = layout.GetOrAddComponent<BatteryPresenter>();
            {
                var view = lf.Text1(layout.gameObject, "");
                presenter.AddBinding((d) => view.value.text = d.name.Current);
            }
            {
                var hl = CreateRow(layout.gameObject);
                var st1 = lf.Text2(hl.gameObject, "0000", width: 50);
                st1.value.margin = new Vector4(5, 0, 5, 0);
                var st2 = lf.Text2(hl.gameObject, "0000", width: 60);
                BindingStorage(presenter, st1, st2);

                var delta = lf.Text2(hl.gameObject, "0000", width: 100);
                delta.value.margin = new Vector4(5, 0, 0, 0);
                BindingStorageDelta(presenter, delta);
            }
            {
                var hl = CreateRow(layout.gameObject);
                var lbl1 = lf.Text1(hl.gameObject, "Discharging", width: 110);
                lbl1.value.margin = new Vector4(5, 0, 5, 0);
                var dis = lf.Text2(hl.gameObject, "0000", width: 80);
                dis.value.margin = new Vector4(5, 0, 5, 0);
                presenter.AddBinding((d) => dis.value.text = skin.PowerDisplay(d.discharge.Current));
            }
            {
                var hl = CreateRow(layout.gameObject);
                var lbl2 = lf.Text1(hl.gameObject, "Charging", width: 110);
                lbl2.value.margin = new Vector4(5, 0, 5, 0);
                var cha = lf.Text2(hl.gameObject, "0000", width: 80);
                cha.value.margin = new Vector4(5, 0, 5, 0);
                presenter.AddBinding((d) => cha.value.text = skin.PowerDisplay(d.charge.Current));
            }

            return layout.gameObject;
        }

        private void BindingStorage(BatteryPresenter presenter, ValueView st1, ValueView st2 = null) => presenter.AddBinding((d) =>
        {
            var current = d.stored.Current;
            var stPercent = Math.Round(current * 100 / d.storageMax, 0);
            st1.value.text = stPercent + "%";
            if (stPercent <= 20)
                st1.valueBkgd.color = new Color(0.5f, 0, 0, 0.4f);
            else
                st1.valueBkgd.color = new Color(0, 0, 0, 0f);

            if (st2 != null)
                st2.value.text = skin.PowerDisplay(current);
        });

        private void BindingStorageDelta(BatteryPresenter presenter, ValueView delta1) => presenter.AddBinding((d) =>
        {
            var (currentMeta, current, _) = d.stored.GetCurrent();
            var (oldestMeta, oldest, _) = d.stored.GetOldest(10);
            if (currentMeta != null && oldestMeta != null)
            {
                var dV = current - oldest;
                var dT = Math.Clamp(((Meta)currentMeta).timestamp - ((Meta)oldestMeta).timestamp, 1, 1000);
                var dVT = dV / dT;
                delta1.value.text = $"\u0394={skin.PowerDisplay(dVT)}/s";

                if (Math.Abs(dVT) < 1000)
                    delta1.valueBkgd.color = new Color(0, 0, 0, 0f);
                else if (dVT < 1000)
                    delta1.valueBkgd.color = new Color(0.5f, 0, 0, 0.4f);
                else
                    delta1.valueBkgd.color = new Color(0, 0.5f, 0, 0.4f);
            }
            else
            {
                delta1.value.text = "";
            }
        });

        private HorizontalLayoutGroup CreateRow(GameObject parent)
        {
            var hl = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            hl.padding = new RectOffset(0, 0, 0, 0);
            hl.spacing = 0;
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

        public Dictionary<string, IThingWatcher> GetWatchers() => new Dictionary<string, IThingWatcher> {
            { "BAT", new BatteryBatchWatcher(lf, skin) },
        };

        public class BatteryBatchWatcher : IThingWatcher
        {
            private class Logger_ : CLogger { }
            private static readonly CLogger Log = new Logger_();

            private readonly ViewLayoutFactory lf;
            private readonly BaseSkin skin;
            public BatteryBatchWatcher(ViewLayoutFactory lf, BaseSkin skin)
            {
                this.lf = lf;
                this.skin = skin;
            }

            public Type SupportedType() => typeof(LogicBatchReader);

            public GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag)
            {
                PresenterDefault presenter = parentRect.GetComponentInChildren<PresenterDefault>();

                if (presenter == null)
                {
                    presenter = parentRect.GetOrAddComponent<PresenterDefault>();
                    {
                        var view = lf.Text1(parentRect.gameObject, $" abba ");
                        presenter.AddBinding((th) => view.value.text = (th as Thing).DisplayName);
                    }
                    var hl = lf.CreateRow(parentRect.gameObject);
                    {
                        var setting = lf.Text2(hl.gameObject, "0000", width: 60);
                        setting.value.margin = new Vector4(5, 0, 5, 0);
                        //presenter.AddBinding((th) => setting.value.text = skin.MathDisplay((th as LogicBatchReader).Setting));
                        presenter.AddBinding((th) =>
                        {
                            var obj = th as LogicBatchReader;
                            var ratio = Math.Round(100 * Device.BatchRead(obj.BatchMethod, obj.LogicType,
                                obj.CurrentPrefabHash, obj.InputNetwork1DevicesSorted));
                            setting.value.text = ratio + "%";
                        });
                        // BindingStorage(presenter, stor);

                        var sum = lf.Text2(hl.gameObject, "0000", width: 100);
                        sum.value.margin = new Vector4(5, 0, 0, 0);
                        presenter.AddBinding((th) =>
                        {
                            var obj = th as LogicBatchReader;
                            sum.value.text = skin.PowerDisplay(
                                Device.BatchRead(LogicBatchMethod.Sum, LogicType.Charge,
                                obj.CurrentPrefabHash, obj.InputNetwork1DevicesSorted));
                        });
                        // BindingStorageDelta(presenter, delta);
                    }
                }

                presenter.Present(thing as LogicBatchReader);
                return presenter.gameObject;
            }
        }
    }
}
