using System.Globalization;
using System;
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

namespace cynofield.mods
{
    public class AugmentedReality : IPlugin
    {
        public static AugmentedReality Instance;

        void IPlugin.OnLoad()
        {
            ConsoleWindow.Print(ToString() + ": loaded!");

            WorldManager.OnWorldStarted += OnWorldStartedHandler;
            WorldManager.OnWorldExit += OnWorldExitHandler;

            Instance = this;
            AugmentedDisplayRight.Create();
            AugmentedDisplayInWorld.Create();
            //InventoryManager.ParentHuman.GlassesSlot.OnOccupantChange += OnOccupantChangeHandler;
        }

        void IPlugin.OnUnload()
        {
            WorldManager.OnWorldStarted -= OnWorldStartedHandler;
            WorldManager.OnWorldExit -= OnWorldExitHandler;
            Instance = null;
            AugmentedDisplayRight.Destroy();
            AugmentedDisplayInWorld.Destroy();
        }

        void OnWorldStartedHandler()
        {
        }

        void OnWorldExitHandler()
        {
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
                AugmentedDisplayInWorld.Instance.Show();
                lookingAt = thing;
                if (lookingAt != null)
                {
                    AugmentedDisplayRight.Instance.Display(
                        $"<color=white><color=green><b>eyes on</b></color>: {Describe(lookingAt)}</color>");
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
                    AugmentedDisplayRight.Instance.Display(
                        $"<color=white><color=green><b>mouse on</b></color>: {Describe(pointingAt)}</color>");
                }
                else
                {
                    AugmentedDisplayRight.Instance.Hide();
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

    public class AugmentedDisplayInWorld : MonoBehaviour
    {
        public static AugmentedDisplayInWorld Instance;

        public static void Create()
        {
            Instance = new GameObject("root").AddComponent<AugmentedDisplayInWorld>();
        }

        public static void Destroy()
        {
            if (Instance == null)
                return;
            UnityEngine.Object.Destroy(Instance.obj);
            UnityEngine.Object.Destroy(Instance.gameObject);
            Instance = null;
        }

        GameObject obj;

        void Start()
        {
            ConsoleWindow.Print($"AugmentedDisplayLeft Start {gameObject} {this}");
            obj = new GameObject("0");
            obj.transform.parent = Instance.gameObject.transform;
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var canvasTransform = canvas.transform;
            //canvasTransform.position = new Vector3(-1f, 3.05f, 2.5f);
            //canvasTransform.position = new Vector3(-100f, 100f, 0f);
            //canvasTransform.localPosition = new Vector3(0f, 0.5f, 0f);
            //canvasTransform.rotation = Quaternion.Euler(0.1f, 1f,1f);
            //canvas.pixelPerfect
            canvasTransform.localScale = Vector3.one * 0.5f; //*5f;// Vector3.one;

            var bkgd = new GameObject("1").AddComponent<RawImage>();
            bkgd.rectTransform.SetParent(canvas.transform, false);
            bkgd.rectTransform.sizeDelta = new Vector2(2f, 2f);
            bkgd.color = new Color(1f, 1f, 1f, 0.05f);

            var text = new GameObject("2").AddComponent<TextMeshProUGUI>();
            text.rectTransform.SetParent(canvas.transform, false);
            text.rectTransform.sizeDelta =bkgd.rectTransform.sizeDelta;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.fontSize = 0.15f;//0.15f;
            text.alpha = 0.5f; // low alpha is used to hide font antialiasing artifacts.
            text.color = Color.white;// new Color(1f, 1f, 1f, 0.15f);// Color.white;
            text.richText = true;
            text.text = "please <color=red><b>don't</b></color> play with me";

            // TextMeshProUGUI textMeshProUgui = gameObject.AddComponent<TextMeshProUGUI>();

            // textMeshProUgui.rectTransform.SetParent(bkgd.transform, false);
            // textMeshProUgui.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            // textMeshProUgui.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            // textMeshProUgui.rectTransform.sizeDelta = new Vector2(1f, 1f);
            // textMeshProUgui.rectTransform.transform.localPosition = Vector3.zero;
            // textMeshProUgui.rectTransform.anchoredPosition = new Vector2(0f, 0f);
            // textMeshProUgui.transform.localPosition = new Vector3(0f, 0.3f, -0.5f);

            // textMeshProUgui.richText = true;
            // textMeshProUgui.text = "please <color=red><b>don't</b></color> play with me";
            // textMeshProUgui.fontSize = 0.15f;//0.15f;
            // textMeshProUgui.color = Color.white;// new Color(1f, 1f, 1f, 0.15f);// Color.white;
            // textMeshProUgui.alignment = TextAlignmentOptions.Left;
            // textMeshProUgui.alpha = 0.5f; // low alpha is used to hide font antialiasing artifacts.
        }

        public void Show()
        {
            if (CursorManager.Instance == null || CursorManager.Instance.FoundThing == null)
                return;
            var th = Assets.Scripts.CursorManager.Instance.FoundThing;
            if (th.transform == null)
                return;
            if (obj == null)
                return;

            // All options requires different values of:
            // canvasTransform.localScale
            // textMeshProUgui.fontSize
            // textMeshProUgui.alpha 

            // when relative to InventoryManager.ParentHuman.transform
            // x - left/right, 0.2 is ok to show on the right part of the screen
            // y - up/down, depends on human height, 1 is ok.
            // z - how close, 0.3 is almost at the helmet glass.
            // gameObject.transform.SetParent(InventoryManager.ParentHuman.transform, false);
            // gameObject.transform.localPosition = new Vector3(0.2f,1,0.3f);

            // when relative to InventoryManager.ParentHuman.GlassesSlot.Occupant.transform,
            // also need to sync rotation with Camera.main.transform.rotation for better result.
            // var glassesTransform = InventoryManager.ParentHuman.GlassesSlot.Occupant.transform;
            // gameObject.transform.SetParent(glassesTransform, false);
            // gameObject.transform.localPosition = new Vector3(0.2f,0.2f,0.2f);
            // gameObject.transform.rotation = Camera.main.transform.rotation;

            // when relative to Camera.main.transform, but in this case better use 2d GUI.
            // var cameraTransform = Camera.main.transform;
            // gameObject.transform.SetParent(cameraTransform, false);
            // gameObject.transform.localPosition = new Vector3(0.2f,0.1f,0.15f);

            obj.transform.SetParent(th.transform, false);
            obj.transform.position = th.transform.position + Vector3.zero;
            //obj.transform.localPosition = new Vector3(0.2f, 0.1f, 1f);
            var tr = th.transform.rotation;
            var cr = Camera.main.transform.rotation;
            var r = cr.normalized;

            // rotate vertically to the camera:
            obj.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0); ;

            var pos = obj.transform.position;
            var posPlayer = InventoryManager.ParentHuman.transform.position;
            obj.transform.position = new Vector3(pos.x, posPlayer.y + 0.8f, pos.z);
            obj.transform.Translate(Camera.main.transform.forward * -0.5f, Space.World);

            ConsoleWindow.Print($"our position {obj.transform.position}, parent position {th.transform.position}");

            //th.transform.Translate(Vector3.forward);
            //th.transform.localScale = Vector3.one * 0.5f;
            // Vector3 look = Camera.main.transform.TransformDirection(Vector3.forward);
            // Debug.DrawRay(Camera.main.transform.position, look, Color.green, 14);
            // var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // cube.transform.position = th.transform.position;
            // Destroy(cube);
            // var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            // plane.transform.position = th.transform.position;


            // var pos = gameObject.transform.position;
            // gameObject.transform.position = new Vector3(pos.x   ,pos.y, pos.z);
            //gameObject.transform.position = new Vector3(-70f   ,-0.7f, 3);


            // var b = gameObject.GetComponentsInChildren<TextMeshProUGUI>();

            // var pos = gameObject.transform.position;
            // pos = new Vector3(pos.x, pos.y+2, pos.z);
            // var a = gameObject.GetComponentsInChildren<Canvas>();
            //ConsoleWindow.Print($"AugmentedDisplayLeft Show {pos} {a.Length} {b.Length}");
            // var pos2 = b[0].transform.position;
            // b[0].transform.position = new Vector3(pos2.x, pos2.y+0.2f, pos2.z);

            obj.SetActive(true);
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
            GUI.Label(new Rect(x, y, w, h), "aaa Loaded");

            //GUI.Label(new Rect(x, y, w, h), text);
            // GUI.Box(new Rect(x, y, w, h), text
            // ,
            // new GUIStyle()
            // {
            //     alignment = TextAnchor.UpperLeft,
            //     richText = true,
            //     fontSize = 16
            // });
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