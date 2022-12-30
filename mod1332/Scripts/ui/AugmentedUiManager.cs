using Assets.Scripts.Objects;
using cynofield.mods.utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager : IHierarchy
    {
        public static AugmentedUiManager Create()
        {
            ThingsUi thingsUi = new ThingsUi();
            AREnabler enabler = new AREnabler();
            return new AugmentedUiManager(thingsUi, enabler);
        }

        private AugmentedUiManager(ThingsUi thingsUi, AREnabler enabler)
        {
            this.thingsUi = thingsUi;
            this.enabler = enabler;
            this.rightHud = AugmentedDisplayRight.Create();
            components.Add(rightHud);
            this.inworldUi = AugmentedDisplayInWorld.Create(thingsUi);
            components.Add(inworldUi);

            // TODO activation from plugin lifecycle
            // TODO `Human` provider and enable/disable state managers
            Utils.Show(inworldUi);
        }

        AugmentedDisplayRight rightHud;
        AugmentedDisplayInWorld inworldUi;
        AREnabler enabler;
        ThingsUi thingsUi;

        Thing lookingAt = null;
        Thing pointingAt = null;

        internal void EyesOn(Thing thing)
        {
            if (!enabler.IsEnabled())
                return;

            if (lookingAt != thing)
            {
                lookingAt = thing;
                if (lookingAt != null)
                {
                    var desc = thingsUi.Describe(lookingAt);
                    rightHud.Display(
                        $"<color=white><color=green><b>eyes on</b></color>: {desc}</color>");
                }
                else
                {
                    rightHud.Hide();
                }
            }
        }

        internal void MouseOn(Thing thing)
        {
            if (!enabler.IsEnabled())
                return;

            if (pointingAt != thing)
            {
                pointingAt = thing;
                if (pointingAt != null)
                {
                    var desc = thingsUi.Describe(pointingAt);
                    rightHud.Display(
                        $"<color=white><color=green><b>mouse on</b></color>: {desc}</color>");
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
