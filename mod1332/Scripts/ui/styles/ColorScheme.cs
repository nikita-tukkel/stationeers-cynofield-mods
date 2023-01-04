using System.Collections.Generic;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui.styles
{
    public class ColorSchemeComponent : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly HashSet<Component> controlledComponents = new HashSet<Component>();
        private ColorScheme cs;
        public void SetColorScheme(ColorScheme cs)
        {
            this.cs = cs;
            if (cs == null)
                return; // or maybe apply some default scheme?

            foreach (var c in controlledComponents)
            {
                //Log.Debug(() => $"{cs} for {c}");
                switch (c)
                {
                    case TextMeshProUGUI text:
                        cs.Apply(text);
                        break;
                    case RawImage bkgd:
                        cs.Apply(bkgd);
                        break;
                }
            }
        }

        public void Add(TextMeshProUGUI text)
        {
            if (controlledComponents.Add(text))
                cs?.Apply(text);
        }
        public void Add(RawImage bkgd)
        {
            if (controlledComponents.Add(bkgd))
                cs?.Apply(bkgd);
        }
    }

    public class ColorScheme
    {
        virtual public void Apply(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            text.color = Color.red;
        }

        virtual public void Apply(RawImage bkgd)
        {
            if (bkgd == null)
                return;

            bkgd.color = Color.green;
        }
    }
}
