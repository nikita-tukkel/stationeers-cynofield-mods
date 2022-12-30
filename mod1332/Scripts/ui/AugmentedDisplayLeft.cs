using Assets.Scripts;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayLeft : MonoBehaviour
    {
        public static AugmentedDisplayLeft Create()
        {
            var result = Utils.CreateGameObject<AugmentedDisplayLeft>();
            result.Init();
            return result;
        }

        Canvas canvas;
        private void Init()
        {
            canvas = CreateCanvas(gameObject);

            var size = new Vector2(300f, 300f);
            var layout = Utils.VL(canvas);
            layout.padding = new RectOffset(100, 0, 100, 0);
            (var text1, var bkgd1) = CreateText(layout, size);
            text1.text = "testtesttesttesttesttesttesttesttest";
            (var text2, var bkgd2) = CreateText(layout, size);
            text2.text = "test2test2test2test2test2test2test2test2";
        }

        public static Canvas CreateCanvas(GameObject parent)
        {
            // Sizes and coordinates here are calculated in pixels, not in in-game meters.
            //  So don't get surprised by large numbers.
            // See canvas.renderingDisplaySize
            var canvas = parent.AddComponent<Canvas>();
            // also try https://forum.unity.com/threads/pixel-art-font-sizing-issues.635422/#post-5693926
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.scaleFactor = 1;
            // ConsoleWindow.Print($"{ToString()} renderingDisplaySize={canvas.renderingDisplaySize}");
            return canvas;
        }

        public static (TextMeshProUGUI, RawImage) CreateText(Component parent, Vector2 size)
        {
            var bkgd = Utils.CreateGameObject<RawImage>(parent);
            bkgd.rectTransform.sizeDelta = size;
            //bkgd.color = new Color(0, 0, 0.1f, 0.6f);
            bkgd.color = new Color(0, 0, 0, 0.6f);

            var text = Utils.CreateGameObject<TextMeshProUGUI>(bkgd);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.margin = new Vector4(15f, 15f, 15f, 15f);
            text.richText = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;
            text.font = Localization.CurrentFont;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = 20f;
            text.color = new Color(1f, 1f, 1f, 0.2f);
            return (text, bkgd);
        }
    }
}
