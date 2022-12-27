using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using cynofield.mods.ui;
using HarmonyLib;
using Stationeers.Addons;

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
            AugmentedUiManager.Create();
            Instance = this;
        }

        void Destroy()
        {
            AugmentedUiManager.Instance?.Destroy();
            Instance = null;
        }

        internal void EyesOn(Thing thing)
        {
            AugmentedUiManager.Instance?.EyesOn(thing);
        }

        internal void MouseOn(Thing thing)
        {
            AugmentedUiManager.Instance?.MouseOn(thing);
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