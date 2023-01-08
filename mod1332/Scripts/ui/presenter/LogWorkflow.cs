using UnityEngine;
using static cynofield.mods.ui.AugmentedDisplayLog;

namespace cynofield.mods.ui.presenter
{
    /// <summary>
    /// Helper class for typical scenario of displaying one typical message in the log on HUD.
    /// Features:
    /// - knows entry point to the log HUD;
    /// - controls that message not displayed too often;
    /// - caches all objects related to the message;
    /// </summary>
    public class LogWorkflow
    {
        // TODO maybe remove DestroyChildren in AugmentedDisplayLog, and implement log entry layout reuse here.
        public float minAllowedPeriod = 60;
        private float lastSend;
        public int maxConsecutive = 1;
        private int count;
        public LogAction logRenderAction;

        public void LogToHud(object data)
        {
            if (LoggingDisabled()) return;

            AugmentedRealityEntry.Instance?.LogToHud("" + data);
        }

        public void LogToHud()
        {
            if (LoggingDisabled()) return;
            if (logRenderAction == null) return;

            AugmentedRealityEntry.Instance?.LogToHud((p) =>
            {
                var gameObject = logRenderAction(p);
                //var presenter = gameObject.GetComponentInChildren<PresenterDefault>();
                return gameObject;
            });
        }

        private bool LoggingDisabled()
        {
            var now = Time.time;
            if ((now - lastSend < minAllowedPeriod && minAllowedPeriod > 0)
                || count >= maxConsecutive && maxConsecutive > 0)
                return true;

            lastSend = now;
            count++;
            return false;
        }

        public void ResetCount() { count = 0; }
        public void Reset()
        {
            lastSend = 0;
            count = 0;
        }
    }
}
