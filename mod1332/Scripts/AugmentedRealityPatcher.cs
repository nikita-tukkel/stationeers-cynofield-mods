
using Assets.Scripts.Objects;
using HarmonyLib;

namespace cynofield.mods
{
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

    [HarmonyPatch(typeof(Assets.Scripts.CursorManager),
    nameof(Assets.Scripts.CursorManager.SetCursorTarget))]
    public class AugmentationPatcherCursorManager
    {
        static void Postfix(ref Assets.Scripts.CursorManager __instance)
        {
            var view = AugmentedRealityEntry.Instance;
            view?.EyesOn(__instance.FoundThing);
        }
    }

    [HarmonyPatch(typeof(Assets.Scripts.UI.InputMouse), "Idle")] // Idle is a private method
    public class AugmentationPatcherInputMouse
    {
        static void Postfix(ref Assets.Scripts.UI.InputMouse __instance)
        {
            var view = AugmentedRealityEntry.Instance;
            Interactable interactable = Traverse.Create(typeof(Assets.Scripts.UI.InputMouse))
                .Field("WorldInteractable").GetValue() as Interactable;
            view?.MouseOn(__instance.CursorThing, interactable);
        }
    }
}
