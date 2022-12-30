using Assets.Scripts.Objects;
using cynofield.mods.utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager : IHierarchy
    {
        public static AugmentedUiManager Create(PlayerProvider playerProvider)
        {
            ThingsUi thingsUi = new ThingsUi();
            return new AugmentedUiManager(thingsUi, playerProvider);
        }

        private AugmentedUiManager(ThingsUi thingsUi, PlayerProvider playerProvider)
        {
            this.thingsUi = thingsUi;
            rightHud = AugmentedDisplayRight.Create();
            components.Add(rightHud);
            inworldUi = AugmentedDisplayInWorld.Create(thingsUi, playerProvider);
            components.Add(inworldUi);
            leftHud = AugmentedDisplayLeft.Create();
            components.Add(leftHud);
        }

        private readonly AugmentedDisplayLeft leftHud;
        private readonly AugmentedDisplayRight rightHud;
        private readonly AugmentedDisplayInWorld inworldUi;
        private readonly ThingsUi thingsUi;
        private Thing lookingAt = null;
        private Thing pointingAt = null;

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
