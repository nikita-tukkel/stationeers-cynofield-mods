using System;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class InWorldAnnotation : MonoBehaviour
    {
        public static InWorldAnnotation Create(Transform parent, ThingsUi thingsUi)
        {
            var result = new GameObject().AddComponent<InWorldAnnotation>();
            result.gameObject.SetActive(false);
            if (parent != null)
                result.transform.SetParent(parent, false);
            result.Init(thingsUi);
            return result;
        }

        GameObject obj;
        Canvas canvas;
        RawImage bkgd;
        TextMeshProUGUI text;
        Thing anchor;

        ThingsUi thingsUi;

        public string id;

        public void Init(ThingsUi thingsUi)
        {
            this.thingsUi = thingsUi;

            //ConsoleWindow.Print($"InWorldAnnotation Start");
            obj = new GameObject("0");
            obj.SetActive(false);
            obj.transform.parent = gameObject.transform;
            canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            //canvas.pixelPerfect = true; // for RenderMode.ScreenSpaceOverlay
            //canvas.transform.localScale = Vector3.one * 0.5f; // less than 0.5 looks bad

            var size = new Vector2(0.7f, 0.7f);
            // Culling doesn't work for unknown reason, so have to duplicate the background for
            //  opposite side of the annotation.
            bkgd = new GameObject("2").AddComponent<RawImage>();
            bkgd.rectTransform.SetParent(canvas.transform, false);
            bkgd.rectTransform.sizeDelta = size;
            var bkgd2 = new GameObject("3").AddComponent<RawImage>();
            bkgd2.rectTransform.SetParent(canvas.transform, false);
            bkgd2.rectTransform.sizeDelta = size;
            bkgd2.transform.Rotate(Vector3.up, 180);

            // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html
            text = new GameObject("1").AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(canvas.transform, false);
            text.rectTransform.sizeDelta = size;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            text.margin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
            text.overflowMode = TextOverflowModes.Truncate;
            text.enableWordWrapping = true;

            // Resources.Load<TMP_FontAsset>(string.Format("UI/{0}", this.FontName))
            // Font font = new Font("StreamingAssets/Fonts/3270-Regular.ttf");
            // var tfont = TMP_FontAsset.CreateFontAsset(font);
            // text.font = tfont;
            //ConsoleWindow.Print($"{text.font.name}");
            text.font = Localization.CurrentFont;
            //0.08f for default font used by TextMeshProUGUI without font specified;
            //0.06f when using Localization.CurrentFont
            text.fontSize = 0.06f;

            ApplyNextColorScheme(bkgd, bkgd2, text);
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


        private static int ColorSchemeId = 0;
        private static void ApplyNextColorScheme(RawImage bkgd, TextMeshProUGUI text)
        {
            ApplyNextColorScheme(bkgd, null, text);
        }
        private static void ApplyNextColorScheme(RawImage bkgd1, RawImage bkgd2, TextMeshProUGUI text)
        {
            ApplyColorScheme(ColorSchemeId, bkgd1, text);
            if (bkgd2 != null)
                ApplyColorScheme(ColorSchemeId, bkgd2, text);
            ColorSchemeId++;
            if (ColorSchemeId > 1)
                ColorSchemeId = 0;
        }

        private static void ApplyColorScheme(int i, RawImage bkgd, TextMeshProUGUI text)
        {
            switch (i)
            {
                case 1:
                    {
                        bkgd.material = MaterialForDarkText();
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
                        // text.alpha = 0.2f; // doesn't seem to change anything, using `bkgd.color.a` or `material.color.a` instead (see above).
                        text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
                        text.fontStyle = FontStyles.Bold;
                        break;
                    }
            }
        }

        public void ShowNear(Thing thing, string id, RaycastHit hit)
        {
            if (thing == null)
            {
                Hide();
                return;
            }
            if (thing == anchor)
            {
                return;
            }
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
            var posHit = hit.point;
            //var posHead = InventoryManager.ParentHuman.HeadBone.transform.position;
            var posHead = InventoryManager.ParentHuman.GlassesSlot.Occupant.transform.position;
            var posLegs = InventoryManager.ParentHuman.transform.position;
            var humanHeight = (posHead.y - posLegs.y) * 1.2f;
            var limit1 = posLegs.y;
            var limit2 = limit1 + humanHeight;
            transform.position = new Vector3(posHit.x, posHit.y, posHit.z);
            Vector3[] corners = new Vector3[4];
            bkgd.rectTransform.GetWorldCorners(corners);
            var y1 = corners[0].y; // bottom left
            var y2 = corners[2].y; // top right
            var height = y2 - y1;
            var pos = transform.position;
            if (y2 > limit2)
            {
                // ConsoleWindow.Print($"ShowNear {y2} > {posHead.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(pos.x, limit2, pos.z);
            }
            else if (y1 < limit1)
            {
                // ConsoleWindow.Print($"ShowNear {y1} < {posLegs.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                transform.position = new Vector3(posHit.x, limit1 + height, posHit.z);
            }
            transform.Translate(Camera.main.transform.forward * -0.5f, Space.World);

            obj.SetActive(true);
            gameObject.SetActive(true);
        }

        public void ShowOver(Thing thing, string id)
        {
            if (thing == null)
            {
                Hide();
                return;
            }
            this.anchor = thing;
            this.id = id;

            // relink to new parent, thus appear in the parent scene.
            transform.SetParent(thing.transform, false);
            transform.position = thing.transform.position;
            Render();

            Vector3 fromPlayerToThing = thing.transform.position - InventoryManager.ParentHuman.transform.position;
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

            obj.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Render()
        {
            thingsUi.RenderArAnnotation(anchor, canvas, text);
        }

        public bool IsActive()
        {
            if (GameManager.GameState != Assets.Scripts.GridSystem.GameState.Running)
                return false;
            if (InventoryManager.ParentHuman == null)
                return false;

            return this.isActiveAndEnabled && anchor != null;
        }

        public void Hide()
        {
            anchor = null;
            gameObject.SetActive(false);
        }

        public void Destroy()
        {
            Destroy(gameObject);
            if (obj != null)
                Destroy(obj);
        }
    }
}
