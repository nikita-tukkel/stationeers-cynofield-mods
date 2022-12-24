using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Pipes;
using HarmonyLib;
using Stationeers.Addons;

namespace cynofield.mods
{
    public class PluginExample : IPlugin
    {
        public static PluginExample Instance;

        void IPlugin.OnLoad()
        {
            Instance = this;
            ConsoleWindow.Print(ToString() + ": loaded!");
            // UnityEngine.Debug.Log(""); // see readme.md for log file location
        }

        void IPlugin.OnUnload()
        {
            Instance = null;
        }

        public string Prefix { get { return ToString(); } }
    }

    [HarmonyPatch(typeof(FiltrationMachine), nameof(FiltrationMachine.GetPassiveTooltip))]
    public class PatcherExample
    {
        static void Postfix(ref PassiveTooltip __result)
        {
            __result.Title = $"{PluginExample.Instance.Prefix} {__result.Title}";
        }
    }
}
