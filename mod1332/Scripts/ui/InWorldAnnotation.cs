using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Objects;
using cynofield.mods.utils;
using TMPro;
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
            int _colorSchemeId = -1)
        {
            var result = Utils.CreateGameObject<InWorldAnnotation>(parent);
            result.Init(thingsUi, playerProvider, _colorSchemeId);
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
                    ApplyColorScheme();
                }
            }
        }

        private Canvas canvas;
        private VerticalLayoutGroup layoutOuter;
        private VerticalLayoutGroup layout;
        private RawImage bkgd;
        private RawImage bkgd2;
        private TextMeshProUGUI text;
        private ThingsUi thingsUi;
        private PlayerProvider playerProvider;
        public string id;
        public Thing anchor;

        private void Init(ThingsUi thingsUi, PlayerProvider playerProvider,
            int _colorSchemeId = -1)
        {
            this.thingsUi = thingsUi;
            this.playerProvider = playerProvider;
            this._colorSchemeId = _colorSchemeId;

            //Log.Info(() => "started");
            //canvas = gameObject.AddComponent<Canvas>();
            canvas = Utils.CreateGameObject<Canvas>(gameObject);
            canvas.renderMode = RenderMode.WorldSpace;
            var size = new Vector2(0.7f, 0.7f);

            // Culling doesn't work for unknown reason, so have to duplicate the background for
            //  opposite side of the annotation.
            bkgd = Utils.CreateGameObject<RawImage>(canvas);
            bkgd2 = Utils.CreateGameObject<RawImage>(canvas);
            {
                bkgd.rectTransform.sizeDelta = size;
                bkgd2.rectTransform.sizeDelta = size;
                bkgd2.transform.Rotate(Vector3.up, 180);
            }

            // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html
            text = Utils.CreateGameObject<TextMeshProUGUI>(layoutOuter);
            //text.rectTransform.sizeDelta = size;
            text.rectTransform.sizeDelta = Vector2.zero;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            text.margin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;

            text.font = Localization.CurrentFont;
            //0.08f for default font used by TextMeshProUGUI without font specified;
            //0.06f when using Localization.CurrentFont
            text.fontSize = 0.06f;

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
                ursa.SyncSizeIntoAnotherRect(bkgd.rectTransform);
                ursa.SyncSizeIntoAnotherRect(bkgd2.rectTransform);
            }

            if (this._colorSchemeId < 0)
            {
                this._colorSchemeId = ColorSchemeCounter++;
                if (ColorSchemeCounter > 1)
                    ColorSchemeCounter = 0;
            }
            ApplyColorScheme();
        }

        public static Material MaterialForLightText()
        {
            // Create transparent material in unity at runtime:
            // https://docs.unity3d.com/Manual/StandardShaderMaterialParameters.html
            // https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/Editor/StandardShaderGUI.cs
            // https://gyanendushekhar.com/2021/02/08/using-transparent-material-in-unity-3d/
            // https://habr.com/ru/post/485018/

            Material material = new Material(Shader.Find("Standard"));
            //Material material = new Material(Shader.Find("Standard (Specular setup)"));
            material.SetFloat("_Mode", 2); // BlendMode.Fade=2, BlendMode.Transparent=3
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); // One or SrcAlpha
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);// must be 0 for semi-transparent material
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            material.SetFloat("_Metallic", 0.1f); // 1 will make it non-transparent metal plate
            material.SetFloat("_Glossiness", 0.1f); // 1 will produce brightest light reflection (like mirror)
            material.SetFloat("_OcclusionStrength", 0f);
            material.EnableKeyword("_EMISSION"); // Otherwise it will not be visible in darkness
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off); // doesn't work on RawImage?
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            var darker = 150;
            var nontransparent = 2f;
            // Color (aka Albedo Color, aka _Color) is set with Material.Color.
            // Darker and less transparent background when using lighter text,
            // lighter background when using darker text
            material.color = new Color(
                Mathf.Clamp((0xad - darker) / 255f, 0, 1),
                Mathf.Clamp((0xad - darker) / 255f, 0, 1),
                Mathf.Clamp((0xe6 - darker) / 255f, 0, 1),
             Mathf.Clamp(0.4f * nontransparent, 0, 1));

            // More light (like 0.05f) when using darker text,
            // less light (like 0.01f) when using lighter text
            // Also give emission color a notion of material color
            var moreLight = 1;
            var emissionColor = new Color(
                Mathf.Clamp(moreLight * material.color.r / 100f, 0.01f, 0.5f),
                Mathf.Clamp(moreLight * material.color.g / 100f, 0.01f, 0.5f),
                Mathf.Clamp(moreLight * material.color.b / 100f, 0.01f, 0.5f),
            1);
            material.SetColor("_EmissionColor", emissionColor); //new Color(0.01f, 0.01f, 0.01f, 1f)

            return material;
        }

        public static Material MaterialForDarkText()
        {
            Material material = new Material(Shader.Find("Standard"));
            material.SetFloat("_Mode", 3); // BlendMode.Fade=2, BlendMode.Transparent=3
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha); // One or SrcAlpha
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);// must be 0 for semi-transparent material
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            material.SetFloat("_Metallic", 0.1f); // 1 will make it non-transparent metal plate
            material.SetFloat("_Glossiness", 0.1f); // 1 will produce brightest light reflection (like mirror)
            material.SetFloat("_OcclusionStrength", 0f);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            material.doubleSidedGI = true;
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off); // doesn't work on RawImage?
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            var darker = 0;
            var nontransparent = 1f;
            material.color = new Color(
                Mathf.Clamp((0xad - darker) / 255f, 0, 1),
                Mathf.Clamp((0xad - darker) / 255f, 0, 1),
                Mathf.Clamp((0xe6 - darker) / 255f, 0, 1),
             Mathf.Clamp(0.4f * nontransparent, 0, 1));
            var moreLight = 15;
            var emissionColor = new Color(
                Mathf.Clamp(moreLight * material.color.r / 100f, 0.01f, 0.5f),
                Mathf.Clamp(moreLight * material.color.g / 100f, 0.01f, 0.5f),
                Mathf.Clamp(moreLight * material.color.b / 100f, 0.01f, 0.5f),
            1);
            material.SetColor("_EmissionColor", emissionColor); //new Color(0.05f, 0.05f, 0.05f, 1f)

            return material;
        }

        private void ApplyColorScheme()
        {
            switch (_colorSchemeId)
            {
                case 1:
                    {
                        bkgd.material = MaterialForDarkText();
                        bkgd2.material = bkgd.material;
                        text.color = new Color(0f, 0f, 0f, 0.65f); // low alpha is used to hide font antialiasing artifacts.
                        text.fontStyle = FontStyles.Bold;
                        break;
                    }
                // case 2:
                //     {
                //         // Background without material
                //         //  - not used, become blind under flashlight
                //         bkgd.color = new Color(0.2f, 0.4f, 0.2f, 0.1f);
                //         text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
                //         text.fontStyle = FontStyles.Bold;
                //         break;
                //     }
                default:
                    {
                        bkgd.material = MaterialForLightText();
                        bkgd2.material = bkgd.material;
                        // text.alpha = 0.2f; // doesn't seem to change anything, using `bkgd.color.a` or `material.color.a` instead (see above).
                        text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
                        text.fontStyle = FontStyles.Bold;
                        break;
                    }
            }
        }

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
            bkgd.rectTransform.GetWorldCorners(corners);
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
            foreach (var yourRect in controlledRects)
            {
                yourRect.sizeDelta = myRect.sizeDelta;
            }

            base.OnRectTransformDimensionsChange();
        }
    }
}
