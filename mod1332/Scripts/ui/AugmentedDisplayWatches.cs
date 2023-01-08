using System;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using cynofield.mods.ui.presenter;
using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayWatches : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static readonly string WATCH_TAG = "#W";

        public static AugmentedDisplayWatches Create(VerticalLayoutGroup parent, ThingsUi thingsUi,
            ViewLayoutFactory lf, Fonts2d fonts2d)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayWatches>();
            result.Init(parent, thingsUi, lf);
            return result;
        }

        private VerticalLayoutGroup parent;
        private ThingsUi thingsUi;
        private ViewLayoutFactory lf;
        private TagParser tagParser = new TagParser();
        private void Init(VerticalLayoutGroup parent, ThingsUi thingsUi, ViewLayoutFactory lf)
        {
            this.parent = parent;
            this.thingsUi = thingsUi;
            this.lf = lf;
        }

        // Key: Thing ID - Watcher Tag
        private readonly Dictionary<WatcherKey, Thing> activeWatchers = new Dictionary<WatcherKey, Thing>(1000);
        private readonly Dictionary<WatcherKey, GameObject> activeViews = new Dictionary<WatcherKey, GameObject>(1000);
        private readonly HashSet<WatcherKey> foundWatchers = new HashSet<WatcherKey>(1000);
        private readonly HashSet<WatcherKey> removedWatchers = new HashSet<WatcherKey>(1000);

        private float periodicUpdateCounter;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter <= 0.5f)
                return;
            periodicUpdateCounter = 0;

            UpdateTrackedObjects();
        }

        private int allThingsRescanCounter = 1000;
        private void UpdateTrackedObjects()
        {
            // Visual update preformed once 0.5s, but AllThings scan period is 2s
            allThingsRescanCounter++;
            if (allThingsRescanCounter >= 3)
            {
                allThingsRescanCounter = 0;
                ScanForWatches();
            }

            foreach (var entry in activeViews)
            {
                var watcherKey = entry.Key;
                var layout = entry.Value;
                var thing = activeWatchers[watcherKey];
                thingsUi.RenderWatch(thing, layout, watcherKey.tag);
                Utils.Show(layout);
            }
        }

        private void ScanForWatches()
        {
            // TODO maybe look into making AllThings scanning a coroutine
            // TODO also make a hook to Rename action, and for #AR tags as well
            foundWatchers.Clear();
            removedWatchers.Clear();
            foreach (var th in Thing.AllThings)
            {
                var id = Utils.GetId(th);
                if (th.DisplayName.Contains(WATCH_TAG, StringComparison.InvariantCultureIgnoreCase))
                {
                    var tags = tagParser.Parse(th.DisplayName);
                    if (tags == null)
                    {
                        // Log.Debug(() => $"No tags for {th.DisplayName}");
                    }
                    else
                    {
                        //Log.Debug(() => $"Tags for {th.DisplayName}: {string.Join("; ", tags)}");
                        foreach (var tag in tags)
                        {
                            if (!tag.name.StartsWith(WATCH_TAG, StringComparison.InvariantCultureIgnoreCase))
                                continue; // filter out other non-`#w` tags

                            var watcherKey = new WatcherKey(id, tag);
                            foundWatchers.Add(watcherKey);
                            if (activeWatchers.TryAdd(watcherKey, th))
                            {
                                OnWatcherAdded(watcherKey, th);
                            }
                        }
                    }
                }
            }

            foreach (var entry in activeWatchers)
            {
                var watcherKey = entry.Key;
                if (!foundWatchers.Contains(watcherKey))
                {
                    removedWatchers.Add(watcherKey);
                }
            }

            foreach (var watcherKey in removedWatchers)
            {
                OnTrackedRemoved(watcherKey);
                activeViews.Remove(watcherKey);
                activeWatchers.Remove(watcherKey);
            }
        }

        private void OnWatcherAdded(WatcherKey watcherKey, Thing thing)
        {
            //Log.Info(() => $"New tracked {watcherKey} {thing.DisplayName}");
            var layout = lf.RootLayout(parent.gameObject, debug: false);
            layout.spacing = -3;
            layout.padding = new RectOffset(5, 5, 0, 0);

            activeViews[watcherKey] = layout.gameObject;
        }

        private void OnTrackedRemoved(WatcherKey watcherKey)
        {
            //Log.Info(() => $"Removed tracked {watcherKey}");
            if (activeViews.TryGetValue(watcherKey, out GameObject layout))
            {
                Utils.Destroy(layout);
            }
        }

        internal struct WatcherKey
        {
            public string thingId;
            public TagParser.Tag tag;

            public WatcherKey(string thingId, TagParser.Tag tag)
            {
                this.thingId = thingId;
                this.tag = tag;
            }

            override public bool Equals(object obj)
            {
                return obj is WatcherKey key &&
                       thingId == key.thingId &&
                       EqualityComparer<TagParser.Tag>.Default.Equals(tag, key.tag);
            }

            override public int GetHashCode()
            {
                int hash = 17;
                if (thingId != null)
                    hash = hash * 23 + thingId.GetHashCode();
                if (tag != null)
                    hash = hash * 23 + tag.GetHashCode();
                return hash;
            }

            override public string ToString()
            {
                return $"{thingId};{tag}";
            }
        }
    }
}
