using Assets.Scripts;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayBanner : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayBanner Create(PlayerProvider playerProvider, ArStateManager stateManager)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayBanner>();
            result.Init(playerProvider, stateManager);
            return result;
        }

        private PlayerProvider playerProvider;
        private ArStateManager stateManager;
        private CanvasGroup canvasGroup;
        private TextMeshProUGUI text;
        private void Init(PlayerProvider playerProvider, ArStateManager stateManager)
        {
            this.playerProvider = playerProvider;
            this.stateManager = stateManager;
        }

        private enum State
        {
            NEW,
            HIDDEN,
            VISIBLE,
            FADING,
        }

        private State state = State.NEW;
        public string message;
        private float fadeSpeed;
        private float periodicUpdateCounter;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;
            if (!stateManager.IsVisible())
                return;

            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter <= 0.1f)
                return;
            periodicUpdateCounter = 0;

            Render();
        }

        void OnEnable()
        {

        }

        void OnDisable()
        {
            if (text != null)
                text.text = "";
        }

        private void Render()
        {
            var oldState = state;
            switch (state)
            {
                case State.NEW:
                    {
                        if (stateManager.IsVisible())
                        {
                            RenderFirstTime();
                            state = State.HIDDEN;
                        }
                    }; break;

                case State.HIDDEN:
                    {
                        if (message != null)
                        {
                            text.text = message;
                            state = State.VISIBLE;
                        }
                    }; break;

                case State.VISIBLE:
                    {
                        text.text = message;
                        if (fadeSpeed > 0)
                            state = State.FADING;
                    }; break;

                case State.FADING:
                    {
                        text.text = message;
                        canvasGroup.alpha -= fadeSpeed;
                        if (canvasGroup.alpha <= 0 || fadeSpeed <= 0)
                        {
                            message = null;
                            state = State.HIDDEN;
                        }
                    }; break;
            }

            if (state == oldState) return;
            //Log.Debug(() => $"{oldState} -> {state}");

            switch (state)
            {
                case State.HIDDEN:
                    fadeSpeed = 0;
                    text.text = "";
                    Utils.Hide(canvasGroup);
                    break;

                case State.VISIBLE:
                    canvasGroup.alpha = 1;
                    Utils.Show(canvasGroup);
                    break;
            }
        }

        public void Hide()
        {
            message = null;
            text.text = "";
            fadeSpeed = 0;
        }

        public void Fade()
        {
            fadeSpeed = 0.02f;
        }

        private void RenderFirstTime()
        {
            Canvas canvas = Utils.CreateGameObject<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            var size = new Vector2(0.7f, 0.7f);

            canvasGroup = canvas.GetOrAddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f; // use to fade out the banner.

            VerticalLayoutGroup layout;
            {
                layout = Utils.CreateGameObject<VerticalLayoutGroup>(canvas);
                var rect = layout.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.spacing = 0;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = false;
                layout.childForceExpandWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;

                // var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
                // fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                // fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // var bkgd = layout.GetOrAddComponent<RawImage>();
                // bkgd.color = new Color(0.2f, 0.4f, 0.2f, 0.1f);
            }

            {
                text = Utils.CreateGameObject<TextMeshProUGUI>(layout);
                text.rectTransform.sizeDelta = new Vector2(0.15f, 0);
                text.alignment = TextAlignmentOptions.TopLeft;
                text.richText = true;
                text.margin = new Vector4(0, 0, 0, 0);
                text.overflowMode = TextOverflowModes.Truncate;
                text.enableWordWrapping = true;
                text.color = new Color(1f, 1f, 1f, 0.4f);
                text.font = Localization.CurrentFont;
                text.fontStyle = FontStyles.Bold;
                text.fontSize = 0.018f;
                text.lineSpacing = 0;

                var fitter = text.GetOrAddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            var human = playerProvider.GetPlayerAvatar();

            // Hard to decide where to anchor the onscreen banner.
            // Anchoring to the glasses or head gives more lively behavior, reaction to breathing, etc.
            // But it also gives big swinging when walking and deformation when turning with jetpack on.
            // {
            //     var head = human.GlassesSlot.Occupant.transform;
            //     transform.SetParent(head, false);
            //     transform.rotation = Quaternion.LookRotation(head.transform.forward);
            //     transform.Translate(head.transform.forward * 0.1f, Space.World);
            //     transform.Translate(head.transform.right * 0.32f, Space.World);
            //     transform.Translate(head.transform.up * -0.27f, Space.World);
            //     transform.Rotate(Vector3.right, -20, Space.Self);
            //     transform.Rotate(Vector3.up, 20, Space.Self);
            // }

            // Anchoring to the spine gives less swinging when walking.
            // But there is still a deformation when turning with jetpack on.
            {
                var glasses = human.GlassesSlot.Occupant.transform;
                var anchor = human.SpineBones[human.SpineBones.Count - 1];
                transform.SetParent(anchor, false);
                transform.rotation = Quaternion.LookRotation(glasses.transform.forward);
                transform.Translate(glasses.transform.forward * 0.15f, Space.World);
                transform.Translate(glasses.transform.up * 0.07f, Space.World);
                transform.Translate(glasses.transform.right * 0.34f, Space.World);
                transform.Rotate(Vector3.right, -20, Space.Self);
                transform.Rotate(Vector3.up, 25, Space.Self);
            }

            Utils.Show(this);
        }
    }
}
