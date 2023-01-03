using Assets.Scripts.Objects;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class UiDefault : IThingCompleteUi
    {
        private readonly ViewLayoutFactory lf;
        private readonly DefaultDataModel dataModel = new DefaultDataModel();
        public UiDefault(ViewLayoutFactory lf)
        {
            this.lf = lf;
        }

        Type IThingDescriber.SupportedType() { return null; }

        public string Describe(Thing thing)
        {
            return
$@"{thing.DisplayName}
please <color=red><b>don't</b></color> play with me";
        }

        public GameObject RenderAnnotation(
            Thing thing,
            RectTransform parentRect,
            GameObject poolreuse)
        {
            throw new NotImplementedException();
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

                public RecordView(TimeSeriesRecord tsr)
                {
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
                    description = tsr.Add("description", new TimeSeriesBuffer<string>(new string[2], 1));
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
                var thingId = Utils.GetId(thing);
                var now = Time.time;
                var data = Get(thingId);
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
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            var presenter = layout.gameObject.AddComponent<DefaultPresenter>();

            {
                var view = lf.Text1(layout.gameObject, thing.DisplayName);
                presenter.AddBinding((d) => view.value.text = d.name.Current);
            }
            {
                var view = lf.Text1(layout.gameObject, "");
                presenter.AddBinding((d) => view.value.text = d.description.Current);
            }

            return layout.gameObject;
        }
    }
}
