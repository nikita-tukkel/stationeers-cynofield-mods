using System.Collections.Concurrent;
using Assets.Scripts.Objects;
using cynofield.mods.utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayRight : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static AugmentedDisplayRight Create(VerticalLayoutGroup layoutRight, ThingsUi thingsUi, Fonts2d fonts2d)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayRight>();
            result.Init(layoutRight, thingsUi, fonts2d);
            return result;
        }

        private Component rootComponent;
        private RectTransform rootRect;
        private ThingsUi thingsUi;
        private Fonts2d fonts2d;
        TextMeshProUGUI text1;
        TextMeshProUGUI text2;
        private void Init(VerticalLayoutGroup root, ThingsUi thingsUi, Fonts2d fonts2d)
        {
            this.rootComponent = root;
            this.rootRect = rootComponent.gameObject.GetComponent<RectTransform>();
            this.thingsUi = thingsUi;
            this.fonts2d = fonts2d;
        }

        public void Display(Thing thing)
        {
            if (thing == currentThing)
                return; // do nothing if Thing not changed, Update will handle the redraw

            // Because currentThing is changed,
            //  need to hide current childs of rootComponent.
            //  Then things UI will render a new one, or reuse one of existing.
            HideWholePool();

            currentThing = thing;
            RenderThingDetails();
            rootComponent.gameObject.SetActive(true);
        }

        public void Hide()
        {
            currentThing = null;
            HideWholePool();
            rootComponent.gameObject.SetActive(false);
        }

        private void HideWholePool()
        {
            // We are not destroying what ThingsUi rendered,
            //  so there is a pool of objects ThingsUi will reuse.
            var children = rootComponent.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                Utils.Hide(child);
            }
        }

        private float periodicUpdateCounter;
        private Thing currentThing = null;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;
            if (periodicUpdateCounter < 0.5f)
                return;
            if (currentThing == null)
            {
                rootComponent.gameObject.SetActive(false);
                return;
            }

            RenderThingDetails();
        }

        private readonly ConcurrentDictionary<string, GameObject> objectsPool = new ConcurrentDictionary<string, GameObject>();
        private void RenderThingDetails()
        {
            if (currentThing == null)
                return;
            //var desc = thingsUi.Describe(currentThing);
            var maybeCache = thingsUi.RenderDetailView(currentThing, rootComponent, objectsPool);
            if (maybeCache != null && maybeCache.name != null)
            {
                // add created object into objects pool for ThingsUi to reuse.

                // TODO destroy the old one if a new one was created
                objectsPool[maybeCache.name] = maybeCache;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect); // Needed to process possible changes in text heights
        }

    }
}
