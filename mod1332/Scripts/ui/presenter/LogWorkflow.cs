
namespace cynofield.mods.ui.presenter
{
    /// <summary>
    /// Helper class for typical scenario of displaying log on HUD.
    /// 
    /// </summary>
    public class LogWorkflow
    {
        public void LogToHud()
        {
            AugmentedRealityEntry.Instance?.LogToHud("aaa");
        }
    }
}
