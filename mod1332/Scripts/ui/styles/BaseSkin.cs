using System;
using System.Collections.Generic;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;

namespace cynofield.mods.ui.styles
{
    public class BaseSkin
    {
        protected readonly Fonts2d fonts2d;
        protected readonly BaseSkin2d _skin2d;

        public BaseSkin(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
            this._skin2d = new BaseSkin2d(fonts2d);
        }

        virtual public BaseSkin2d skin2d { get => _skin2d; }

        virtual public string MathDisplay(double v)
        {
            return Math.Round(v, 3).ToString();
        }

        virtual public string PowerDisplay(double power)
        {
            if (double.IsNaN(power))
                return "NaN";

            var abs = Math.Abs(power);
            if (abs > 900_000)
            {
                return $"{Math.Round(power / 1_000_000f, 2)}MW";
            }
            else if (abs > 900)
            {
                return $"{Math.Round(power / 1_000f, 2)}kW";
            }
            else
            {
                return $"{Math.Round(power, 2)}W";
            }
        }
    }

    public class BaseSkin2d
    {
        protected readonly Fonts2d fonts2d;

        public BaseSkin2d(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
        }

        virtual public StyleHud2d Hud2d(StyleRefinement refinement = null)
        {
            return new StyleHud2d();
        }

        virtual public void MainFont(TextMeshProUGUI text, StyleRefinement refinement = null)
        {
            fonts2d.SetFont.superstar(20, text);
        }
    }

    public class CustomizedSkin : BaseSkin
    {
        public CustomizedSkin(Fonts2d fonts2d) : base(fonts2d)
        { }

        override public BaseSkin2d skin2d { get => _skin2d; }
    }

    public class StyleRefinement
    {
        public enum StyleContext
        {
        }

        public List<StyleContext> contexts = new List<StyleContext>();
    }

    public class StyleHud2d
    {
        private static int AdjustWidth(int v) => (int)Mathf.RoundToInt(v * Screen.width / 1920f);
        private static int AdjustHeight(int v) => (int)Mathf.RoundToInt(v * Screen.height / 1080f);

        public int leftPanelWidth = AdjustWidth(350);
        public int rightPanelWidth = AdjustWidth(350);
        public int horizontalPaddingLeft = AdjustWidth(100);
        public int horizontalPaddingRight = AdjustWidth(100);
        public int verticalPaddingTop = AdjustHeight(100);
        public int verticalPaddingBottom = AdjustHeight(200);

        public Color leftPanelBkgd = new Color(0, 0, 0, 0.6f);
        public Color rightPanelBkgd = new Color(0, 0, 0, 0.6f);
    }
}

