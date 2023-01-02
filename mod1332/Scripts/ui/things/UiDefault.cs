using Assets.Scripts.Objects;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.things
{
    class UiDefault : IThingCompleteUi
    {
        private readonly BaseSkin skin;

        public UiDefault(BaseSkin skin)
        {
            this.skin = skin;
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
            GameObject poolreuse)
        {
            string message = Describe(thing);
            return RenderDetailsOther(message, parentRect, poolreuse);
        }

        public GameObject RenderDetailsOther(
            string message,
            RectTransform parentRect,
            GameObject poolreuse)
        {
            TextMeshProUGUI text = null;
            if (poolreuse != null)
                poolreuse.TryGetComponent(out text);
            if (text == null)
            {
                text = CreateText(parentRect);
            }
            text.text = message;
            return text.gameObject;
        }

        private TextMeshProUGUI CreateText(RectTransform parent)
        {
            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = new Vector2(parent.sizeDelta.x, 0);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(2f, 2f, 2f, 2f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;
            skin.skin2d.MainFont(text);

            var textFitter = text.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // var textRenderer = text.gameObject.GetComponent<CanvasRenderer>();
            // textRenderer.EnableRectClipping(clippingRect);

            text.color = new Color(1f, 1f, 1f, 1f);
            return text;
        }
    }
}
