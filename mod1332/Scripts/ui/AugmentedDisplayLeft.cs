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

        public static AugmentedDisplayLeft Create(VerticalLayoutGroup layoutLeft, ThingsUi thingsUi, Fonts2d fonts2d)
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

        public TextMeshProUGUI CreateText(Component parent, Vector2 size)
        {
            var text = Utils.CreateGameObject<TextMeshProUGUI>(parent);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(15f, 15f, 15f, 15f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;

            fonts2d.SetFont.superstar(20, text); // 5/5

            //text.lineSpacing = 20;

            text.color = new Color(1f, 1f, 1f, 1f);
            return text;
        }
    }
}
