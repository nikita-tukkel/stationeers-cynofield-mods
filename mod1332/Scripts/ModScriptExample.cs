using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using HarmonyLib;
using Stationeers.Addons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace cynofield.mods
{
    public class AugmentedReality : IPlugin
    {
        public static AugmentedReality Instance;
        public ThingsUi thingsUi;

        void IPlugin.OnLoad()
        {
            ConsoleWindow.Print(ToString() + ": loaded!");

            WorldManager.OnWorldStarted += OnWorldStartedHandler;
            WorldManager.OnWorldExit += OnWorldExitHandler;
            Create();
        }

        void IPlugin.OnUnload()
        {
            WorldManager.OnWorldStarted -= OnWorldStartedHandler;
            WorldManager.OnWorldExit -= OnWorldExitHandler;
            Destroy();
        }

        void OnWorldStartedHandler()
        {
            if (Instance != null)
                Destroy();
            //ConsoleWindow.Print(ToString() + ": OnWorldStartedHandler");
            Create();
        }

        void OnWorldExitHandler()
        {
            //ConsoleWindow.Print(ToString() + ": OnWorldExitHandler");
            if (Instance != null)
                Destroy();
        }


        void Create()
        {
            thingsUi = new ThingsUi();
            AugmentedDisplayRight.Create();
            AugmentedDisplayInWorld.Create(thingsUi);
            Instance = this;
        }

        void Destroy()
        {
            thingsUi.Destroy();
            AugmentedDisplayRight.Destroy();
            AugmentedDisplayInWorld.Destroy();
            Instance = null;
        }

        public bool IsActive()
        {
            if (GameManager.GameState != Assets.Scripts.GridSystem.GameState.Running)
                return false;
            // if (WorldManager.IsGamePaused)
            //     return false;
            if (InventoryManager.ParentHuman == null)
                return false;
            if (InventoryManager.ParentHuman.GlassesSlot == null)
                return false;

            return true;
        }

        public bool IsEnabled()
        {
            if (!IsActive())
                return false;

            var glasses = InventoryManager.ParentHuman.GlassesSlot.Occupant;
            if (glasses == null)
                return false;

            if (!(glasses is Assets.Scripts.Objects.Items.SensorLenses))
                return false;

            var lenses = glasses as SensorLenses;
            // only check battery state and ignore inserted sensor:
            var power = (lenses.Battery == null) ? 0 : lenses.Battery.PowerStored;
            return lenses.OnOff && power > 0;
        }

        void OnOccupantChangeHandler()
        {
            var occupant = InventoryManager.ParentHuman.GlassesSlot.Occupant;
            if (occupant == null)
            {
                ConsoleWindow.Print($"no glasses");
            }
            else
            {
                ConsoleWindow.Print($"{occupant}");
            }
        }

        Thing lookingAt = null;
        Thing pointingAt = null;

        internal void EyesOn(Thing thing)
        {
            if (!IsEnabled())
                return;

            if (lookingAt != thing)
            {
                lookingAt = thing;
                if (lookingAt != null)
                {
                    var desc = "TODO"; // thingsUi.Description2d(lookingAt);
                    AugmentedDisplayRight.Instance.Display(
                        $"<color=white><color=green><b>eyes on</b></color>: {desc}</color>");
                }
                else
                {
                    AugmentedDisplayRight.Instance.Hide();
                }
            }
        }

        internal void MouseOn(Thing thing)
        {
            if (!IsEnabled())
                return;

            if (pointingAt != thing)
            {
                pointingAt = thing;
                if (pointingAt != null)
                {
                    var desc = "TODO"; // thingsUi.Description2d(pointingAt);
                    AugmentedDisplayRight.Instance.Display(
                        $"<color=white><color=green><b>mouse on</b></color>: {desc}</color>");
                }
                else
                {
                    AugmentedDisplayRight.Instance.Hide();
                }
            }
        }

    }

    public class AugmentedDisplayRight : MonoBehaviour
    {
        public static AugmentedDisplayRight Instance;

        public static void Create()
        {
            var gameObject = new GameObject("AugmentedDisplayRight");
            gameObject.SetActive(false);
            Instance = gameObject.AddComponent<AugmentedDisplayRight>();
        }

        public static void Destroy()
        {
            if (Instance == null)
                return;
            UnityEngine.Object.Destroy(Instance.gameObject);
            Instance = null;
        }

        private string text;
        public void Display(string text)
        {
            gameObject.SetActive(true);
            this.text = text;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        void OnGUI() // called by Unity
        {
            gameObject.SetActive(AugmentedReality.Instance.IsEnabled());
            if (!AugmentedReality.Instance.IsEnabled())
            {
                return;
            }

            var x = Screen.width * 2 / 3;
            var w = Screen.width - x - 110;
            var y = 100;
            var h = Screen.height - y - 300;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);

            //GUI.Label(new Rect(x, y, w, h), text);
            GUI.Box(new Rect(x, y, w, h), text
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 16
            });
            GUI.Box(new Rect(x, y + 100, w, h),
            $@"<color=grey>
aaa Loaded
</color>"
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 14,

            });
        }
    }

