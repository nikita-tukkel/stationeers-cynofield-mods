using System;
using Assets.Scripts;
using cynofield.mods.ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.utils
{
    /// <summary>
    /// Designed to tune available fonts and TextMeshProUGUI to perform pixel perfect text rendering.
    /// Text size is set to the nearest available for the choosen font.
    /// You may get actual text size by getting Bounds from displayed TextMeshProUGUI.
    /// </summary>
    public class Fonts2d
    {
        private readonly AssetsLoader assetsLoader;
        private FontSetter1 fontSetter1;
        public Fonts2d(AssetsLoader assetsLoader)
        {
            this.assetsLoader = assetsLoader;
            this.fontSetter1 = new FontSetter1(this);
        }

        public FontSetter1 SetFont { get => fontSetter1; }

        private TMP_FontAsset LoadOrDefault(string fontName, int size, TextMeshProUGUI text)
        {
            var font = assetsLoader.LoadAsset<TMP_FontAsset>(fontName);
            if (font == null)
                SetDefaultFont(size, text);

            return font;
        }

        public void SetDefaultFont(int size, TextMeshProUGUI text)
        {
            var gameInternalFont = Localization.CurrentFont;
            if (gameInternalFont != null)
            {
                // Try to draw with internal font first, if it is available.
                // Its natural size is unknown and visually it is not pixel perfect anyway.
                // So simply use the requested size, it will look blurred in any case.
                text.font = Localization.CurrentFont;
                text.fontSize = size;
            }
            else
            {
                // TextMeshProUGUI embedded default font is LiberationSans SDF, which natural size is 36
                // It is SDF, so it will be blurry anyway.
                // But at least it will be as clean as possible.
                text.font = null;
                text.fontSize = NearestSize(size, 36);
            }
        }

        private int NearestSize(int requested, int fontNatural)
        {
            var result = (int)Math.Round((float)requested / fontNatural);
            if (result < 1) result = 1;
            return result * fontNatural;
        }

        public void Demo(Component parent, Vector2 size, Rect clippingRect)
        {
            var width = (int)(size.x/2);
            var l0 = Utils.HL(parent);
            l0.childAlignment = TextAnchor.UpperCenter;
            l0.gameObject.GetComponent<RectTransform>().sizeDelta = size;

            var spacing = 15;
            var l1 = Utils.VL(l0);
            l1.spacing = spacing;
            var f1 = l1.gameObject.AddComponent<ContentSizeFitter>();
            f1.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            f1.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var l2 = Utils.VL(l0);
            l2.spacing = spacing;
            var f2 = l2.gameObject.AddComponent<ContentSizeFitter>();
            f2.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            f2.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            //string demoStr = "\nHello!";
            string demoStr = "\nHello! Привет! \u03940123456789"; // \u0394 = greek Delta
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.superstar(20, text);
                text.text = "superstar" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.cubecavern(20, text);
                text.text = "cubecavern" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.publicpixel(20, text);
                text.text = "publicpixel" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.retro_gaming(20, text);
                text.text = "retro_gaming" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.minecraftia_regular(20, text);
                text.text = "minecraftia_regular" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.smallest_pixel_7(20, text);
                text.text = "smallest_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.small_pixel_7(20, text);
                text.text = "small_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.modern_lcd_7(20, text);
                text.text = "modern_lcd_7" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.light_pixel_7(20, text);
                text.text = "light_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.upheavalpro(20, text);
                text.text = "upheavalpro" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                SetFont.pixel_unicode(40, text);
                text.text = "pixel_unicode" + demoStr;
            }
            {
                var text = CreateDemoText(l1, width, clippingRect);
                text.font = Localization.CurrentFont;
                text.fontSize = 20;
                text.text = "Localization.CurrentFont" + demoStr;
            }
            //-------------
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.half_bold_pixel_7(20, text);
                text.text = "half_bold_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.sgk100(30, text);
                text.text = "sgk100" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.wide_pixel_7(20, text);
                text.text = "wide_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.thin_pixel_7(20, text);
                text.text = "thin_pixel_7" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.zx_spectrum_7(10, text);
                text.text = "zx_spectrum_7" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.zx_spectrum_7_bold(10, text);
                text.text = "zx_spectrum_7_bold" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.cloude_regular(20, text);
                text.text = "cloude_regular" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.cloude_regular_bold(20, text);
                text.text = "cloude_regular_bold" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.freepixel(30, text);
                text.text = "freepixel" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                SetFont.pixeleum_48(20, text);
                text.text = "pixeleum_48" + demoStr;
            }
            {
                var text = CreateDemoText(l2, width, clippingRect);
                text.font = null;
                text.fontSize = 20;
                text.text = "LiberationSans SDF" + demoStr;
            }
        }

        private TextMeshProUGUI CreateDemoText(Component parent, int w, Rect clippingRect)
        {
            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = new Vector2(w, 0);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(5f, 5f, 5f, 5f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;
            text.outlineWidth = 0;

            //text.lineSpacing = 0;
            text.lineSpacing = 1; // Set to zero and then to 1 to calibrate `Line Height` setting of Text Mesh Pro Font Asset

            var textFitter = text.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // var textRenderer = text.gameObject.GetComponent<CanvasRenderer>();
            // textRenderer.EnableRectClipping(clippingRect);

            return text;
        }

#pragma warning disable IDE1006
        public class FontSetter1
        {
            private readonly Fonts2d parent;
            internal FontSetter1(Fonts2d parent)
            {
                this.parent = parent;
            }

            public void zx_spectrum_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("zx_spectrum-7-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void zx_spectrum_7_bold(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("zx_spectrum-7_bold-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void wide_pixel_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("wide_pixel-7-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void smallest_pixel_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("smallest_pixel-7-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void upheavalpro(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("upheavalpro-20.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 20);
            }
            public void thin_pixel_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("thin_pixel-7-20.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 20);
            }
            public void superstar(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("superstar_memesbruh03-16.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 16);
            }
            public void small_pixel_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("small_pixel-7-20.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 20);
            }
            public void retro_gaming(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("retro_gaming-11.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 11);
            }
            public void publicpixel(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("publicpixel-8.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 8);
            }
            public void pixeleum_48(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("pixeleum-48-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void pixel_unicode(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("pixel-unicode.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 16);
            }
            public void modern_lcd_7(int size, TextMeshProUGUI text)
            {
                // Nice, but only for large artistic LED-style text.
                var font = parent.LoadOrDefault("modern_lcd-7-20.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 20);
            }
            public void minecraftia_regular(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("minecraftia-regular-8.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 8);
            }
            public void light_pixel_7(int size, TextMeshProUGUI text)
            {
                // nice but this asset needs
                // text.lineSpacing = 20;

                // Suitable for headers or something big
                var font = parent.LoadOrDefault("light_pixel-7-20.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 20);
            }
            public void freepixel(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("freepixel-16.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 16);
            }
            public void half_bold_pixel_7(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("half_bold_pixel-7-10.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 10);
            }
            public void cubecavern(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("cubecavern_memesbruh03-16.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 16);
            }
            public void sgk100(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("sgk100-16.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 16);
            }
            public void cloude_regular(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("cloude_regular-32.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 32);
            }
            public void cloude_regular_bold(int size, TextMeshProUGUI text)
            {
                var font = parent.LoadOrDefault("cloude_regular_bold-32.asset", size, text);
                if (font == null) return;
                text.font = font;
                text.fontSize = parent.NearestSize(size, 32);
            }
        }
    }
}

