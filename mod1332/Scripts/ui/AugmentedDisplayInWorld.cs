using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Objects;
using cynofield.mods.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cynofield.mods.ui
{
    public class AugmentedDisplayInWorld : MonoBehaviour
    {
        public static AugmentedDisplayInWorld Create(ThingsUi thingsUi)
        {
            var instance = new GameObject("root").AddComponent<AugmentedDisplayInWorld>();
            instance.thingsUi = thingsUi;
            //ConsoleWindow.Print($"AugmentedDisplayInWorld create {Instance} {Instance.gameObject}");
            return instance;
        }

        void OnDestroy()
        {
            // destroy them explicitly because they attach to different parents
            foreach (var ann in annotations)
            {
                (ann as InWorldAnnotation).Destroy();
            }
            annotations.Clear();

            foreach (var ann in staticAnnotations.Values)
                ann.Destroy();
            staticAnnotations.Clear();
            foreach (var ann in staticAnnotationsPool)
                (ann as InWorldAnnotation).Destroy();
            staticAnnotationsPool.Clear();
        }

        private ThingsUi thingsUi;
        private readonly Queue annotations = new Queue();
        private readonly Utils utils = new Utils();
        private NearbyObjects nearbyObjects;

        void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                var ann = CreateAnnotation();
                annotations.Enqueue(ann);
            }
        }

        private readonly Dictionary<string, Thing> nearbyThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> trackedThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, Thing> removedThings = new Dictionary<string, Thing>(1000);
        private readonly Dictionary<string, InWorldAnnotation> staticAnnotations = new Dictionary<string, InWorldAnnotation>(1000);
        private readonly Queue staticAnnotationsPool = new Queue();
        private float periodicUpdateCounter;
        void Update()
        {
            if (nearbyObjects == null)
            {
                var human = InventoryManager.ParentHuman?.transform;
                if (human != null)
                {
                    nearbyObjects = NearbyObjects.Create(InventoryManager.ParentHuman.transform);
                }
            }

            periodicUpdateCounter += Time.deltaTime;

            bool isCtrlKeyDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (isCtrlKeyDown)
            {
                foreach (var obj in annotations)
                {
                    (obj as InWorldAnnotation).gameObject.SetActive(false);
                }
                return;
            }

            if (periodicUpdateCounter > 0.5f)
                PeriodicUpdate();

            if (CursorManager.Instance == null || CursorManager.CursorThing == null)
                return;

            bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!isShiftKeyDown)
                return;

            var thing = CursorManager.CursorThing;
            var hit = CursorManager.CursorHit;

            if (!thingsUi.Supports(thing))
                return;

            Show(thing, hit);
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
            Debug.Log($"New tracked {thing.DisplayName}");
            var colorSchemeId = ParseColorSchemeId(thing.DisplayName);
            var ann = CreateStaticAnnotation(colorSchemeId);
            staticAnnotations.Add(thingId, ann);
        }

        private int ParseColorSchemeId(string str)
        {
            string[] tokens = str.Split(' ');
            string arToken = null;
            foreach (var token in tokens)
            {
                if (token.StartsWith("#AR", StringComparison.InvariantCultureIgnoreCase))
                {
                    arToken = token;
                    break;
                }
            }
            if (arToken == null)
                return -1;
            var tokenParams = arToken.Substring(3);
            if (!int.TryParse(tokenParams, out int result))
                return -1;

            return result;
        }

        private void OnTrackedRemoved(string thingId, Thing thing)
        {
            Debug.Log($"Removed tracked {thing.DisplayName}");
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
            return InWorldAnnotation.Create(null, thingsUi, colorSchemeId);
        }

        public void Show(Thing thing, RaycastHit hit)
        {
            if (thing == null)
                return;

            var thingId = utils.GetId(thing);

            InWorldAnnotation existing = null;
            foreach (var obj in annotations)
            {
                // find annotation already shown for this thing
                var a = (obj as InWorldAnnotation);
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

            ann.ShowNear(thing, thingId, hit);
        }
    }
}
