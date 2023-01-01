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
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

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
            Log.Info(() => $"InventoryManager={InventoryManager.Instance}");
            var inventoryHuman = InventoryManager.ParentHuman;
            Log.Info(() => $"InventoryManager.ParentHuman={inventoryHuman}");
            if (inventoryHuman != null)
            {
                Log.Info(() => $"InventoryManager.GlassesSlot={inventoryHuman.GlassesSlot}");
            }
            Log.Info(() => $"PlayerStateWindow={PlayerStateWindow.Instance}");
            Log.Info(() => $"PlayerStateWindow.Parent={PlayerStateWindow.Instance.Parent}");
            Log.Info(() => $"WorldManager={WorldManager.Instance}");
            Log.Info(() => $"Humans={Human.AllHumans.Count}");
            var human = Human.LocalHuman;
            Log.Info(() => $"Human={Human.LocalHuman}");
            if (human != null)
            {
                Log.Info(() => $"GlassesSlot={human.GlassesSlot}");
                Log.Info(() => $"Glasses={human.GlassesSlot?.Occupant}");
            }
            Log.Info(() => $"AllThings={Thing.AllThings.Count}");

            var gameManager = GameManager.Instance;
            var menuCutscene = gameManager ? gameManager.MenuCutscene : null;
            Log.Info(() => $"GameManager={gameManager}");
            Log.Info(() => $"MenuCutscene={menuCutscene}");
            if (menuCutscene != null)
            {
                Log.Info(() => $"MenuCutscene.Components={menuCutscene.GetComponentsInChildren<Component>().Length}");
                var helmets = GameManager.Instance.MenuCutscene.GetComponentsInChildren<Transform>()
                    .Where((o) => o.name.Equals("helmet", StringComparison.InvariantCultureIgnoreCase));
                Log.Info(() => $"MenuCutscene.Helmets={string.Join("\n", helmets)}");
            }
        }
    }
}
