using Assets.Scripts.Objects;
using cynofield.mods.ui.things;
using cynofield.mods.utils;
using Objects.Pipes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class ThingsUi
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly Fonts2d fonts2d;
        private readonly UiDefault defaultArUi;
        private readonly List<IThingDescriber> alluis = new List<IThingDescriber>();
        private readonly Dictionary<Type, IThingDescriber> uis = new Dictionary<Type, IThingDescriber>();
        public ThingsUi(Fonts2d fonts2d)
        {
            this.fonts2d = fonts2d;
            this.defaultArUi = new UiDefault(fonts2d);

            alluis.Add(new TransformerUi());
            alluis.Add(new CableUi());
            alluis.Add(new CircuitHousingUi(fonts2d));
            foreach (var ui in alluis)
            {
                uis.Add(ui.SupportedType(), ui);
            }
        }

        public bool Supports(Thing thing)
        {
            var type = thing.GetType();
            if (uis.ContainsKey(type))
                return true;

            // Some exceptions to show UiDefault for them
            return (thing is VolumePump);
        }

        private IThingDescriber GetUi(Thing thing)
        {
            var type = thing.GetType();
            if (!uis.TryGetValue(type, out IThingDescriber ui)) ui = defaultArUi;
            return ui;
        }

        public void RenderArAnnotation(Thing thing, Canvas canvas, TextMeshProUGUI textMesh)
        {
            // Log.Debug(() => $"RenderArAnnotation thing={thing}, canvas={canvas}, text={textMesh}");
            var ui = GetUi(thing);
            // Log.Debug(() => $"RenderArAnnotation ui={ui}");
            if (ui is IThingAnnotationRenderer)
            {
                // TODO more complex rendering
            }
            else if (ui is IThingDescriber)
            {
                var desc = ui.Describe(thing);
                textMesh.text = desc;
            }
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
            var name = ui.ToString(); // use thing ui class name as caching key
            objectsPool.TryGetValue(name, out GameObject gameObject);
            if (ui is IThingDetailsRenderer)
            {
                gameObject = (ui as IThingDetailsRenderer).RenderDetails(thing, parentRect, gameObject);
            }
            else if (ui is IThingDescriber)
            {
                var desc = ui.Describe(thing);
                gameObject = defaultArUi.RenderDetailsOther(desc, parentRect, gameObject);
            }

            if (gameObject == null)
                return null;

            gameObject.transform.SetParent(parent.gameObject.transform, false);
            gameObject.name = name;
            Utils.Show(gameObject);
            return gameObject;
        }
    }

    interface IThingDescriber
    {
        Type SupportedType();
        string Describe(Thing thing);
    }

    interface IThingAnnotationRenderer : IThingDescriber
    {
        GameObject RenderAnnotation(Thing thing, RectTransform parentRect, GameObject poolreuse);
    }

    interface IThingDetailsRenderer : IThingDescriber
    {
        GameObject RenderDetails(Thing thing, RectTransform parentRect, GameObject poolreuse);
    }

    interface IThingCompleteUi : IThingAnnotationRenderer, IThingDetailsRenderer { }
}
