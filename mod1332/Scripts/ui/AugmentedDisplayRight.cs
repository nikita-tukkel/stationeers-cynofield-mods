using UnityEngine;

using cynofield.mods.utils;
namespace cynofield.mods.ui
{
    public class AugmentedDisplayRight : MonoBehaviour
    {
        public static AugmentedDisplayRight Create()
        {
            return Utils.CreateGameObject<AugmentedDisplayRight>();
        }

        private string text;
        public void Display(string text)
        {
            gameObject.SetActive(true);
            this.text = text;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void OnGUI() // called by Unity
        {
            var x = Screen.width * 2 / 3;
            var w = Screen.width - x - 110;
            var y = 100;
            var h = Screen.height - y - 300;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);

            //GUI.Label(new Rect(x, y, w, h), text);
            GUI.Box(new Rect(x, y, w, h), text
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 16
            });
            GUI.Box(new Rect(x, y + 100, w, h),
            $@"<color=grey>
aaa Loaded
</color>"
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 14,

            });
        }
    }
}
