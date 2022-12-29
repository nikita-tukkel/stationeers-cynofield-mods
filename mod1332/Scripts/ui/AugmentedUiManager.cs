using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using cynofield.mods.ui;
using cynofield.mods.utils;
using HarmonyLib;
using Stationeers.Addons;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager : IHierarchy
    {
        public static AugmentedUiManager Instance;

        public static void Create()
        {
            ThingsUi thingsUi = new ThingsUi();
            Instance = new AugmentedUiManager(thingsUi);
        }

        public AugmentedUiManager(ThingsUi thingsUi)
        {
            this.thingsUi = thingsUi;
            this.enabler = new AREnabler();
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
