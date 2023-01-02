using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayLeft : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayLeft Create(Fonts2d fonts2d)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayLeft>();
            result.Init(fonts2d);
            return result;
        }

        private Fonts2d fonts2d;
        private LayoutFactory lf;
        Canvas canvas;
        private void Init(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
            this.lf = new LayoutFactory(fonts2d);

            // https://stackoverflow.com/questions/66759954/unity-ui-how-to-make-a-composite-layout-group-to-combine-multiple-images-in
            // canvas = lf.CreateCanvas(gameObject); // root game object = Canvas + VerticalLayoutGroup
            // var rootLayout = Utils.VL(canvas);
            // rootLayout.padding = new RectOffset(100, 0, 100, 0);

            // var size = new Vector2(300f, 300f);

            // var bkgd = Utils.CreateGameObject<RawImage>(rootLayout);
            // bkgd.rectTransform.sizeDelta = size;
            // bkgd.color = new Color(0, 0, 0, 0.6f);
            
            // var layout = Utils.VL(rootLayout.gameObject);
            // var bkgd5 = Utils.CreateGameObject<RawImage>(layout);
            // bkgd5.color = Color.green;

            // var layout = Utils.VL(bkgd);
            // layout.padding = new RectOffset(0, 0, 0, 0);
            // (var text1, var bkgd1) = lf.CreateText(layout, size);
            // text1.text = "testtesttesttesttesttesttesttesttest";
            // (var text2, var bkgd2) = lf.CreateText(layout, size);
            // text2.text = "TEST TEST 0123456789\nTTTTWWWWAAAAOOOOEEEE\nПРИВЕТ привет";

            //Demo2d();
        }

        // private void Demo2d()
        // {
        //     var subparent = Utils.CreateGameObject(gameObject);
        //     var demoCanvas = Utils.CreateGameObject<Canvas>(subparent);
        //     demoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //     demoCanvas.pixelPerfect = true;
        //     demoCanvas.scaleFactor = 1;

        //     fonts2d.Demo(demoCanvas, lf);
        // }
    }

    public class LayoutFactory
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly Fonts2d fonts2d;
        public LayoutFactory(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
        }

        public Canvas CreateCanvas2d(GameObject parent)
        {
            // Sizes and coordinates here are calculated in pixels, not in in-game meters.
            //  So don't get surprised by large numbers.
            // See canvas.renderingDisplaySize
            var canvas = parent.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.scaleFactor = 1;
            //Log.Debug(() => $"renderingDisplaySize={canvas.renderingDisplaySize}");
            return canvas;
        }

        public (TextMeshProUGUI, RawImage) CreateText(Component parent, Vector2 size)
        {
            var bkgd = Utils.CreateGameObject<RawImage>(parent);
            bkgd.rectTransform.sizeDelta = size;
            //bkgd.color = new Color(0, 0, 0.1f, 0.6f);
            // bkgd.color = new Color(0, 0, 0, 0.6f);
            bkgd.color = Color.red;

            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(15f, 15f, 15f, 15f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;

            fonts2d.SetFont.superstar(20, text); // 5/5
            //fonts2d.SetFont.cubecavern(20, text); // 5/5
            //fonts2d.SetFont.publicpixel(10,text); // 5/5
            //fonts2d.SetFont.retro_gaming(10,text); // 4/5
            //fonts2d.SetFont.minecraftia_regular(20, text); // 5/5
            //fonts2d.SetFont.smallest_pixel_7(20,text); // 4/5
            //fonts2d.SetFont.small_pixel_7(20,text); // 4/5
            //fonts2d.SetFont.modern_lcd_7(20,text); // 5/5
            //fonts2d.SetFont.light_pixel_7(10, text); // 4/5
            //fonts2d.SetFont.upheavalpro(20,text); // 3/5
            //fonts2d.SetFont.pixel_unicode(40,text); // 3/5
            //fonts2d.SetFont.half_bold_pixel_7(20, text); // 3/5
            //fonts2d.SetFont.sgk100(40,text); // 3/5
            //fonts2d.SetFont.wide_pixel_7(20,text); // 3/5
            //fonts2d.SetFont.thin_pixel_7(40,text); // 3/5
            //fonts2d.SetFont.zx_spectrum_7(20,text); // 3/5
            //fonts2d.SetFont.cloude_regular(40,text); // 3/5
            //fonts2d.SetFont.freepixel(20, text);  // 2/5
            //fonts2d.SetFont.cloude_regular_bold(60,text); // 2/5
            //fonts2d.SetFont.pixeleum_48(20,text); // 2/5
            //fonts2d.SetFont.webpixel_bitmap_medium(20,text); // 2/5
            //fonts2d.SetFont.zx_spectrum_7_bold(20,text); // 3/5

            //text.lineSpacing = 20;

            text.color = new Color(1f, 1f, 1f, 1f);
            return (text, null);
        }
    }
}
