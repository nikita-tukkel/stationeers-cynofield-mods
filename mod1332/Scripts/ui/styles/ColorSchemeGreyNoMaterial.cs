using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.styles
{
    /// <summary>
    /// Color scheme with non material background
    ///  - not used, become blind under flashlight
    /// </summary>
    public class ColorSchemeWhiteTextNoMaterial : ColorScheme
    {
        public override void Apply(TextMeshProUGUI text)
        {
            text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
            text.fontStyle = FontStyles.Bold;
        }

        public override void Apply(RawImage bkgd)
        {
            bkgd.color = new Color(0.2f, 0.4f, 0.2f, 0.1f);
        }
    }
}
