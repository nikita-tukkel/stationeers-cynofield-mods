using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.presenter
{
    public class ViewLayoutFactory
    {
        private readonly BaseSkin skin;
        public ViewLayoutFactory(BaseSkin skin)
        {
            this.skin = skin;
        }

        public VerticalLayoutGroup RootLayout(GameObject parent, bool debug = false)
        {
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            {
                layout.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childScaleWidth = false;
                layout.childScaleHeight = false;

                var fitter = layout.GetOrAddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (debug)
                {
                    var bkgdDebug = layout.GetOrAddComponent<RawImage>();
                    bkgdDebug.color = new Color(0, 1, 0, 0.1f);
                }
            }

            return layout;
        }

        public HorizontalLayoutGroup CreateRow(GameObject parent, bool debug = false)
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

            if (debug)
            {
                var bkgdDebug = hl.GetOrAddComponent<RawImage>();
                bkgdDebug.color = new Color(0, 1, 0, 0.1f);
            }
            return hl;
        }

        public ValueView Text1(GameObject parent, string text, float width = 0)
        {
            var tmp = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.margin = new Vector4(0f, 0f, 0f, 0f);
            tmp.richText = true;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.enableWordWrapping = true;
            skin.skin2d.MainFont(tmp);
            tmp.color = new Color(1f, 1f, 1f, 1f);
            tmp.text = text;
            var fitter = tmp.gameObject.AddComponent<ContentSizeFitter>();
            if (width <= 0)
            {
                tmp.rectTransform.sizeDelta = new Vector2(parent.GetComponent<RectTransform>().sizeDelta.x, 0);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else
            {
                tmp.rectTransform.sizeDelta = new Vector2(width, 0);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return new ValueView(null, tmp, null);
        }

        public ValueView Text2(GameObject parent, string message,
           Color bkgd = default, float width = 0, bool visible = true)
        {
            var size = new Vector2(width, 0);

            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            layout.GetOrAddComponent<RectTransform>().sizeDelta = size;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
            if (width <= 0)
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            else
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var hpool = layout.gameObject.AddComponent<HiddenPoolComponent>();
            hpool.SetVisible(visible);

            var text = Text1(layout.gameObject, message, width);
            RawImage img = null;
            if (bkgd != null)
            {
                img = layout.gameObject.AddComponent<RawImage>();
                img.rectTransform.sizeDelta = size;
                img.color = bkgd;
            }
            return new ValueView(layout, text.value, img);
        }
    }

    public class NameValuePairView : ValueView
    {
        public TextMeshProUGUI name;

        public NameValuePairView(LayoutGroup layout, TextMeshProUGUI name, ValueView value) : base(value)
        {
            this.name = name;
            this.layout = layout;
            if (layout != null)
            {
                layout.TryGetComponent(out HiddenPoolComponent hpool);
                this.visiblility = hpool;
            }
        }
    }

    public class ValueView : IPresenterView
    {
        public LayoutGroup layout;
        public HiddenPoolComponent visiblility;
        public TextMeshProUGUI value;
        public RawImage valueBkgd;
        public ValueView(ValueView prototype)
        {
            this.layout = prototype.layout;
            this.visiblility = prototype.visiblility;
            this.value = prototype.value;
            this.valueBkgd = prototype.valueBkgd;
        }
        public ValueView(LayoutGroup layout, TextMeshProUGUI value, RawImage valueBkgd)
        {
            this.layout = layout;
            if (layout != null)
            {
                layout.TryGetComponent(out HiddenPoolComponent hpool);
                this.visiblility = hpool;
            }
            this.value = value;
            this.valueBkgd = valueBkgd;
        }
    }
}
