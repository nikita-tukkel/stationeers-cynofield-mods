using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.styles
{
    public class ColorSchemeDarkText : ColorScheme
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private static readonly Material _material = MaterialForDarkText();

        public override void Apply(TextMeshProUGUI text)
        {
            //Log.Debug(()=>$"apply for {text}");
            text.color = new Color(0f, 0f, 0f, 0.65f); // low alpha is used to hide font antialiasing artifacts.
            text.fontStyle = FontStyles.Bold;
        }

        public override void Apply(RawImage bkgd)
        {
            //Log.Debug(()=>$"apply for {bkgd}");
            bkgd.material = _material;
        }

        private static Material MaterialForDarkText()
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
    }
}