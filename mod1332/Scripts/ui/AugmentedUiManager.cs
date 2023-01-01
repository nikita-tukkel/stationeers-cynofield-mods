using Assets.Scripts.Objects;
using cynofield.mods.utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager : IHierarchy
    {
        public static AugmentedUiManager Create(PlayerProvider playerProvider, Fonts2d fonts2d)
        {
            ThingsUi thingsUi = new ThingsUi();
            return new AugmentedUiManager(thingsUi, playerProvider, fonts2d);
        }

        private AugmentedUiManager(ThingsUi thingsUi, PlayerProvider playerProvider, Fonts2d fonts2d)
        {
            this.thingsUi = thingsUi;
            rightHud = AugmentedDisplayRight.Create(fonts2d);
            components.Add(rightHud);
            inworldUi = AugmentedDisplayInWorld.Create(thingsUi, playerProvider);
            components.Add(inworldUi);
            leftHud = AugmentedDisplayLeft.Create(fonts2d);
            components.Add(leftHud);
        }

        private readonly AugmentedDisplayLeft leftHud;
        private readonly AugmentedDisplayRight rightHud;
        private readonly AugmentedDisplayInWorld inworldUi;
        private readonly ThingsUi thingsUi;
        private Thing lookingAt = null;
        private Thing pointingAt = null;

        // TODO move Describe call into rightHud to have hud updated in runtime
        internal void EyesOn(Thing thing)
        {
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
