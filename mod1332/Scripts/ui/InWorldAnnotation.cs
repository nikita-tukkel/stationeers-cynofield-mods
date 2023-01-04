using System.Data;
using Assets.Scripts.Objects;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class InWorldAnnotation : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static InWorldAnnotation Create(Transform parent,
            ThingsUi thingsUi, PlayerProvider playerProvider,
            List<ColorScheme> colorSchemes, int colorSchemeId = -1)
        {
            var result = Utils.CreateGameObject<InWorldAnnotation>(parent);
            result.Init(thingsUi, playerProvider, colorSchemes, colorSchemeId);
            return result;
        }

        private static int ColorSchemeCounter = 0;
        private int _colorSchemeId;
        public int ColorSchemeId
        {
            set
            {
                if (_colorSchemeId != value)
                {
                    _colorSchemeId = value;
                    if (colorScheme != null)
                        colorScheme.SetColorScheme(GetCurrentColorScheme());
                }
            }
        }

        private ThingsUi thingsUi;
        private PlayerProvider playerProvider;
        private List<ColorScheme> colorSchemes;

        private Canvas canvas;
        private VerticalLayoutGroup layout;
        private ColorSchemeComponent colorScheme;

        public string id;
        public Thing anchor;

        private void Init(ThingsUi thingsUi, PlayerProvider playerProvider,
            List<ColorScheme> colorSchemes, int colorSchemeId = -1)
        {
            //Log.Info(() => "started");
            this.thingsUi = thingsUi;
            this.playerProvider = playerProvider;
            this.colorSchemes = colorSchemes;

            if (colorSchemeId < 0)
                this._colorSchemeId = Math.Clamp(ColorSchemeCounter++, 0, colorSchemes.Count - 1);
            else
                this._colorSchemeId = Math.Clamp(colorSchemeId, 0, colorSchemes.Count - 1);
            if (ColorSchemeCounter >= colorSchemes.Count)
                ColorSchemeCounter = 0;

            //canvas = gameObject.AddComponent<Canvas>();
            canvas = Utils.CreateGameObject<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            var size = new Vector2(0.7f, 0.7f);

            // Culling doesn't work for unknown reason, so have to duplicate the background for
            //  opposite side of the annotation.
            var bkgdFront = Utils.CreateGameObject<RawImage>(canvas);
            var bkgdBack = Utils.CreateGameObject<RawImage>(canvas);
            {
                bkgdFront.rectTransform.sizeDelta = size;
                bkgdBack.rectTransform.sizeDelta = size;
                bkgdBack.transform.Rotate(Vector3.up, 180);
            }

            //layout = canvas.gameObject.AddComponent<VerticalLayoutGroup>();
            layout = Utils.CreateGameObject<VerticalLayoutGroup>(canvas);
            //layout = bkgd.gameObject.AddComponent<VerticalLayoutGroup>();
            {
                var rect = layout.GetComponent<RectTransform>();
                rect.sizeDelta = size;
                layout.padding = new RectOffset(0, 0, 0, 0);
                layout.spacing = 0;
                layout.childAlignment = TextAnchor.UpperLeft;
                // true => will set the children rect size; 
                // false => children set the size of their rect by themselves;
                layout.childControlWidth = true;
                layout.childControlHeight = true;

                // true => expand children rect size to all available space; 
                // false => don't expand children rect;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                layout.childScaleWidth = false;
                layout.childScaleHeight = false;

                var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
                // Some childs will change this resize rules to produce smaller/bigger annotations
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                var ursa = layout.gameObject.AddComponent<UnityRectSoundsAnal>();
                ursa.SyncSizeIntoAnotherRect(bkgdFront.rectTransform);
                ursa.SyncSizeIntoAnotherRect(bkgdBack.rectTransform);

                // Pass color scheme information to children
                colorScheme = layout.gameObject.AddComponent<ColorSchemeComponent>();
                colorScheme.Add(bkgdFront);
                colorScheme.Add(bkgdBack);
                colorScheme.SetColorScheme(GetCurrentColorScheme());
            }
        }

        private ColorScheme GetCurrentColorScheme() => colorSchemes[Math.Clamp(_colorSchemeId, 0, colorSchemes.Count - 1)];

        public void ShowNear(Thing thing, string id)
        {
            this.anchor = thing;
            this.id = id;

            // relink to new parent, thus appear in the parent scene.
            transform.SetParent(thing.transform, false);
            transform.SetPositionAndRotation(
                // need to reset position on this component reuse
                thing.transform.position,

                // rotate vertically to the camera:
                Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0)
                );

            Render();

            // Fine tune coordinates only after content is rendered.
            // Expect that `bkgd` is here and covers whole annotation.
            var human = playerProvider.GetPlayerAvatar();
            //var posHead = human.HeadBone.transform.position;
            var posHead = human.GlassesSlot.Occupant.transform.position;
            var posLegs = human.transform.position;
            var humanHeight = (posHead.y - posLegs.y) * 1f;
            var limit1 = posLegs.y;
            var limit2 = limit1 + humanHeight;
            //transform.position = new Vector3(posHit.x, posHit.y, posHit.z);
            transform.position = posHead;
            Vector3[] corners = new Vector3[4];
            var rect = layout.GetComponent<RectTransform>();
            rect.GetWorldCorners(corners);
            var y1 = corners[0].y; // bottom left
            var y2 = corners[2].y; // top right
            var height = y2 - y1;
            var pos = transform.position;
            // these fine tuning is not actually needed anymore, since annotation is anchored closer to the head
            if (y2 > limit2)
            {
                // Log.Debug(() => $"ShowNear {y2} > {posHead.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(pos.x, limit2, pos.z);
            }
            else if (y1 < limit1)
            {
                // Log.Debug(() => $"ShowNear {y1} < {posLegs.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(pos.x, limit1 + height, pos.z);
            }
            transform.Translate(Camera.main.transform.forward * 0.5f, Space.World);

            Utils.Show(gameObject);
        }

        public void ShowOver(Thing thing, string id)
        {
            this.anchor = thing;
            this.id = id;

            // relink to new parent, thus appear in the parent scene.
            transform.SetParent(thing.transform, false);
            transform.position = thing.transform.position;
            Render();

            var human = playerProvider.GetPlayerAvatar();
            Vector3 fromPlayerToThing = thing.transform.position - human.transform.position;
            // TODO refactor
            var fd = Vector3.Dot(fromPlayerToThing, Vector3.forward);
            var res = Vector3.forward;
            var resd = fd;
            var bd = Vector3.Dot(fromPlayerToThing, Vector3.back);
            if (bd > resd)
            {
                res = Vector3.back;
                resd = bd;
            }
            var ld = Vector3.Dot(fromPlayerToThing, Vector3.left);
            if (ld > resd)
            {
                res = Vector3.left;
                resd = ld;
            }
            var rd = Vector3.Dot(fromPlayerToThing, Vector3.right);
            if (rd > resd)
            {
                res = Vector3.right;
                resd = rd;
            }
            transform.rotation = Quaternion.LookRotation(res);
            transform.Translate(res * -0.35f, Space.World);

            Utils.Show(gameObject);
        }

        public void Render()
        {
            thingsUi.RenderArAnnotation(anchor, layout);
            //Log.Debug(()=>$"{Utils.PrintHierarchy(gameObject)}");
            // Log.Debug(() => $"bkgd {bkgd.rectTransform.sizeDelta}");
            // Log.Debug(() => $"layoutInner {layoutInner.GetComponent<RectTransform>().sizeDelta}");
            // Log.Debug(() => $"bkgd {bkgd.rectTransform.position}");
            // Log.Debug(() => $"layoutInner {layoutInner.GetComponent<RectTransform>().position}");
        }

        public bool IsActive()
        {
            return this.isActiveAndEnabled && anchor != null;
        }

        public void Deactivate()
        {
            Utils.Hide(this);

            // for now go without lower level objects reuse.
            // once deactivated, all thingsUi objects are destroyed.
            foreach (Transform child in layout.transform)
            {
                Utils.Destroy(child);
            }

            anchor = null;
            id = null;
        }
    }

    public class UnityRectSoundsAnal : UIBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger LoggerInYourRect = new Logger_();

        private readonly HashSet<RectTransform> controlledRects = new HashSet<RectTransform>();

        public void SyncSizeIntoAnotherRect(RectTransform otherRect)
        {
            controlledRects.Add(otherRect);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            gameObject.TryGetComponent(out RectTransform myRect);
            if (myRect == null)
                return;
            // LoggerInYourRect.Debug(() => $"{gameObject} your rect is transformed to {myRect.sizeDelta}");
            // LoggerInYourRect.Debug(() => $"{Utils.PrintHierarchy(gameObject)}");

            OnResize();

            foreach (var yourRect in controlledRects)
            {
                yourRect.sizeDelta = myRect.sizeDelta;
            }

            base.OnRectTransformDimensionsChange();
        }

        public delegate void ResizeCallback();
        public event ResizeCallback OnResize;
    }
}
