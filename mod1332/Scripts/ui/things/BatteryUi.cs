using System;
using System.Collections.Concurrent;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class BatteryUi : IThingDescriber, IThingWatcher
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
                public readonly TimeSeriesBuffer<double> charge;
                public readonly TimeSeriesBuffer<double> powerActual;
                public readonly TimeSeriesBuffer<double> powerPotential;
                public readonly double chargeMax;

                public RecordView(TimeSeriesRecord tsr, double chargeMax)
                {
                    var resolutionSeconds = 0.5f;
                    var historyDepthSeconds = 120;
                    var bufferSize = Mathf.RoundToInt(historyDepthSeconds / resolutionSeconds);
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
                    description = tsr.Add("description", new TimeSeriesBuffer<string>(new string[2], 1));
                    charge = tsr.Add("charge", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    powerActual = tsr.Add("powerActual", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    powerPotential = tsr.Add("powerPotential", new TimeSeriesBuffer<double>(new double[bufferSize], resolutionSeconds));
                    this.chargeMax = chargeMax;
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
                data.charge.Add(obj.GetLogicValue(LogicType.Charge), now);
                data.powerActual.Add(obj.GetLogicValue(LogicType.PowerActual), now);
                data.powerPotential.Add(obj.GetLogicValue(LogicType.PowerPotential), now);
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
                    var charge = lf.Text2(hl.gameObject, "0000", width: 60);
                    charge.value.margin = new Vector4(5, 0, 5, 0);
                    var delta = lf.Text2(hl.gameObject, "0000", width: 100);
                    delta.value.margin = new Vector4(5, 0, 0, 0);
                    presenter.AddBinding((d) =>
                    {
                        var (currentMeta, current, _) = d.charge.GetCurrent();
                        var chargePercent = Math.Round(current * 100 / d.chargeMax, 0);
                        charge.value.text = chargePercent + "%";
                        var (oldestMeta, oldest, _) = d.charge.GetOldest();
                        if (currentMeta != null && oldestMeta != null)
                        {
                            var dV = current - oldest;
                            var dT = Math.Clamp(((Meta)currentMeta).timestamp - ((Meta)oldestMeta).timestamp, 0, 1000);
                            delta.value.text = $"\u0394={skin.PowerDisplay(dV / dT)}/s";
                        }
                        else
                        {
                            delta.value.text = "aaa";
                        }

                    });
                }
                Utils.Show(parentRect);
            }

            presenter.Present(data);
            return presenter.gameObject;
        }

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

    }
}
