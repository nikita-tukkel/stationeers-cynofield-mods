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

        public void Demo(Canvas parent, LayoutFactory lf)
        {
            var width = 500;
            var height = 70;
            var l0 = Utils.HL(parent);
            l0.padding = new RectOffset(400, 0, 100, 0);
            // var f0 = l0.gameObject.AddComponent<ContentSizeFitter>();
            // f0.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            var l1 = Utils.VL(l0);
            l1.spacing = 0;
            // var f1 = l1.gameObject.AddComponent<ContentSizeFitter>();
            // f1.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            var l2 = Utils.VL(l0);
            l2.spacing = 0;
            l2.padding = new RectOffset(400, 0, 0, 0);
            {
                var text = CreateDemoText(l1, width, height);
                SetFont.superstar(20, text);
                text.text = "superstar\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, height);
                SetFont.cubecavern(20, text);
                text.text = "cubecavern\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, 80);
                SetFont.publicpixel(20, text);
                text.text = "publicpixel\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, 80);
                SetFont.retro_gaming(20, text);
                text.text = "retro_gaming\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, 80);
                SetFont.minecraftia_regular(20, text);
                text.text = "minecraftia_regular\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, height);
                SetFont.smallest_pixel_7(20, text);
                text.text = "smallest_pixel_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, height);
                SetFont.small_pixel_7(20, text);
                text.text = "small_pixel_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, 80);
                SetFont.modern_lcd_7(20, text);
                text.text = "modern_lcd_7\nHello! Привет! \u03940123456789";
                text.lineSpacing = 20;
            }
            {
                var text = CreateDemoText(l1, width, 80);
                SetFont.light_pixel_7(20, text);
                text.text = "light_pixel_7\nHello! Привет! \u03940123456789";
                text.lineSpacing = 20;
            }
            {
                var text = CreateDemoText(l1, width, height);
                SetFont.upheavalpro(20, text);
                text.text = "upheavalpro\nHello! Привет! \u03940123456789";
                text.lineSpacing = 20;
            }
            {
                var text = CreateDemoText(l1, width, 100);
                SetFont.pixel_unicode(40, text);
                text.text = "pixel_unicode\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l1, width, 100);
                text.font = Localization.CurrentFont;
                text.fontSize = 20;
                text.text = "Localization.CurrentFont\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 80);
                SetFont.half_bold_pixel_7(20, text);
                text.text = "half_bold_pixel_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 100);
                SetFont.sgk100(30, text);
                text.text = "sgk100\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 80);
                SetFont.wide_pixel_7(20, text);
                text.text = "wide_pixel_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, height);
                SetFont.thin_pixel_7(20, text);
                text.text = "thin_pixel_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, height);
                SetFont.zx_spectrum_7(10, text);
                text.text = "zx_spectrum_7\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, height);
                SetFont.zx_spectrum_7_bold(10, text);
                text.text = "zx_spectrum_7_bold\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 90);
                SetFont.cloude_regular(20, text);
                text.text = "cloude_regular\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 90);
                SetFont.cloude_regular_bold(20, text);
                text.text = "cloude_regular_bold\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 90);
                SetFont.freepixel(30, text);
                text.text = "freepixel\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 90);
                SetFont.pixeleum_48(20, text);
                text.text = "pixeleum_48\nHello! Привет! \u03940123456789";
            }
            {
                var text = CreateDemoText(l2, width, 90);
                text.font = null;
                text.fontSize = 20;
                text.text = "LiberationSans SDF\nHello! Привет! \u03940123456789";
            }
        }

        private TextMeshProUGUI CreateDemoText(Component parent, int w, int h)
        {
            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = new Vector2(w, h);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(15f, 15f, 15f, 15f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;
            text.outlineWidth = 0;
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

