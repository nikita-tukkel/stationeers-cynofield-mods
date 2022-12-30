using System;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.UI;
using UnityEngine;

namespace cynofield.mods.utils
{
    public class PlayerProvider
    {
        public Human GetPlayerAvatar()
        {
            return Human.LocalHuman;
        }

        public bool IsGameInitialized()
        {
            return InventoryManager.Instance && PlayerStateWindow.Instance && Human.LocalHuman;
        }

        public Transform GetMainMenuHelmet()
        {
            if (!GameManager.Instance || !GameManager.Instance.MenuCutscene)
                return null;
            var helmets = GameManager.Instance.MenuCutscene.GetComponentsInChildren<Transform>()
                .Where((o) => o.name.Equals("helmet", StringComparison.InvariantCultureIgnoreCase));
            var helmet = helmets.FirstOrDefault();
            return helmet;
        }

        public static void DebugInfo()
        {
            ConsoleWindow.Print($"InventoryManager={InventoryManager.Instance}");
            var inventoryHuman = InventoryManager.ParentHuman;
            ConsoleWindow.Print($"InventoryManager.ParentHuman={inventoryHuman}");
            if (inventoryHuman != null)
            {
                ConsoleWindow.Print($"InventoryManager.GlassesSlot={inventoryHuman.GlassesSlot}");
            }
            ConsoleWindow.Print($"PlayerStateWindow={PlayerStateWindow.Instance}");
            ConsoleWindow.Print($"PlayerStateWindow.Parent={PlayerStateWindow.Instance.Parent}");
            ConsoleWindow.Print($"WorldManager={WorldManager.Instance}");
            ConsoleWindow.Print($"Humans={Human.AllHumans.Count}");
            var human = Human.LocalHuman;
            ConsoleWindow.Print($"Human={Human.LocalHuman}");
            if (human != null)
            {
                ConsoleWindow.Print($"GlassesSlot={human.GlassesSlot}");
                ConsoleWindow.Print($"Glasses={human.GlassesSlot?.Occupant}");
            }
            ConsoleWindow.Print($"AllThings={Thing.AllThings.Count}");

            var gameManager = GameManager.Instance;
            var menuCutscene = gameManager ? gameManager.MenuCutscene : null;
            ConsoleWindow.Print($"GameManager={gameManager}");
            ConsoleWindow.Print($"MenuCutscene={menuCutscene}");
            if (menuCutscene != null)
            {
                ConsoleWindow.Print($"MenuCutscene.Components={menuCutscene.GetComponentsInChildren<Component>().Length}");
                var helmets = GameManager.Instance.MenuCutscene.GetComponentsInChildren<Transform>()
                    .Where((o) => o.name.Equals("helmet", StringComparison.InvariantCultureIgnoreCase));
                ConsoleWindow.Print($"MenuCutscene.Helmets={string.Join("\n", helmets)}");
            }
        }
    }
}
