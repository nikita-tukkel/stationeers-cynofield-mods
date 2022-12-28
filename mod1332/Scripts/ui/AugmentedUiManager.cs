using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using cynofield.mods.ui;
using HarmonyLib;
using Stationeers.Addons;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedUiManager
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
        }

        AugmentedDisplayRight rightHud;
        AugmentedDisplayInWorld inworldUi;
        List<Component> components = new List<Component>(); // TODO make it a GameObject?
        AREnabler enabler;
        ThingsUi thingsUi;

        Thing lookingAt = null;
        Thing pointingAt = null;

        public void Enable()
        {

        }


        public void Disable()
        {

        }

        public void Destroy()
        {
            foreach (var c in components)
            {
                UnityEngine.Object.Destroy(c.gameObject);
            }
        }

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

    }

}
