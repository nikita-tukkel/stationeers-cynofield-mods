using System;
using System.Collections.Concurrent;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Motherboards;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class WeatherStationUi : IThingDescriber, IThingWatcher
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly WeatherStationDataModel dataModel = new WeatherStationDataModel();
        private readonly ViewLayoutFactory lf;
        private readonly BaseSkin skin;
        public WeatherStationUi(ViewLayoutFactory lf, BaseSkin skin)
        {
            this.lf = lf;
            this.skin = skin;
        }

        public Type SupportedType() { return typeof(WeatherStation); }

        public string Describe(Thing thing)
        {
            var obj = thing as WeatherStation;
            if (obj.Powered)
            {
                var mode = obj.GetLogicValue(LogicType.Mode);
                var stormEta = obj.GetLogicValue(LogicType.NextWeatherEventTime);
                string modeStr = "<color=green>Clear</color>";
                switch (mode)
                {
                    case 1: modeStr = $"<color=red>Storm in ${stormEta}s</color>"; break;
                    case 2: modeStr = "Storm"; break;
                }

                return $"{obj.DisplayName} {modeStr}";
            }
            else
            {
                return $"{obj.DisplayName} OFF";
            }
        }

        public class WeatherStationDataModel
        {
            private readonly TimeSeriesDb db = new TimeSeriesDb();
            private readonly ConcurrentDictionary<string, RecordView> views = new ConcurrentDictionary<string, RecordView>();

            public class RecordView
            {
#pragma warning disable IDE1006
                public readonly TimeSeriesBuffer<string> name;
                public readonly TimeSeriesBuffer<string> description;
                public readonly TimeSeriesBuffer<bool> powered;
                public readonly TimeSeriesBuffer<double> mode;
                public readonly TimeSeriesBuffer<double> stormEta;

                public RecordView(TimeSeriesRecord tsr)
                {
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
                    description = tsr.Add("description", new TimeSeriesBuffer<string>(new string[2], 1));
                    powered = tsr.Add("powered", new TimeSeriesBuffer<bool>(new bool[2], 0.5f));
                    mode = tsr.Add("mode", new TimeSeriesBuffer<double>(new double[10], 1));
                    stormEta = tsr.Add("stormEta", new TimeSeriesBuffer<double>(new double[10], 1));
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

            public RecordView Snapshot(Thing thing, string description)
            {
                var obj = thing as WeatherStation;
                if (obj == null)
                    return null;

                var thingId = Utils.GetId(thing);
                var now = Time.time;
                var data = Get(thingId);
                data.name.Add(Utils.GetName(thing), now);
                if (description != null)
                    data.description.Add(description, now);
                data.powered.Add(obj.Powered, now);

                var testing = false;
                if (testing)
                {
                    var rnd = new System.Random();
                    if (rnd.NextDouble() > 0.7)
                    {
                        var testMode = rnd.Next(0, 3);
                        data.mode.Add(testMode, now);
                    }
                    var testEta = Time.time;
                    data.stormEta.Add(testEta, now);
                }
                else
                {
                    data.mode.Add(obj.GetLogicValue(LogicType.Mode), now);
                    data.stormEta.Add(obj.GetLogicValue(LogicType.NextWeatherEventTime), now);
                }
                return data;
            }
        }

        public class WeatherStationPresenter : PresenterBase<WeatherStationDataModel.RecordView>
        {
            public double lastMode;
        }

        public GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag)
        {
            var description = Describe(thing);
            var data = dataModel.Snapshot(thing, description);
            var presenter = parentRect.GetComponentInChildren<WeatherStationPresenter>();

            if (presenter == null)
            {
                //Log.Debug(() => $"Creating new watch for {thing.DisplayName}");

                var stormAnnouncement = new AnnouncementWorkflow();

                presenter = parentRect.GetOrAddComponent<WeatherStationPresenter>();
                {
                    var view = lf.Text1(parentRect.gameObject, $"{description}");
                    presenter.AddBinding((d) => view.value.text = d.name.Current);
                }
                var hl = CreateRow(parentRect.gameObject);
                {
                    var modeName = lf.Text2(hl.gameObject, "0000", width: 80);
                    modeName.value.margin = new Vector4(5, 0, 5, 0);
                    var eta = lf.Text2(hl.gameObject, "0000", visible: false, width: 80);
                    eta.value.margin = new Vector4(5, 0, 0, 0);
                    presenter.AddBinding((d) =>
                    {
                        eta.value.text = "";
                        eta.visiblility.Hide();
                        if (d.powered.Current)
                        {
                            var mode = d.mode.Current;
                            switch (mode)
                            {
                                case 0:
                                    {
                                        modeName.value.text = "Clear";
                                        modeName.valueBkgd.color = new Color(0, 0.3f, 0, 0.4f);
                                    }
                                    break;
                                case 1:
                                    {
                                        modeName.value.text = "Storm in";
                                        modeName.valueBkgd.color = new Color(1f, 0, 0, 0.4f);

                                        eta.visiblility.Show();
                                        eta.value.text = skin.MathDisplay(d.stormEta.Current) + "s";

                                        if (presenter.lastMode != mode)
                                            stormAnnouncement.ShowAnnouncement("Storm incoming");
                                        else
                                            stormAnnouncement.Reset();
                                    }
                                    break;
                                case 2:
                                    {
                                        modeName.value.text = "Storm";
                                        modeName.valueBkgd.color = new Color(1f, 0, 0, 0.4f);
                                    }
                                    break;
                            }
                            presenter.lastMode = mode;
                        }
                        else
                        {
                            modeName.value.text = "OFF";
                            modeName.valueBkgd.color = new Color(0.3f, 0.3f, 0.3f, 0.4f);
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
