using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.styles
{
    public class ColorSchemeLightText : ColorScheme
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private static readonly Material _material = MaterialForLightText();

        public override void Apply(TextMeshProUGUI text)
        {
            //Log.Debug(()=>$"apply for {text}");
            // text.alpha = 0.2f; // doesn't seem to change anything, using `bkgd.color.a` or `material.color.a`.
            text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
            text.fontStyle = FontStyles.Bold;
        }

        public override void Apply(RawImage bkgd)
        {
            //Log.Debug(()=>$"apply for {bkgd}");
            bkgd.material = _material;
        }

        private static Material MaterialForLightText()
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
    }
}