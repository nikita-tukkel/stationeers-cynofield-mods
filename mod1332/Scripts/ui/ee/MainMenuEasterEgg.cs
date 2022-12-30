using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;
using cynofield.mods.utils;
using TMPro;

namespace cynofield.mods.ee
{
    public class MainMenuEasterEgg : MonoBehaviour
    {
        private static MainMenuEasterEgg Instance;
        public static void Create()
        {
            var playerProvider= new PlayerProvider();
            var helmet = playerProvider.GetMainMenuHelmet();

            Destroy();
            Instance = Utils.CreateGameObject<MainMenuEasterEgg>(helmet);
            Instance.Init();

            if (GameManager.Instance.MenuCutscene.isActiveAndEnabled)
            {
                Utils.Show(Instance);
            }
        }

        public static void Destroy()
        {
            Utils.Destroy(Instance);
            Instance = null;
        }

        public static void Show()
        {
            Utils.Show(Instance);
        }
        public static void Hide()
        {
            Utils.Hide(Instance);
        }

        private void Init()
        {
            var canvas = Utils.CreateGameObject<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            var size = new Vector2(0.5f, 0.5f);
            var bkgd = Utils.CreateGameObject<RawImage>(canvas);
            bkgd.rectTransform.sizeDelta = size;
            bkgd.color = new Color(0xad / 255f, 0xad / 255f, 0xe6 / 255f, 0.2f);
            var text = Utils.CreateGameObject<TextMeshProUGUI>(canvas);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.Top;
            text.richText = true;
            text.margin = new Vector4(0.01f, 0.01f, 0.01f, 0.01f);
            text.enableWordWrapping = true;
            text.text = "Hello AR World!";
            text.color = Color.white;
            text.font = Localization.CurrentFont;
            text.fontSize = 0.06f;

            transform.position = gameObject.transform.position;
            var dir = gameObject.transform.forward;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(dir * 0.3f, Space.Self);
            transform.Translate(Vector3.down * 0.2f, Space.Self);
        }
    }
}