#pragma warning disable IDE0051, IDE0060

    // [HarmonyPatch(typeof(Assets.Scripts.Objects.Slot))]
    // public class AugmentationPatcherGlassesSlot
    // {
    //     [HarmonyPatch(nameof(Slot.OnOccupantChange))] // Update is a private method
    //     [HarmonyPostfix]
    //     static void ChangePostfix(Slot __instance)
    //     {
    //         ConsoleWindow.Print($"{__instance}");
    //     }
    // }

    [HarmonyPatch(typeof(PlayerStateWindow))]
    public class AugmentationPatcherPlayerStateWindow
    {
        static bool once = false;

        [HarmonyPatch("Update")] // Update is a private method
        [HarmonyPostfix]
        static void UpdatePostfix(PlayerStateWindow __instance)
        {
            if (WorldManager.IsGamePaused)
                return;

            if (!once)
            {
                once = true;
                //ConsoleWindow.Print($"{__instance.InfoExternalPressure.font}");
            }
            // var infoExternalParent = __instance.InfoExternal.Transform.parent;
            // var statusInfoPanel = infoExternalParent.parent;
            // var gameCanvas = statusInfoPanel.parent;
            // ConsoleWindow.Print($"here: {statusInfoPanel} {statusInfoPanel.name} {statusInfoPanel.position}");

            // var gameObject = new GameObject("Panel");
            // TextMeshProUGUI panel = gameObject.AddComponent<TextMeshProUGUI>();
            // panel.transform.SetParent(gameCanvas);
            // var pos = panel.transform.localPosition;
            // panel.transform.localPosition = new Vector3(pos.x, pos.y - 300, pos.z);
            // panel.text = "bitchen";
            // DeeperView.Instance.panel = gameObject;

            // GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.15f);
            // GUI.Label(new Rect(Screen.width * 2f / 3, 50, Screen.width, Screen.height * 0.8f),
            //  $"bitchen");
            // GUI.Label(new Rect(10, 10, Screen.width, Screen.height * 0.8f),
            //  $"bitchen");

        }
    }

    [HarmonyPatch(typeof(Structure), nameof(Structure.GetPassiveTooltip))]
    public class AugmentationPatcherCable
    {
        static void Postfix(ref Structure __instance, ref PassiveTooltip __result)
        {
            switch (__instance)
            {
                case Cable c:
                    //ConsoleWindow.Print($"here: {__result.Title}");
                    //__result.Title = "xyiaytle";
                    break;
            }
        }
    }

    // 		Assets.Scripts.CursorManager.SetCursorTarget() : void @06001AAD
    // Assets.Scripts.Objects.Items SPUOre : SensorProcessingUnit
    //Assets.Scripts.Objects.Items.SensorLenses.UpdateEachFrame
    // 		Assets.Scripts.UI.InputMouse.Idle() : void @06002504
    //  InputMouse.MaxInteractDistance

    [HarmonyPatch(typeof(Assets.Scripts.CursorManager),
    nameof(Assets.Scripts.CursorManager.SetCursorTarget))]
    public class AugmentationPatcherCursorManager
    {
        static void Postfix(ref Assets.Scripts.CursorManager __instance)
        {
            var view = AugmentedReality.Instance;
            view.EyesOn(__instance.FoundThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.UI.InputMouse), "Idle")] // Idle is a private method
    public class AugmentationPatcherInputMouse
    {
        static void Postfix(ref Assets.Scripts.UI.InputMouse __instance)
        {
            var view = AugmentedReality.Instance;
            view.MouseOn(__instance.CursorThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.Objects.Items.SensorLenses),
    nameof(Assets.Scripts.Objects.Items.SensorLenses.UpdateEachFrame))]
    public class AugmentationPatcherSensorLenses
    {
        static void Postfix(ref Assets.Scripts.Objects.Items.SensorLenses __instance)
        {
            //ConsoleWindow.Print(ToString() + ": loaded!");
        }
    }


    [HarmonyPatch(typeof(FiltrationMachine), nameof(FiltrationMachine.GetPassiveTooltip))]
    public class AugmentationPatcherFiltrationMachine
    {
        static void Postfix(ref PassiveTooltip __result)
        {
        }
    }


}