using Assets.Scripts;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.presenter
{
    public class ViewLayoutFactory3d
    {
        private readonly BaseSkin skin;
        public ViewLayoutFactory3d(BaseSkin skin)
        {
            this.skin = skin;
        }

        public VerticalLayoutGroup RootLayout(GameObject parent, bool debug = false)
        {
            var layout = Utils.CreateGameObject<VerticalLayoutGroup>(parent);
            {
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childScaleWidth = false;
                layout.childScaleHeight = false;

                // var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
                // fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                // fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (debug)
                {
                    var bkgdDebug = layout.gameObject.AddComponent<RawImage>();
                    bkgdDebug.color = new Color(0, 1, 0, 0.1f);
                }
            }

            return layout;
        }

        public ValueView Text1(GameObject parent, string message, float width = 0)
        {
            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = new Vector2(width, 0);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            text.margin = new Vector4(0, 0, 0, 0);
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;

            text.font = Localization.CurrentFont;
            text.fontSize = 0.06f;
            text.fontStyle = FontStyles.Bold;
            text.text = message;

            var fitter = text.gameObject.AddComponent<ContentSizeFitter>();
            // HF=Unconstrained will make text fit parent width and perform word wrapping
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return new ValueView(null, text, null);
        }

        public ValueView Text2(GameObject parent, string message,
           Color bkgd = default, float width = 0, bool visible = true)
        {
            var size = new Vector2(width, 0);

            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
            var layoutRect = layout.GetComponent<RectTransform>();
            layoutRect.sizeDelta = size;
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

        public NameValuePairView NameValuePair(GameObject parent, string name, string value,
           Color valueBkgd = default, bool visible = true)
        {
            var layout = Utils.CreateGameObject<HorizontalLayoutGroup>(parent);
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
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var hpool = layout.gameObject.AddComponent<HiddenPoolComponent>();
            hpool.SetVisible(visible);

            var nameView = Text1(layout.gameObject, name, width: 0.18f);
            nameView.value.margin = new Vector4(0.01f, 0.01f, 0.01f, 0.01f);
            var valueView = Text2(layout.gameObject, value, bkgd: valueBkgd, width: 0.35f);
            valueView.value.margin = new Vector4(0.01f, 0.01f, 0.01f, 0.01f);

            return new NameValuePairView(layout, name: nameView.value, value: valueView);
        }
    }
}
