using System.Collections.Concurrent;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Entities;
using Assets.Scripts.Objects.Pipes;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayRight : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayRight Create(VerticalLayoutGroup layoutRight, ThingsUi thingsUi,
            BaseSkin skin, Fonts2d fonts2d)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayRight>();
            result.Init(layoutRight, thingsUi, skin, fonts2d);
            return result;
        }

        private Component rootComponent;
        private VerticalLayoutGroup detailsLayout;
        private RectTransform detailsLayoutRect;
        private ThingsUi thingsUi;
        private BaseSkin skin;
        private Fonts2d fonts2d;
        private void Init(VerticalLayoutGroup root, ThingsUi thingsUi, BaseSkin skin, Fonts2d fonts2d)
        {
            this.rootComponent = root;
            this.thingsUi = thingsUi;
            this.skin = skin;
            this.fonts2d = fonts2d;

            detailsLayout = Utils.CreateGameObject<VerticalLayoutGroup>(rootComponent);
            detailsLayout.padding = new RectOffset(0, 0, 0, 0);
            detailsLayout.spacing = 0;
            detailsLayout.childAlignment = TextAnchor.UpperLeft;
            detailsLayout.childControlWidth = true;
            detailsLayout.childControlHeight = false;
            detailsLayout.childForceExpandWidth = true;
            detailsLayout.childForceExpandHeight = false;
            detailsLayout.childScaleWidth = false;
            detailsLayout.childScaleHeight = false;
            detailsLayoutRect = detailsLayout.GetOrAddComponent<RectTransform>();
            detailsLayout.GetOrAddComponent<Mask>();
            Utils.Show(detailsLayout);
        }

        public void Display(Thing thing, List<Thing> otherThings = null)
        {
            if (thing == currentThing)
                return; // do nothing if Thing not changed, Update will handle the redraw

            // Because currentThing is changed,
            //  need to hide current childs of rootComponent.
            //  Then things UI will render a new one, or reuse one of existing.
            Utils.Hide(detailsLayout);

            currentThing = thing;
            accompThings.Clear();
            if (otherThings != null)
                accompThings.AddRange(otherThings);
            var occupants = FindInternalThings(currentThing);
            if (occupants is IEnumerable<Thing>) accompThings.AddRange(occupants as IEnumerable<Thing>);
            else accompThings.Add(occupants as Thing);

            rootComponent.gameObject.SetActive(true);
            detailsLayout.gameObject.SetActive(true);
            RenderThingDetails(currentThing);
            foreach (var th in accompThings)
                RenderThingDetails(th);
        }

        public void Hide()
        {
            currentThing = null;
            accompThings.Clear();
            // We are not destroying what ThingsUi rendered,
            //  so there is a pool of objects ThingsUi will reuse.
            Utils.Hide(detailsLayout);
            rootComponent.gameObject.SetActive(false);
            detailsLayout.gameObject.SetActive(false);
        }

        private float periodicUpdateCounter;
        private Thing currentThing = null;
        private readonly List<Thing> accompThings = new List<Thing>();
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter < 0.5f)
                return;
            periodicUpdateCounter = 0;
            if (currentThing == null)
            {
                rootComponent.gameObject.SetActive(false);
                accompThings.Clear();
                return;
            }

            RenderThingDetails(currentThing);
            foreach (var th in accompThings)
                RenderThingDetails(th);
        }

        private readonly ConcurrentDictionary<string, GameObject> objectsPool = new ConcurrentDictionary<string, GameObject>();
        private void RenderThingDetails(Thing thing)
        {
            if (thing == null)
                return;
            var maybeCache = thingsUi.RenderDetailView(thing, detailsLayout, objectsPool);
            if (maybeCache != null && maybeCache.name != null)
            {
                // destroy the old one if a new one was created
                objectsPool.TryGetValue(maybeCache.name, out GameObject oldValue);
                if (oldValue != null && oldValue != maybeCache)
                {
                    Log.Warn(() => $"replacing cached {oldValue.name} with {maybeCache.name} {maybeCache}");
                    Utils.Destroy(oldValue);
                }

                // add created object into objects pool for ThingsUi to reuse.
                objectsPool[maybeCache.name] = maybeCache;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(detailsLayoutRect); // Needed to process possible changes in text heights
        }

        private object FindInternalThings(Thing parent)
        {
            //Log.Debug(() => $"FindInternalThings {parent}");
            switch (parent)
            {
                case DeviceInputOutputCircuit o: return o.ProgrammableChip;
                case ICircuitHolder o: return parent.Slots[0]?.Occupant;
            }

            return null;
        }
    }
}
