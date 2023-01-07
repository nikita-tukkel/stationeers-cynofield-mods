using System.Globalization;
using Assets.Scripts.Objects;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class UiDefault : IThingCompleteUi, IThingWatcher, IThingWatcherProvider
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly ViewLayoutFactory lf;
        private readonly ViewLayoutFactory3d lf3d;
        private readonly DefaultDataModel dataModel = new DefaultDataModel();
        public UiDefault(ViewLayoutFactory lf, ViewLayoutFactory3d lf3d)
        {
            this.lf = lf;
            this.lf3d = lf3d;
        }

        public Type SupportedType() { return null; }

        public string Describe(Thing thing)
        {
            return
$@"{thing.DisplayName}
please <color=red><b>don't</b></color> play with me";
        }

        public GameObject RenderAnnotation(
            Thing thing,
            RectTransform parentRect) => RenderAnnotation(thing, parentRect, null);

        public GameObject RenderAnnotation(
            Thing thing,
            RectTransform parentRect,
            string description)
        {
            var data = dataModel.Snapshot(thing, description);

            var presenter = parentRect.GetComponentInChildren<DefaultPresenter>();
            if (presenter == null)
            {
                //Log.Debug(() => $"{Utils.GetId(thing)} creating new annotation view");
                presenter = CreateAnnotationView(thing, parentRect).GetComponent<DefaultPresenter>();
            }

            presenter.Present(data);
            return presenter.gameObject;
        }

        public GameObject RenderDetails(
            Thing thing,
            RectTransform parentRect,
            GameObject poolreuse) => RenderDetails(thing, parentRect, poolreuse, null);

        public GameObject RenderDetails(
            Thing thing,
            RectTransform parentRect,
            GameObject poolreuse,
            string description)
        {
            var data = dataModel.Snapshot(thing, description);
            DefaultPresenter presenter = null;
            if (poolreuse != null)
                poolreuse.TryGetComponent(out presenter);

            if (presenter == null)
                presenter = CreateDetailsView(thing, parentRect).GetComponent<DefaultPresenter>();

            presenter.Present(data);
            return presenter.gameObject;
        }

        public class DefaultDataModel
        {
            private readonly TimeSeriesDb db = new TimeSeriesDb();
            private readonly ConcurrentDictionary<string, RecordView> views = new ConcurrentDictionary<string, RecordView>();

            public class RecordView
            {
#pragma warning disable IDE1006
                public readonly TimeSeriesBuffer<string> name;
                public readonly TimeSeriesBuffer<string> description;
                public readonly string typeName;

                public RecordView(TimeSeriesRecord tsr, string typeName)
                {
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
                    description = tsr.Add("description", new TimeSeriesBuffer<string>(new string[2], 1));
                    this.typeName = typeName;
                }
            }

            public RecordView Get(Thing thing)
            {
                var thingId = Utils.GetId(thing);
                return views.GetOrAdd(thingId, (string _) =>
                {
                    var hr = db.Get(thingId, () => new TimeSeriesRecord());
                    var typeName = $"{thing.GetType()}".ToLower()
                           .Replace("assets.scripts.objects.", "")
                           .Replace("assets.scripts.", "");
                    return new RecordView(hr, typeName);
                });
            }

            public RecordView Snapshot(Thing thing, string description)
            {
                var now = Time.time;
                var data = Get(thing);
                data.name.Add(thing.DisplayName, now);
                if (description != null)
                    data.description.Add(description, now);
                return data;
            }
        }

        public class DefaultPresenter : PresenterBase<DefaultDataModel.RecordView>
        { }

        private GameObject CreateDetailsView(Thing thing, RectTransform parent)
        {
            var layout = lf.RootLayout(parent.gameObject, debug: false);
            var presenter = layout.gameObject.AddComponent<DefaultPresenter>();

            {
                var view = lf.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = (d.description.Current ?? d.name.Current) + " " + d.typeName);
            }

            return layout.gameObject;
        }

        private GameObject CreateAnnotationView(Thing thing, RectTransform parent)
        {
            parent.gameObject.TryGetComponent(out ColorSchemeComponent colorScheme);
            var layout = lf3d.RootLayout(parent.gameObject, debug: false);
            var presenter = layout.gameObject.AddComponent<DefaultPresenter>();

            // When want to change parent resize behaviour:
            // var parentFitter = parentRect.gameObject.GetComponent<ContentSizeFitter>();
            // parentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            {
                var view = lf3d.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = d.description.Current ?? d.name.Current);

                if (colorScheme != null)
                    colorScheme.Add(view.value);

                // var ursa = view.value.gameObject.AddComponent<UnityRectSoundsAnal>();
                // ursa.OnResize += delegate
                // {
                //     Log.Debug(() => $"0 {Utils.GetId(thing)} {parent.sizeDelta}");
                //     Log.Debug(() => $"1 {Utils.GetId(thing)} {layout.GetComponent<RectTransform>().sizeDelta}");
                //     Log.Debug(() => $"2 {Utils.GetId(thing)} {view.value.rectTransform.sizeDelta}");
                // };
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent); // Needed to process possible changes in text heights
            return layout.gameObject;
        }

        public GameObject RenderWatch(Thing thing, RectTransform parent, TagParser.Tag watcherTag) => RenderWatch(thing, parent, watcherTag, null);
        public GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag, string description)
        {
            //Log.Debug(() => $"RenderWatch {thing.DisplayName}: {description}");
            var data = dataModel.Snapshot(thing, description);
            var presenter = parentRect.GetComponentInChildren<DefaultPresenter>();

            if (presenter == null)
            {
                Log.Debug(() => $"Creating new watch for {thing.DisplayName}");
                var view = lf.Text1(parentRect.gameObject, $"{description}");
                presenter = view.value.gameObject.AddComponent<DefaultPresenter>();
                presenter.AddBinding((d) => view.value.text = d.description.Current ?? d.name.Current);

                Utils.Show(parentRect);
            }

            presenter.Present(data);
            return presenter.gameObject;
        }

        Dictionary<string, IThingWatcher> IThingWatcherProvider.GetWatchers()
        {
            return null;
        }
    }
}
