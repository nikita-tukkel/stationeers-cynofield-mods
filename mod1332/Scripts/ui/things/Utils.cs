using System;

namespace cynofield.mods.ui.things
{
    public class UiUtils
    {
        public string PowerDisplay(float power)
        {
            if (power > 900_000)
            {
                return $"{Math.Round(power / 1_000_000f, 2)}MW";
            }
            else if (power > 900)
            {
                return $"{Math.Round(power / 1_000f, 2)}kW";
            }

            else
            {
                return $"{Math.Round(power, 2)}W";
            }
        }
    }
}
