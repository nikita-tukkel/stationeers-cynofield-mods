using Assets.Scripts;
using cynofield.mods.ee;
using cynofield.mods.utils;
using Stationeers.Addons;

namespace cynofield.mods
{
    public class AugmentedRealityPlugin : IPlugin
    {
        void IPlugin.OnLoad()
        {
            ConsoleWindow.Print($"{ToString()}: loaded!" );
            //PlayerProvider.DebugInfo();
            
            WorldManager.OnWorldStarted += OnWorldStartedHandler;
            WorldManager.OnWorldExit += OnWorldExitHandler;
            AugmentedRealityEntry.Create();

            MainMenuEasterEgg.Create();
        }

        void IPlugin.OnUnload()
        {
            MainMenuEasterEgg.Destroy();

            WorldManager.OnWorldStarted -= OnWorldStartedHandler;
            WorldManager.OnWorldExit -= OnWorldExitHandler;
            AugmentedRealityEntry.Instance?.Destroy();
        }

        void OnWorldStartedHandler()
        {
            MainMenuEasterEgg.Hide();
            AugmentedRealityEntry.Create();
        }

        void OnWorldExitHandler()
        {
            MainMenuEasterEgg.Show();
            AugmentedRealityEntry.Instance?.Destroy();
        }
    }
}
