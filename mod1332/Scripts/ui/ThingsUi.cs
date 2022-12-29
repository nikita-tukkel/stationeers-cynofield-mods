using Assets.Scripts.Objects;
using cynofield.mods.ui.things;
using Objects.Pipes;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class ThingsUi
    {
        public ThingsUi()
        {
            alluis.Add(new TransformerUi());
            alluis.Add(new CableUi());
            alluis.Add(new CircuitHousingUi());
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

        private IThingArDescriber GetUi(Thing thing)
        {
            var type = thing.GetType();
            if (!uis.TryGetValue(type, out IThingArDescriber ui)) ui = defaultArUi;
            return ui;
        }

        public void RenderArAnnotation(Thing thing, Canvas canvas, TextMeshProUGUI textMesh)
        {
            //Debug.Log($"ThingsUi.RenderArAnnotation thing={thing}, canvas={canvas}, text={textMesh}");
            var ui = GetUi(thing);
            //Debug.Log($"ThingsUi.RenderArAnnotation ui={ui}");
            if (ui is IThingArRenderer)
            {
                // TODO more complex rendering
            }
            else if (ui is IThingArDescriber)
            {
                var desc = ui.Describe(thing);
                textMesh.text = desc;
            }
        }

        public string Describe(Thing thing)
        {
            return GetUi(thing).Describe(thing);
        }

        private readonly IThingArDescriber defaultArUi = new UiDefault();
        private readonly List<IThingArDescriber> alluis = new List<IThingArDescriber>();
        private readonly Dictionary<Type, IThingArDescriber> uis = new Dictionary<Type, IThingArDescriber>();
    }

    interface IThingArDescriber
    {
        Type SupportedType();
        string Describe(Thing thing);
    }

    interface IThingArRenderer : IThingArDescriber
    {
        void Render(Transform parent);
    }
}
