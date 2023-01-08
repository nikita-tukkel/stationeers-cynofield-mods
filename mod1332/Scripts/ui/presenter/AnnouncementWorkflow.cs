using cynofield.mods.utils;
using UnityEngine;

namespace cynofield.mods.ui.presenter
{
    /// <summary>
    /// Helper class for typical scenario of displaying announcement on the HUD banner.
    /// Features:
    /// - knows entry point to the HUD banner and log;
    /// - controls that message not displayed too often;
    /// - caches all objects related to the message;
    /// </summary>
    /// 
    /// Workflow steps:
    /// - animate message;
    /// - wait;
    /// - fade out;
    /// - hide;
    /// - display in log panel;
    public class AnnouncementWorkflow
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public float minAllowedPeriod = 60;
        private float lastSend;
        public int maxConsecutive = 1;
        private int count;

        public void ShowAnnouncement(object data)
        {
            var banner = AugmentedRealityEntry.Instance?.GetBanner();
            if (banner == null)
                return;

            if (AnnouncementDisabled()) return;

            AnnouncementWorkflowWorker worker = banner.GetComponentInChildren<AnnouncementWorkflowWorker>();
            if (worker == null)
            {
                worker = Utils.CreateGameObject<AnnouncementWorkflowWorker>(banner);
            }
            worker.StartAnnouncement(data);
        }

        private bool AnnouncementDisabled()
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

    public class AnnouncementWorkflowWorker : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private AugmentedDisplayBanner banner;
        private string message;
        private TextAnimator textAnimator;
        private float waitStartedTimestamp;
        private float fadeStartedTimestamp;
        public void StartAnnouncement(object message)
        {
            this.banner = gameObject.transform.parent.GetComponent<AugmentedDisplayBanner>();
            if (message == null || banner == null)
                return;

            if (state != State.IDLE)
            {
                // Quickly finish previous announcement
                state = State.IDLE;
                LogToHud();
                this.message = null;
            }

            this.message = "" + message;
            textAnimator = new TextAnimator(this.message);
            gameObject.SetActive(true);
            state = State.ANIMATE_TEXT;
            UpdateInternal();
        }

        private float periodicUpdateCounter;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter <= 0.1f)
                return;
            periodicUpdateCounter = 0;

            UpdateInternal();
        }

        private enum State
        {
            IDLE,
            ANIMATE_TEXT,
            WAIT,
            FADING_OUT,
        }

        private State state;
        private void UpdateInternal()
        {
            var oldState = state;
            switch (state)
            {
                case State.ANIMATE_TEXT:
                    if (!textAnimator.MoveNext())
                        state = State.WAIT;

                    banner.message = textAnimator.Current;
                    break;

                case State.WAIT:
                    if (Time.time - waitStartedTimestamp > 2)
                        state = State.FADING_OUT;
                    break;

                case State.FADING_OUT:
                    if (Time.time - fadeStartedTimestamp > 5)
                    {
                        LogToHud();
                        state = State.IDLE;
                    }
                    break;
            }

            if (state == oldState) return;
            //Log.Debug(() => $"{oldState} -> {state}");

            switch (state)
            {
                case State.IDLE:
                    gameObject.SetActive(false);
                    break;

                case State.WAIT:
                    waitStartedTimestamp = Time.time;
                    break;

                case State.FADING_OUT:
                    fadeStartedTimestamp = Time.time;
                    banner.Fade();
                    break;
            }
        }

        private void LogToHud()
        {
            if (string.IsNullOrEmpty(message))
                return;

            AugmentedRealityEntry.Instance?.LogToHud(message);
        }
    }
}
