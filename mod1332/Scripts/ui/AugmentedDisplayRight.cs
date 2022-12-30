using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayRight : MonoBehaviour
    {
        public static AugmentedDisplayRight Create()
        {
            var result = Utils.CreateGameObject<AugmentedDisplayRight>();
            result.Init();
            return result;
        }

        Canvas canvas;
        TextMeshProUGUI text1;
        private void Init()
        {
            canvas = AugmentedDisplayLeft.CreateCanvas(gameObject);

            var size = new Vector2(300f, 300f);
            var layout = Utils.VL(canvas);
            layout.padding = new RectOffset((int)(Screen.width - size.x - 100), 0, 100, 0);
            RawImage bkgd1;
            (text1, bkgd1) = AugmentedDisplayLeft.CreateText(layout, size);
            text1.text = "testtesttesttesttesttesttesttesttest";
            (var text2, var bkgd2) = AugmentedDisplayLeft.CreateText(layout, size);
            text2.text = "test2test2test2test2test2test2test2test2";
        }

        public void Display(string text)
        {
            gameObject.SetActive(true);
            this.text1.text = text;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
