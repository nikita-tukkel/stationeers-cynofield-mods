using Assets.Scripts.Objects;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager : IHierarchy
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedUiManager Create(PlayerProvider playerProvider,
            BaseSkin skin, Fonts2d fonts2d)
        {
            ThingsUi thingsUi = new ThingsUi(skin, fonts2d);
            return new AugmentedUiManager(thingsUi, playerProvider, skin, fonts2d);
        }

        private AugmentedUiManager(ThingsUi thingsUi, PlayerProvider playerProvider,
            BaseSkin skin, Fonts2d fonts2d)
        {
            this.thingsUi = thingsUi;
            this.skin = skin;

            var mainPanel = CreateMainPanel(skin, fonts2d, demoMode: false);
            components.Add(mainPanel.canvas);

            leftHud = AugmentedDisplayLeft.Create(mainPanel.layoutLeft, thingsUi, fonts2d);
            components.Add(leftHud);
            rightHud = AugmentedDisplayRight.Create(mainPanel.layoutRight, thingsUi, skin, fonts2d);
            components.Add(rightHud);
            inworldUi = AugmentedDisplayInWorld.Create(thingsUi, playerProvider);
            components.Add(inworldUi);
        }

        private class MainPanelComponents
        {
            public Canvas canvas;
            public VerticalLayoutGroup layoutLeft;
            public VerticalLayoutGroup layoutRight;

            public MainPanelComponents(Canvas canvas, VerticalLayoutGroup layoutLeft, VerticalLayoutGroup layoutRight)
            {
                this.canvas = canvas;
                this.layoutLeft = layoutLeft;
                this.layoutRight = layoutRight;
            }
        }

        private MainPanelComponents CreateMainPanel(BaseSkin skin, Fonts2d fonts2d, bool demoMode)
        {
            // https://stackoverflow.com/questions/66759954/unity-ui-how-to-make-a-composite-layout-group-to-combine-multiple-images-in

            var hudSkin = skin.Hud2d();
            var leftPanelWidth = hudSkin.leftPanelWidth;
            var rightPanelWidth = hudSkin.rightPanelWidth;
            var horizontalPadding = hudSkin.horizontalPaddingLeft;
            var verticalPaddingTop = hudSkin.verticalPaddingTop;
            var verticalPaddingBottom = hudSkin.verticalPaddingBottom;
            var centerWidth = Screen.width - leftPanelWidth - rightPanelWidth - 2 * horizontalPadding;
            var hudWidth = Screen.width - 2 * horizontalPadding;
            var hudHeight = Screen.height - verticalPaddingTop - verticalPaddingBottom;

            //===================
            // Root panel: Canvas + HorizontalLayoutGroup
            var canvas = Utils.CreateGameObject<Canvas>();
            var rootObj = canvas.gameObject;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.scaleFactor = 1;

            // For CanvasRenderer.EnableRectClipping zero is the center of the screen here.
            // Rect is (bottom left corner X, bottom left corner Y, width, height)
            var clippingRect = new Rect(
                -Screen.width / 2 + horizontalPadding,     // bottom left corner X from screen center
                -Screen.height / 2 + verticalPaddingBottom,// bottom left corner Y from screen center
                hudWidth,
                hudHeight
            );
            //Log.Debug(() => $"clippingRect = {clippingRect}");

            var canvasGroup = rootObj.AddComponent<CanvasGroup>();
            //canvasGroup.alpha = 0.1f; // may be used to fade out whole UI.

            // var renderer = rootObj.AddComponent<CanvasRenderer>();
            // renderer.EnableRectClipping(clippingRect);

            var rootLayout = rootObj.AddComponent<HorizontalLayoutGroup>();
            rootLayout.padding = new RectOffset(horizontalPadding, 0, verticalPaddingTop, 0);
            rootLayout.spacing = 0;
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = false;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childScaleWidth = false;
            rootLayout.childControlHeight = false;
            rootLayout.childForceExpandHeight = true;
            rootLayout.childScaleHeight = false;

            //===================
            // Left panel: RawImage + (optional) ContentSizeFitter + VerticalLayoutGroup
            var bkgd1 = Utils.CreateGameObject<RawImage>(rootLayout);
            bkgd1.color = demoMode ? new Color(1, 0, 0, 0.1f) : hudSkin.leftPanelBkgd;
            bkgd1.rectTransform.sizeDelta = new Vector2(leftPanelWidth, hudHeight);
            //bkgd1.rectTransform.pivot = Vector2.zero;
            // add ContentSizeFitter to RawImage for resize
            // var fitter1 = bkgd1.gameObject.AddComponent<ContentSizeFitter>();
            // fitter1.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            // fitter1.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout1 = bkgd1.gameObject.AddComponent<VerticalLayoutGroup>();
            layout1.padding = new RectOffset(1, 1, 1, 1);
            layout1.spacing = 1;
            layout1.childAlignment = TextAnchor.UpperLeft;
            layout1.childControlWidth = false;
            layout1.childForceExpandWidth = false;
            layout1.childScaleWidth = false;
            layout1.childControlHeight = false;
            layout1.childForceExpandHeight = false;
            layout1.childScaleHeight = false;

            //===================
            // Left panel childs: Text + ContentSizeFitter + CanvasRenderer.
            // ContentSizeFitter allows to dynamically resize text for content.
            // CanvasRenderer clips out childs who don't fit into root layout.
            if (demoMode)
            {
                for (var i = 0; i < 44; i++)
                {
                    var text = Utils.CreateGameObject<TextMeshProUGUI>(layout1);
                    text.rectTransform.sizeDelta = new Vector2(bkgd1.rectTransform.sizeDelta.x, 0);
                    text.alignment = TextAlignmentOptions.TopLeft;
                    text.margin = new Vector4(2f, 2f, 2f, 2f);
                    text.richText = true;
                    text.overflowMode = TextOverflowModes.Truncate;
                    text.enableWordWrapping = true;
                    skin.MainFont(text);
                    text.text = $"Text {i} Text {i} Text {i} Text {i} Text {i}\naaaa\nbbbb";

                    var textFitter = text.gameObject.AddComponent<ContentSizeFitter>();
                    textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var textRenderer = text.gameObject.GetComponent<CanvasRenderer>();
                    textRenderer.EnableRectClipping(clippingRect);
                }
            }

            var bkgd2 = Utils.CreateGameObject<RawImage>(rootLayout);
            bkgd2.color = demoMode ? new Color(0, 1, 0, 0.1f) : new Color(0, 0, 0, 0f);
            bkgd2.rectTransform.sizeDelta = new Vector2(centerWidth, hudHeight);

            var bkgd3 = Utils.CreateGameObject<RawImage>(rootLayout);
            bkgd3.color = demoMode ? new Color(0, 0, 1, 0.1f) : hudSkin.rightPanelBkgd;
            bkgd3.rectTransform.sizeDelta = new Vector2(rightPanelWidth, hudHeight);
            // add ContentSizeFitter to RawImage for resize
            // var fitter3 = bkgd3.gameObject.AddComponent<ContentSizeFitter>();
            // fitter3.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            // fitter3.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layout3 = bkgd3.gameObject.AddComponent<VerticalLayoutGroup>();
            layout3.padding = new RectOffset(1, 1, 1, 1);
            layout3.spacing = 1;
            layout3.childAlignment = TextAnchor.UpperLeft;
            layout3.childControlWidth = false;
            layout3.childForceExpandWidth = false;
            layout3.childScaleWidth = false;
            layout3.childControlHeight = false;
            layout3.childForceExpandHeight = false;
            layout3.childScaleHeight = false;

            if (demoMode)
            {
                for (var i = 0; i < 37; i++)
                {
                    var text = Utils.CreateGameObject<TextMeshProUGUI>(layout3);
                    text.rectTransform.sizeDelta = new Vector2(bkgd3.rectTransform.sizeDelta.x, 0);
                    text.alignment = TextAlignmentOptions.TopLeft;
                    text.margin = new Vector4(2f, 2f, 2f, 2f);
                    text.richText = true;
                    text.overflowMode = TextOverflowModes.Truncate;
                    text.enableWordWrapping = true;
                    skin.MainFont(text);
                    text.text = $"Text {i} Text {i} Text {i} Text {i} Text {i}\naaaa\nbbbb";

                    var textFitter = text.gameObject.AddComponent<ContentSizeFitter>();
                    textFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    var textRenderer = text.gameObject.GetComponent<CanvasRenderer>();
                    textRenderer.EnableRectClipping(clippingRect);
                }
            }

            if (demoMode)
                Demo2d(bkgd2.rectTransform, clippingRect, fonts2d);

            //Log.Debug(() => $"Hierarchy of {rootObj}:\n{Utils.PrintHierarchy(rootObj)}");

            return new MainPanelComponents(canvas, layout1, layout3);
        }

        private void Demo2d(RectTransform parent, Rect clippingRect, Fonts2d fonts2d)
        {
            //Log.Debug(() => $"demo size = {parent.sizeDelta}");
            var canvas = Utils.CreateGameObject<Canvas>(parent);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.scaleFactor = 1;
            fonts2d.Demo(canvas, parent.sizeDelta, clippingRect);
        }

        private readonly AugmentedDisplayLeft leftHud;
        private readonly AugmentedDisplayRight rightHud;
        private readonly AugmentedDisplayInWorld inworldUi;
        private readonly ThingsUi thingsUi;
        private readonly BaseSkin skin;
        private Thing lookingAt = null;
        private Thing pointingAt = null;

        // TODO move Describe call into rightHud to have hud updated in runtime
        internal void EyesOn(Thing thing)
        {
            if (lookingAt != thing)
            {
                lookingAt = thing;
                if (lookingAt != null)
                {
                    rightHud.Display(lookingAt);
                }
                else
                {
                    rightHud.Hide();
                }
            }
        }

        internal void MouseOn(Thing thing)
        {
            if (pointingAt != thing)
            {
                pointingAt = thing;
                if (pointingAt != null)
                {
                    rightHud.Display(pointingAt);
                }
                else
                {
                    rightHud.Hide();
                }
            }
        }

        private readonly List<Component> components = new List<Component>();

        IHierarchy IHierarchy.Parent => null;

        IEnumerator IEnumerable.GetEnumerator() => components.GetEnumerator();
    }
}
