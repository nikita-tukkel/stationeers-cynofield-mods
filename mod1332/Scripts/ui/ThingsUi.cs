using Assets.Scripts.Objects;
using cynofield.mods.ui.presenter;
using cynofield.mods.ui.styles;
using cynofield.mods.ui.things;
using cynofield.mods.utils;
using Objects.Pipes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class ThingsUi
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private void InitThings(BaseSkin skin, ViewLayoutFactory lf, ViewLayoutFactory3d lf3d)
        {
            alluis.Add(new TransformerUi(skin));
            alluis.Add(new CableUi(skin));
            alluis.Add(new ProgrammableChipUi(lf, lf3d, skin));
            alluis.Add(new WeatherStationUi(lf));
            alluis.Add(new BatteryUi(lf, skin));

            foreach (var ui in alluis)
            {
                var thingType = ui.SupportedType();
                uis[thingType] = ui;
                if (ui is IThingWatcher watcher)
                    defaultWatchers[thingType] = watcher;
            }

            foreach (var ui in alluis)
            {
                var watchers = (ui as IThingWatcherProvider)?.GetWatchers();
                if (watchers != null)
                {
                    foreach (var entry in watchers)
                    {
                        var watcherId = entry.Key;
                        var watcher = entry.Value;
                        var thingType = watcher.SupportedType();
                        specificWatchers[(thingType, watcherId)] = watcher;
                        if (!defaultWatchers.ContainsKey(thingType))
                            defaultWatchers[thingType] = watcher;
                    }
                }
            }
        }

        private readonly BaseSkin skin;
        private readonly Fonts2d fonts2d;
        private readonly UiDefault defaultArUi;
        private TagParser tagParser = new TagParser();
        private readonly List<IThingDescriber> alluis = new List<IThingDescriber>();
        private readonly Dictionary<Type, IThingDescriber> uis = new Dictionary<Type, IThingDescriber>();
        private readonly Dictionary<Type, IThingWatcher> defaultWatchers = new Dictionary<Type, IThingWatcher>();
        private readonly Dictionary<(Type, string), IThingWatcher> specificWatchers = new Dictionary<(Type, string), IThingWatcher>();
        public ThingsUi(BaseSkin skin, ViewLayoutFactory lf, ViewLayoutFactory3d lf3d, Fonts2d fonts2d)
        {
            this.skin = skin;
            this.fonts2d = fonts2d;
            this.defaultArUi = new UiDefault(lf, lf3d);
            InitThings(skin, lf, lf3d);
        }

        public bool Supports(Thing thing)
        {
            var type = thing.GetType();
            if (uis.ContainsKey(type))
                return true;

            // Some exceptions to show UiDefault for them
            return (thing is VolumePump);
        }

        internal IThingDescriber GetUi(Thing thing)
        {
            // TODO rework from ui.SupportedType() into ui.IsSupported(thing)
            var type = thing.GetType();
            if (!uis.TryGetValue(type, out IThingDescriber ui)) ui = defaultArUi;
            return ui;
        }

        public void RenderArAnnotation(Thing thing, Component parent)
        {
            if (thing == null || parent == null || parent.gameObject == null)
                return;
            if (!parent.gameObject.TryGetComponent<RectTransform>(out var parentRect))
                return;

            // Log.Debug(() => $"RenderArAnnotation thing={thing}, parent={parent}");
            var ui = GetUi(thing);
            // Log.Debug(() => $"RenderArAnnotation ui={ui}");
            GameObject gameObject = null;
            if (ui is IThingAnnotationRenderer)
            {
                gameObject = (ui as IThingAnnotationRenderer).RenderAnnotation(thing, parentRect);
            }
            else if (ui is IThingDescriber)
            {
                var desc = ui.Describe(thing);
                gameObject = defaultArUi.RenderAnnotation(thing, parentRect, desc);
            }

            if (gameObject == null)
                return;

            Utils.Show(gameObject);
        }

        public GameObject RenderDetailView(Thing thing, Component parent,
            ConcurrentDictionary<string, GameObject> objectsPool)
        {
            if (thing == null || parent == null || parent.gameObject == null)
                return null;
            if (!parent.gameObject.TryGetComponent<RectTransform>(out var parentRect))
                return null;

            var ui = GetUi(thing);
            //Log.Debug(() => $"{thing} to {ui}");
            //Log.Debug(() => $"parentRect.sizeDelta {parentRect.sizeDelta}");
            var name = ui.ToString(); // use thing ui class name as caching key
            objectsPool.TryGetValue(name, out GameObject gameObject);
            if (ui is IThingDetailsRenderer)
            {
                gameObject = (ui as IThingDetailsRenderer).RenderDetails(thing, parentRect, gameObject);
            }
            else if (ui is IThingDescriber)
            {
                var desc = ui.Describe(thing);
                gameObject = defaultArUi.RenderDetails(thing, parentRect, gameObject, desc);
            }

            if (gameObject == null)
                return null;

            gameObject.transform.SetParent(parent.gameObject.transform, false);
            gameObject.name = name;
            Utils.Show(gameObject);
            return gameObject;
        }

        public GameObject RenderWatch(Thing thing, GameObject parent, TagParser.Tag tag)
        {
            if (thing == null || parent == null || parent.gameObject == null)
                return null;
            if (!parent.TryGetComponent<RectTransform>(out var parentRect))
                return null;

            string watcherId = null;
            if (tag?.paramsString != null && tag.paramsString.Length > 0)
                watcherId = tag.paramsString[0];

            var thingType = thing.GetType();
            if (!specificWatchers.TryGetValue((thingType, watcherId), out IThingWatcher watcher))
            {
                defaultWatchers.TryGetValue(thingType, out watcher);
            }

            GameObject gameObject;
            if (watcher == null)
            {
                var ui = GetUi(thing);
                gameObject = defaultArUi.RenderWatch(thing, parentRect, tag, ui.Describe(thing));
            }
            else
            {
                gameObject = watcher.RenderWatch(thing, parentRect, tag);
            }
            return gameObject;
        }
    }

    interface IThingDescriber
    {
        /// <summary>
        /// What kind of `Thing`s are supported by this class. E.g., `CircuitHousing`.
        /// </summary>
        Type SupportedType();

        /// <summary>
        /// This is a cheap alternative for creating a complex UI layouts with Render* methods, 
        /// defined in the interfaces descendant to IThingDescriber. Just output a rich text you need.
        /// 
        /// This is a fallback method and it is not used when corresponding Render* method is available.
        /// </summary>
        string Describe(Thing thing);
    }

    interface IThingAnnotationRenderer : IThingDescriber
    {
        GameObject RenderAnnotation(Thing thing, RectTransform parentRect);
    }

    interface IThingDetailsRenderer : IThingDescriber
    {
        GameObject RenderDetails(Thing thing, RectTransform parentRect, GameObject poolreuse);
    }

    interface IThingCompleteUi : IThingAnnotationRenderer, IThingDetailsRenderer { }

    interface IThingWatcher
    {
        Type SupportedType();
        GameObject RenderWatch(Thing thing, RectTransform parentRect, TagParser.Tag watcherTag);
    }

    interface IThingWatcherProvider
    {
        Dictionary<string, IThingWatcher> GetWatchers();
    }
}
