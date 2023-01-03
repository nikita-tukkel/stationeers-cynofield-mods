
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

        public ValueView Text1(GameObject parent, string text, int width = 0)
        {
            var tmp = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            tmp.rectTransform.sizeDelta = new Vector2(width, 0);
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.margin = new Vector4(0f, 0f, 0f, 0f);
            tmp.richText = true;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.enableWordWrapping = false;
            skin.skin2d.MainFont(tmp);
            tmp.color = new Color(1f, 1f, 1f, 1f);
            tmp.text = text;
            var nameFitter = tmp.gameObject.AddComponent<ContentSizeFitter>();
            if (width <= 0)
                nameFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            else
                nameFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            nameFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return new ValueView(null, tmp, null);
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

    public class ValueView : IView
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
