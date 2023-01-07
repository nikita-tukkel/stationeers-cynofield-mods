using System;
using System.Collections.Concurrent;
using cynofield.mods.ui.presenter;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayLog : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayLog Create(GameObject parent, ViewLayoutFactory lf)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayLog>();
            result.Init(parent, lf);
            return result;
        }

        private GameObject parent;
        private ViewLayoutFactory lf;
        private readonly ConcurrentQueue<LogEntryView> activeLogEntries = new ConcurrentQueue<LogEntryView>();
        private readonly ConcurrentQueue<LogEntryView> pooledLogEntries = new ConcurrentQueue<LogEntryView>();
        private void Init(GameObject parent, ViewLayoutFactory lf)
        {
            this.parent = parent;
            this.lf = lf;

            var parentSize = parent.GetComponent<RectTransform>().sizeDelta;

            for (int i = 0; i < 10; i++)
            {
                var entryView = LogEntryView.Create(lf, parentSize.x);
                HideLogEntry(entryView);
            }
        }

        private float periodicUpdateCounter;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;

            if (periodicUpdateCounter <= 0.5f)
                return;
            periodicUpdateCounter = 0;

            foreach (var logEntry in activeLogEntries)
            {
                if (logEntry.age > 60)
                {
                    HideLogEntry(logEntry);
                }

                logEntry.Render();
            }
        }

        private void HideLogEntry(LogEntryView logEntry)
        {
            if (logEntry == null)
                return;
            logEntry.visibility.Hide();
            logEntry.onHide();
            Utils.DestroyChildren(logEntry.clientLayout.transform);
            pooledLogEntries.Enqueue(logEntry);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
        }

        private void ShowLogEntry(LogEntryView logEntry)
        {
            if (logEntry == null)
                return;

            var parentRect = parent.GetComponent<RectTransform>();
            logEntry.visibility.Show(parentRect);
            logEntry.onShow();
            activeLogEntries.Enqueue(logEntry);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            //Log.Debug(()=>Utils.PrintHierarchy(parent.gameObject));
        }

        public void LogToHud(string message)
        {
            LogToHud((layout) =>
            {
                var parentSize = parent.GetComponent<RectTransform>().sizeDelta;
                lf.Text1(layout, message, width: parentSize.x);
            });
        }

        public void LogToHud(LogAction logRenderAction)
        {
            LogEntryView logEntry = null;
            if (pooledLogEntries.Count <= 0)
            { // Discard oldest log entry when pool is empty
                activeLogEntries.TryDequeue(out logEntry);
                HideLogEntry(logEntry);
            }
            pooledLogEntries.TryDequeue(out logEntry);
            if (logEntry == null)
            {
                Log.Warn(() => "Unexpected out of pooled LogEntryView");
                return;
            }

            logRenderAction(logEntry.clientLayout.gameObject);
            ShowLogEntry(logEntry);
        }

        public delegate void LogAction(GameObject parent);

        public class LogEntryView : MonoBehaviour
        {
            public static LogEntryView Create(ViewLayoutFactory lf, float width)
            {
                //var result = Utils.CreateGameObject<LogEntryView>();
                var layout = lf.CreateRow(null, debug: false);
                var result = layout.gameObject.AddComponent<LogEntryView>();
                result.Init(layout, lf, width);
                return result;
            }

            private ViewLayoutFactory lf;
            private float width;
            private HorizontalLayoutGroup layout;
            public VerticalLayoutGroup clientLayout;
            public HiddenPoolComponent visibility;
            internal float creationTimestamp;
            internal float age;
            private PresenterDefault agePresenter;
            internal void Init(HorizontalLayoutGroup layout, ViewLayoutFactory lf, float width)
            {
                this.layout = layout;
                this.lf = lf;
                this.width = width;

                var ageWidth = 40;
                var logEntryPaddingLeft = 5;
                var logEntryWidth = width - ageWidth - logEntryPaddingLeft;

                {
                    layout.padding = new RectOffset(5, 5, 0, 0);
                    layout.childControlWidth = false;
                    layout.childForceExpandWidth = false;
                    layout.childControlHeight = true;
                    layout.childForceExpandHeight = true;

                    var fitter = layout.gameObject.AddComponent<ContentSizeFitter>();
                    fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                    visibility = layout.gameObject.AddComponent<HiddenPoolComponent>();
                }

                var age = lf.Text1(layout.gameObject, "", width: ageWidth);
                age.value.alignment = TextAlignmentOptions.MidlineJustified;
                age.value.margin = new Vector4(5, 0, 0, 0);
                agePresenter = age.value.GetOrAddComponent<PresenterDefault>();
                agePresenter.AddBinding((e) => age.value.text = $"{Math.Round((e as LogEntryView).age)}s");

                clientLayout = lf.RootLayout(layout.gameObject, debug: false);
                clientLayout.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(logEntryWidth, 0);
                clientLayout.spacing = 0;
                clientLayout.padding = new RectOffset(logEntryPaddingLeft, 0, 0, 0);
            }

            internal void Render()
            {
                // Update age and call client presenter if exists
                age = Time.time - creationTimestamp;
                agePresenter.Present(this);
                var presenter = clientLayout.GetComponentInChildren<PresenterDefault>();
                if (presenter != null)
                {
                    presenter.Present(this);
                }
            }

            internal void onShow()
            {
                creationTimestamp = Time.time;
            }

            internal void onHide()
            {
            }
        }
    }
}
