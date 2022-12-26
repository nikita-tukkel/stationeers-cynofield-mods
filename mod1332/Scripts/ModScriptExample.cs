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
            AugmentedDisplayInWorld.Create();
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
                    var desc = thingsUi.Description2d(lookingAt);
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
                    var desc = thingsUi.Description2d(pointingAt);
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

    public class ThingsUi
    {

        public void Render(Thing thing, Canvas canvas, TextMeshProUGUI textMesh)
        {
            // TODO different types of rendering complexity
            var desc = AugmentedReality.Instance.thingsUi.Description2d(thing);
            textMesh.text = desc;
            //             $@"{thing.DisplayName}
            // {desc}
            // please <color=red><b>don't</b></color> play with me";
        }

        public void RenderARFully(Thing thing, Canvas canvas)
        {

        }

        public void RenderARText(Thing thing, TextMeshProUGUI textMesh)
        {

        }

        public string Description2d(Thing thing)
        {
            return Describe(thing);
        }

        public void Destroy()
        {

        }

        public string Describe(Thing thing)
        {
            if (thing == null)
                return "nothing";
            switch (thing)
            {
                case Cable obj:
                    var net = obj.CableNetwork;
                    return $"network: {net.DisplayName} {PowerDisplay(net.CurrentLoad)} / {PowerDisplay(net.PotentialLoad)}";
                case Transformer obj:
                    {
                        var color = obj.Powered ? "green" : "red";
                        return
    $@"{obj.DisplayName}
<color={color}><b>{obj.Setting}</b></color>
{PowerDisplay(obj.UsedPower)}
{PowerDisplay(obj.AvailablePower)}";
                    }
                case CircuitHousing obj:
                    {
                        var chip = obj._ProgrammableChipSlot.Occupant as ProgrammableChip;
                        if (chip == null)
                        {
                            return $@"{obj.DisplayName}
<color=red><b>db={obj.Setting}</b>
no chip</color>";
                        }
                        else
                        {
                            var registers = Traverse.Create(chip)
                            .Field("_Registers").GetValue() as double[];
                            return
$@"{obj.DisplayName}
<color=green><b>db={obj.Setting}</b><mspace=1em> </mspace>r15={registers[15]}</color>
<mspace=0.65em>{DisplayRegisters(registers)}</mspace>
";
                        }
                    }
                default:
                    return thing.ToString();
            }
        }

        private string DisplayRegisters(double[] registers)
        {
            string result = "";
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                if (registers[i] == 0)
                    continue;
                count++;
                result += $"r{i}={Math.Round(registers[i], 2)}";
                if (count > 0 && count % 2 == 0)
                    result += "\n";
                else
                    result += "<mspace=1em> </mspace>";
            }
            return result;
        }

        static private string PowerDisplay(float power)
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

    public class AugmentedDisplayInWorld : MonoBehaviour
    {
        public static AugmentedDisplayInWorld Instance;

        public static void Create()
        {
            Instance = new GameObject("root").AddComponent<AugmentedDisplayInWorld>();
            //ConsoleWindow.Print($"AugmentedDisplayInWorld create {Instance} {Instance.gameObject}");
        }

        public static void Destroy()
        {
            if (Instance == null)
                return;

            foreach (var ann in Instance.annotations)
            {
                (ann as InWorldAnnotation).Destroy();
            }
            Instance.annotations.Clear();
            UnityEngine.Object.Destroy(Instance);
            UnityEngine.Object.Destroy(Instance.gameObject);
            Instance.gameObject.SetActive(false);
            Instance = null;
        }

        public class InWorldAnnotation : MonoBehaviour
        {
            GameObject obj;
            Canvas canvas;
            RawImage bkgd;
            TextMeshProUGUI text;
            Thing anchor;

            public string id;

            void Start()
            {
                //ConsoleWindow.Print($"InWorldAnnotation Start");
                obj = new GameObject("0");
                obj.SetActive(false);
                obj.transform.parent = gameObject.transform;
                canvas = obj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                var canvasTransform = canvas.transform;
                canvasTransform.localScale = Vector3.one * 0.5f; // less than 0.5 looks bad

                bkgd = new GameObject("1").AddComponent<RawImage>();
                bkgd.rectTransform.SetParent(canvas.transform, false);
                bkgd.rectTransform.sizeDelta = new Vector2(1f, 1f);

                // https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/RichText.html
                text = new GameObject("2").AddComponent<TextMeshProUGUI>();
                text.rectTransform.SetParent(canvas.transform, false);
                text.rectTransform.sizeDelta = bkgd.rectTransform.sizeDelta;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.richText = true;
                text.margin = new Vector4(0.05f, 0.05f, 0.05f, 0.05f);
                // Resources.Load<TMP_FontAsset>(string.Format("UI/{0}", this.FontName))
                // Font font = new Font("StreamingAssets/Fonts/3270-Regular.ttf");
                // var tfont = TMP_FontAsset.CreateFontAsset(font);
                // text.font = tfont;
                //ConsoleWindow.Print($"{text.font.name}");
                text.font = Localization.CurrentFont;
                //0.08f for default font used by TextMeshProUGUI without font specified;
                //0.06f when using Localization.CurrentFont
                text.fontSize = 0.06f;

                ColorScheme2(bkgd, text);
            }

            void ColorScheme1(RawImage bkgd, TextMeshProUGUI text)
            {
                bkgd.color = new Color(0.3f, 0.3f, 0.7f, 0.2f);
                text.alpha = 0.2f;
                text.color = new Color(0f, 0f, 0f, 1f);
            }

            void ColorScheme2(RawImage bkgd, TextMeshProUGUI text)
            {
                // bkgd.color = new Color(0xad / 255f, 0xd8 / 255f, 0xe6 / 255f, 0.2f);
                bkgd.color = new Color(0xad / 255f, 0xad / 255f, 0xe6 / 255f, 0.2f);
                text.alpha = 0.2f;
                text.color = new Color(1f, 1f, 1f, 0.1f); // low alpha is used to hide font antialiasing artifacts.
            }

            public void ShowNear(Thing thing, string id, RaycastHit hit)
            {
                if (thing == null)
                {
                    Hide();
                    return;
                }
                if (thing == anchor)
                {
                    return;
                }
                this.anchor = thing;
                this.id = id;

                // relink to new parent, thus appear in the parent scene.
                transform.SetParent(thing.transform, false);
                transform.SetPositionAndRotation(
                    // need to reset position on this component reuse
                    thing.transform.position + Vector3.zero,

                    // rotate vertically to the camera:
                    Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0)
                    );

                Render();

                // Fine tune coordinates only after content is rendered.
                // Expect that `bkgd` is here and covers whole annotation.
                var posHit = hit.point;
                //var posHead = InventoryManager.ParentHuman.HeadBone.transform.position;
                var posHead = InventoryManager.ParentHuman.GlassesSlot.Occupant.transform.position;
                var posLegs = InventoryManager.ParentHuman.transform.position;
                var humanHeight = (posHead.y - posLegs.y) * 1.2f;
                var limit1 = posLegs.y;
                var limit2 = limit1 + humanHeight;
                transform.position = new Vector3(posHit.x, posHit.y, posHit.z);
                Vector3[] corners = new Vector3[4];
                bkgd.rectTransform.GetWorldCorners(corners);
                var y1 = corners[0].y; // bottom left
                var y2 = corners[2].y; // top right
                var height = y2 - y1;
                var pos = transform.position;
                if (y2 > limit2)
                {
                    // ConsoleWindow.Print($"ShowNear {y2} > {posHead.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                    transform.position = new Vector3(pos.x, limit2, pos.z);
                }
                else if (y1 < limit1)
                {
                    // ConsoleWindow.Print($"ShowNear {y1} < {posLegs.y} ; pos.y={pos.y}, y1={y1}, y2={y2}; head={posHead.y}, legs={posLegs.y}");
                    transform.position = new Vector3(posHit.x, limit1 + height, posHit.z);
                }
                transform.Translate(Camera.main.transform.forward * -0.5f, Space.World);

                obj.SetActive(true);
                gameObject.SetActive(true);
            }

            public void Render()
            {
                AugmentedReality.Instance.thingsUi.Render(anchor, canvas, text);
            }

            public void Hide()
            {
                anchor = null;
                gameObject.SetActive(false);
            }

            public bool IsActive()
            {
                if (GameManager.GameState != Assets.Scripts.GridSystem.GameState.Running)
                    return false;
                if (InventoryManager.ParentHuman == null)
                    return false;

                return this.isActiveAndEnabled && anchor != null;
            }

            public void Destroy()
            {
                Destroy(gameObject);
                Destroy(obj);
            }
        }

        Queue annotations = new Queue();

        void Start()
        {
            annotations.Enqueue(new GameObject().AddComponent<InWorldAnnotation>());
            annotations.Enqueue(new GameObject().AddComponent<InWorldAnnotation>());
            annotations.Enqueue(new GameObject().AddComponent<InWorldAnnotation>());
        }

        private float sinceLastUpdate;
        void Update()
        {
            sinceLastUpdate += Time.deltaTime;

            bool isCtrlKeyDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (isCtrlKeyDown)
            {
                foreach (var obj in Instance.annotations)
                {
                    (obj as InWorldAnnotation).gameObject.SetActive(false);
                }
                return;
            }

            if (sinceLastUpdate > 0.5f)
            {
                sinceLastUpdate = 0;
                PeriodicUpdate();
            }

            if (CursorManager.Instance == null || CursorManager.CursorThing == null)
            {
                return;
            }

            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!isShiftKeyDown)
                return;

            var thing = CursorManager.CursorThing;
            var hit = CursorManager.CursorHit;
            AugmentedDisplayInWorld.Instance.Show(thing, hit);
        }

        private void PeriodicUpdate()
        {
            if (!this.isActiveAndEnabled)
                return;

            foreach (var obj in Instance.annotations)
            {
                var a = (obj as InWorldAnnotation);
                if (a == null || !a.IsActive())
                    continue;

                a.Render();
            }
        }

        public void Show(Thing thing, RaycastHit hit)
        {
            if (thing == null)
                return;

            var thingId = GetId(thing);

            foreach (var obj in Instance.annotations)
            {
                // return if there is already shown annotation for this thing
                var a = (obj as InWorldAnnotation);
                if (a.id == thingId && a.IsActive())
                    return; // TODO update description?
            }

            var ann = annotations.Dequeue() as InWorldAnnotation;
            annotations.Enqueue(ann);
            ann.ShowNear(thing, thingId, hit);
        }

        string GetId(Thing thing) { return thing.NetworkId.ToString(); }
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