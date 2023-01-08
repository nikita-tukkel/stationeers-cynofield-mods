using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using cynofield.mods.ui.styles;
using cynofield.mods.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static cynofield.mods.ui.AugmentedUiManager;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayInWorld : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static readonly string AR_TAG = "#AR";

        public static AugmentedDisplayInWorld Create(ThingsUi thingsUi, PlayerProvider playerProvider,
            List<ColorScheme> colorSchemes)
        {
            var result = Utils.CreateGameObject<AugmentedDisplayInWorld>();
            result.Init(thingsUi, playerProvider, colorSchemes);
            return result;
        }

        private void Init(ThingsUi thingsUi, PlayerProvider playerProvider, List<ColorScheme> colorSchemes)
        {
            this.thingsUi = thingsUi;
            this.playerProvider = playerProvider;
            this.colorSchemes = colorSchemes;
            for (int i = 0; i < 3; i++)
            {
                var ann = CreateAnnotation();
                annotations.Enqueue(ann);
            }
        }

        void OnDestroy()
        {
            IterateAll((ann) => Utils.Destroy(ann));
            annotations.Clear();
            staticAnnotations.Clear();
            staticAnnotationsPool.Clear();
        }

        void OnEnable()
        {
            IterateAll((ann) => { if (ann.anchor) Utils.Show(ann); });
        }

        void OnDisable()
        {
            IterateAll((ann) => ann.Deactivate());
        }

        private delegate void ActionDelegate(InWorldAnnotation ann);
        private void IterateAll(ActionDelegate method)
        {
            // must iterate all objects explicitly on show/hide/destroy because they attached to different parents
            foreach (var ann in annotations) { method(ann as InWorldAnnotation); }
            foreach (var ann in staticAnnotations.Values) { method(ann); }
            foreach (var ann in staticAnnotationsPool) { method(ann as InWorldAnnotation); }
        }

        private ThingsUi thingsUi;
        private PlayerProvider playerProvider;
        private List<ColorScheme> colorSchemes;
        private readonly Queue annotations = new Queue();
        private readonly Utils utils = new Utils();
        private NearbyObjects nearbyObjects;

        private readonly Dictionary<string, Thing> nearbyThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> trackedThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> removedThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, InWorldAnnotation> staticAnnotations = new Dictionary<string, InWorldAnnotation>(1000);
        private readonly Queue staticAnnotationsPool = new Queue();
        private float periodicUpdateCounter;
        private Thing currentCursorThing = null;
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            if (nearbyObjects == null)
            {
                // late init in case Human object was not available on early calls to Update.
                var human = playerProvider.GetPlayerAvatar();
                if (human)
                    nearbyObjects = NearbyObjects.Create(human.transform);
            }

            periodicUpdateCounter += Time.deltaTime;

            if (periodicUpdateCounter > 0.5f)
                PeriodicUpdate();

            if (InputWindow.InputState != InputPanelState.None)
                return;

            bool isCtrlKeyDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (isCtrlKeyDown)
            {
                foreach (var obj in annotations)
                {
                    (obj as InWorldAnnotation).Deactivate();
                }
                return;
            }

            if (CursorManager.Instance == null || CursorManager.CursorThing == null)
                return;

            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!isShiftKeyDown)
            {
                currentCursorThing = null;
                return;
            }

            if (currentCursorThing == null)
            {
                currentCursorThing = CursorEventInfo.FromCursorManager(CursorManager.Instance.FoundThing).Subject;
            }

            //Log.Debug(() => $"currentCursorThing{currentCursorThing}");

            if (!thingsUi.Supports(currentCursorThing))
                return;

            Show(currentCursorThing);
        }

        private void PeriodicUpdate()
        {
            periodicUpdateCounter = 0;
            if (!this.isActiveAndEnabled)
                return;

            foreach (var obj in annotations)
            {
                var a = (obj as InWorldAnnotation);
                if (a == null || !a.IsActive())
                    continue;

                a.Render();
            }

            UpdateTrackedObjects();
        }

        private void UpdateTrackedObjects()
        {
            if (nearbyObjects == null)
                return;
            nearbyObjects.GetAll(nearbyThings);
            foreach (var entry in nearbyThings)
            {
                var id = entry.Key;
                var th = entry.Value;
                if (trackedThings.TryAdd(id, th))
                {
                    OnTrackedAdded(id, th);
                }
            }

            removedThings.Clear();
            foreach (var entry in trackedThings)
            {
                var id = entry.Key;
                var th = entry.Value;
                if (!nearbyThings.ContainsKey(id))
                {
                    removedThings[id] = th;
                }
            }
            foreach (var entry in removedThings)
            {
                var id = entry.Key;
                var th = entry.Value;
                trackedThings.Remove(id);
                OnTrackedRemoved(id, th);
            }

            foreach (var entry in trackedThings)
            {
                var id = entry.Key;
                var th = entry.Value;
                if (!staticAnnotations.TryGetValue(id, out InWorldAnnotation ann))
                    continue;
                var colorSchemeId = ParseColorSchemeId(th.DisplayName);
                ann.ColorSchemeId = colorSchemeId;
                ann.ShowOver(th, id);
            }
        }

        private void OnTrackedAdded(string thingId, Thing thing)
        {
            var colorSchemeId = ParseColorSchemeId(thing.DisplayName);
            //Log.Info(() => $"New tracked {thing.DisplayName}, colorSchemeId={colorSchemeId}");
            var ann = CreateStaticAnnotation(colorSchemeId);
            staticAnnotations.Add(thingId, ann);
        }

        // TODO replace with TagParser
        private int ParseColorSchemeId(string str)
        {
            string[] tokens = str.Split(' ');
            string arToken = null;
            foreach (var token in tokens)
            {
                if (token.StartsWith(AR_TAG, StringComparison.InvariantCultureIgnoreCase))
                {
                    arToken = token;
                    break;
                }
            }
            if (arToken == null)
                return -1;
            var tokenParams = arToken.Substring(AR_TAG.Length);
            if (!int.TryParse(tokenParams, out int result))
                return -1;

            return result;
        }

        private void OnTrackedRemoved(string thingId, Thing thing)
        {
            //Log.Info(() => $"Removed tracked {thing.DisplayName}");
            if (!staticAnnotations.TryGetValue(thingId, out InWorldAnnotation ann))
                return;

            // hide and move into pool
            ann.gameObject.SetActive(false);
            staticAnnotations.Remove(thingId);
            staticAnnotationsPool.Enqueue(ann);
        }

        private InWorldAnnotation CreateStaticAnnotation(int colorSchemeId = -1)
        {
            InWorldAnnotation ann;
            if (staticAnnotationsPool.Count > 0)
            {
                ann = staticAnnotationsPool.Dequeue() as InWorldAnnotation;
                ann.ColorSchemeId = colorSchemeId;
            }
            else
            {
                ann = CreateAnnotation(colorSchemeId);
            }
            return ann;
        }

        private InWorldAnnotation CreateAnnotation(int colorSchemeId = -1)
        {
            return InWorldAnnotation.Create(null, thingsUi, playerProvider, colorSchemes, colorSchemeId);
        }

        public void Show(Thing thing)
        {
            //Log.Debug(() => $"Show {thing}");
            if (thing == null)
                return;

            var thingId = Utils.GetId(thing);

            InWorldAnnotation existing = null;
            foreach (var obj in annotations)
            {
                // find annotation already shown for this thing
                var a = obj as InWorldAnnotation;
                if (a.id == thingId && a.IsActive())
                {
                    existing = a;
                    break;
                }
            }

            InWorldAnnotation ann;
            if (existing == null)
            {
                ann = annotations.Dequeue() as InWorldAnnotation;
                annotations.Enqueue(ann);
            }
            else
            {
                ann = existing;
            }

            ann.ShowNear(thing, thingId);
        }
    }
}
