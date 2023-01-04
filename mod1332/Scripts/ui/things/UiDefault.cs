using Assets.Scripts;
using Assets.Scripts.Objects;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using System.Collections.Concurrent;
using TMPro;
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
            var presenter = parentRect.GetComponentInChildren<DefaultPresenter>();

            if (presenter == null)
            {
                var text = Utils.CreateGameObject<TextMeshProUGUI>(parentRect);
                //var text = Utils.CreateGameObject<TextMeshProUGUI>();
                presenter = text.gameObject.AddComponent<DefaultPresenter>();
                text.rectTransform.sizeDelta = Vector2.zero;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.richText = true;
                text.margin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
                text.overflowMode = TextOverflowModes.Truncate;
                text.enableWordWrapping = true;

                text.font = Localization.CurrentFont;
                text.fontSize = 0.06f;
                text.text = description;

                var fitter = text.gameObject.AddComponent<ContentSizeFitter>();
                // HF=Unconstrained will make text fit parent width and perform word wrapping
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // When want to change parent resize behaviour:
                // var parentFitter = parentRect.gameObject.GetComponent<ContentSizeFitter>();
                // parentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                parentRect.gameObject.TryGetComponent(out ColorSchemeComponent colorScheme);
                if (colorScheme != null)
                {
                    // Apply parent color scheme
                    colorScheme.Add(text);
                }
            }

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

                public RecordView(TimeSeriesRecord tsr)
                {
                    name = tsr.Add("name", new TimeSeriesBuffer<string>(new string[2], 1));
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
                if (description == null)
                    data.name.Add(thing.DisplayName, now);
                else
                    data.name.Add(description, now);
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

            return layout.gameObject;
        }
    }
}
