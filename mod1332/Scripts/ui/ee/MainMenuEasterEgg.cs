using System;
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
            var playerProvider = new PlayerProvider();
            var helmet = playerProvider.GetMainMenuHelmet();

            Destroy();
            Instance = Utils.CreateGameObject<MainMenuEasterEgg>(helmet);
            Instance.Init();

            if (GameManager.Instance.MenuCutscene.isActiveAndEnabled)
            {
                Instance.gameObject.SetActive(true);
            }
        }

        public static void Destroy()
        {
            Utils.Destroy(Instance);
            Instance = null;
        }

        public static void Show()
        {
            Instance.AnimationInit();
            Instance.gameObject.SetActive(true);
        }
        public static void Hide()
        {
            Instance.AnimationInit();
            Utils.Hide(Instance);
        }

        private Canvas canvas;
        private TextMeshProUGUI text;
        private void Init()
        {
            canvas = Utils.CreateGameObject<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            var size = new Vector2(0.7f, 0.4f);
            var bkgd = Utils.CreateGameObject<RawImage>(canvas);
            bkgd.rectTransform.sizeDelta = size;
            bkgd.color = new Color(0xad / 255f, 0xad / 255f, 0xe6 / 255f, 0.05f);
            text = Utils.CreateGameObject<TextMeshProUGUI>(canvas);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            text.margin = new Vector4(0.04f, 0.04f, 0.04f, 0.04f);
            text.enableWordWrapping = true;
            text.text = "";
            text.color = Color.white;
            text.font = Localization.CurrentFont;
            text.fontStyle = FontStyles.Bold;
            text.fontSize = 0.05f;
            text.lineSpacing = 30f;

            transform.position = gameObject.transform.position;
            var dir = gameObject.transform.forward;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.Translate(dir * 0.3f, Space.Self);
            transform.Translate(Vector3.down * 0.25f, Space.Self);
        }

        private float periodicUpdateCounter;
        private readonly string textHello = "Welcome to <color=red>HEROs v1.0</color>!\nYou have new mail.";
        private readonly string textWait = "/\u2014\\|"; // \u2014=em dash (long hypen)
        private int textIndex = -1;
        private int textWaitIndex;
        private int animationState;
        private readonly int framesHidden = 15;
        void Update()
        {
            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter < 0.2f)
                return;
            periodicUpdateCounter = 0;

            Animate();
            animationState++;
        }

        private void AnimationInit()
        {
            animationState = 0;
            textIndex = -1;
            textWaitIndex = 0;
            text.text = "";
        }

        private void Animate()
        {
            if (animationState < framesHidden)
            {
                if (animationState <= framesHidden - 5)
                    Utils.Hide(canvas);
                else
                    Utils.Show(canvas);
                return;
            }

            if (animationState > framesHidden)
            {
                if (textIndex < textHello.Length - 1)
                {
                    textIndex++;
                    while (textIndex < textHello.Length && textHello[textIndex] == ' ') textIndex++;
                    var c = textHello[textIndex];
                    if (c == '<')
                    {
                        while (textIndex < textHello.Length && textHello[textIndex] != '>') textIndex++;
                        textIndex = Math.Clamp(textIndex + 1, 0, textHello.Length - 1); // also take next char after tag
                    }
                    while (textIndex < textHello.Length && textHello[textIndex] == ' ') textIndex++;

                    text.text = textHello.Substring(0, textIndex + 1);
                }
                else
                {
                    if (text.text.Length <= textHello.Length)
                        text.text += "  ";
                    var oldText = text.text;
                    text.text = oldText.Substring(0, oldText.Length - 2) + " " + textWait[textWaitIndex];

                    textWaitIndex++;
                    if (textWaitIndex >= textWait.Length)
                        textWaitIndex = 0;
                }
            }
        }
    }
}
