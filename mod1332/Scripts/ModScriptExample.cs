using System;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using HarmonyLib;
using Stationeers.Addons;
using TMPro;
using UnityEngine;

namespace cynofield.mods
{
    public class DeeperView : IPlugin
    {
        public static DeeperView Instance;

        void IPlugin.OnLoad()
        {
            Instance = this;
            ConsoleWindow.Print(ToString() + ": loaded!");
            DeeperDisplay.Create();
            InventoryManager.ParentHuman.GlassesSlot.OnOccupantChange += handler_OnOccupantChange;
        }

        void handler_OnOccupantChange()
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

        void IPlugin.OnUnload()
        {
            Instance = null;
            DeeperDisplay.Destroy();
        }

        public string Prefix
        {
            get
            {
                return ToString();
            }
        }

        Thing lookingAt = null;
        Thing pointingAt = null;

        internal void EyesOn(Thing thing)
        {
            if (lookingAt != thing)
            {
                lookingAt = thing;
                if (lookingAt != null)
                {
                    DeeperDisplay.Instance.Display(
                        $"<color=white><color=green><b>eyes on</b></color>: {Describe(lookingAt)}</color>");
                }
                else
                {
                    DeeperDisplay.Instance.Hide();
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
                    DeeperDisplay.Instance.Display(
                        $"<color=white><color=green><b>mouse on</b></color>: {Describe(pointingAt)}</color>");
                }
                else
                {
                    DeeperDisplay.Instance.Hide();
                }
            }
        }

        public static string Describe(Thing thing)
        {
            switch (thing)
            {
                case Cable c:
                    var net = c.CableNetwork;
                    return $"network: {net.DisplayName} {net.CurrentLoad} / {net.PotentialLoad}";
                case Transformer t:
                    var color = t.Powered ? "green" : "red";
                    return
$@"{t.DisplayName}
<color={color}><b>{t.Setting}</b></color>
{PowerDisplay(t.UsedPower)}
{PowerDisplay(t.AvailablePower)}";
                default:
                    return thing.ToString();
            }
        }

        static private string PowerDisplay(float power)
        {
            if (power > 900_000)
            {
                return $"{Math.Round(power / 1_000_000, 1)}MW";
            }
            else
            {
                return $"{power}W";
            }
        }
    }

    public class DeeperDisplay : MonoBehaviour
    {
        public static DeeperDisplay Instance;

        public static void Create()
        {
            var gameObject = new GameObject("DeeperDisplay");
            gameObject.SetActive(false);
            Instance = gameObject.AddComponent<DeeperDisplay>();
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
            var x = Screen.width * 2 / 3;
            var w = Screen.width - x - 110;
            var y = 100;
            var h = Screen.height - y - 300;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.Box(new Rect(x, y, w, h), text
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 16
            });
            GUI.Box(new Rect(x, y + 100, w, h),
            $@"<color=white>
aaaaa {InventoryManager.ParentHuman.GlassesSlot.Occupant}
</color>"
            ,
            new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                richText = true,
                fontSize = 12
            });
        }
    }

#pragma warning disable IDE0051, IDE0060

    // [HarmonyPatch(typeof(Assets.Scripts.Objects.Slot))]
    // public class DeeperPatcherGlassesSlot
    // {
    //     [HarmonyPatch(nameof(Slot.OnOccupantChange))] // Update is a private method
    //     [HarmonyPostfix]
    //     static void ChangePostfix(Slot __instance)
    //     {
    //         ConsoleWindow.Print($"{__instance}");
    //     }
    // }

    [HarmonyPatch(typeof(PlayerStateWindow))]
    public class DeeperPatcherPlayerStateWindow
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
                ConsoleWindow.Print($"{__instance.InfoExternalPressure.font}");
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
    public class DeeperPatcherCable
    {
        static void Postfix(ref Structure __instance, ref PassiveTooltip __result)
        {
            switch (__instance)
            {
                case Cable c:
                    //ConsoleWindow.Print($"here: {__result.Title}");
                    __result.Title = "xyiaytle";
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
    public class DeeperPatcherCursorManager
    {
        static void Postfix(ref Assets.Scripts.CursorManager __instance)
        {
            var view = DeeperView.Instance;
            view.EyesOn(__instance.FoundThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.UI.InputMouse), "Idle")] // Idle is a private method
    public class DeeperPatcherInputMouse
    {
        static void Postfix(ref Assets.Scripts.UI.InputMouse __instance)
        {
            var view = DeeperView.Instance;
            view.MouseOn(__instance.CursorThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.Objects.Items.SensorLenses),
    nameof(Assets.Scripts.Objects.Items.SensorLenses.UpdateEachFrame))]
    public class DeeperPatcherSensorLenses
    {
        static void Postfix(ref Assets.Scripts.Objects.Items.SensorLenses __instance)
        {
            //ConsoleWindow.Print(ToString() + ": loaded!");
        }
    }


    [HarmonyPatch(typeof(FiltrationMachine), nameof(FiltrationMachine.GetPassiveTooltip))]
    public class DeeperPatcherFiltrationMachine
    {
        static void Postfix(ref PassiveTooltip __result)
        {
            __result.Title = $"{DeeperView.Instance.Prefix} {__result.Title}";
        }
    }


}