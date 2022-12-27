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

namespace cynofield.mods.ui
{
    public class AREnabler
    {
        public bool IsLoaded()
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
            if (!IsLoaded())
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


    }
}
