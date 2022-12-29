using System.Net.Http.Headers;
using System.Collections.Generic;
using Assets.Scripts.Objects;
using UnityEngine;
using System;

namespace cynofield.mods.utils
{
    public class NearbyObjects : MonoBehaviour
    {
        public static NearbyObjects Create(Transform parent)
        {
            var instance = new GameObject("NearestObjects").AddComponent<NearbyObjects>();
            instance.transform.SetParent(parent, false);
            return instance;
        }

        private readonly Utils utils = new Utils();
        private readonly Collider[] nearbyColliders = new Collider[1000];
        private readonly Dictionary<string, Thing> nearbyThings = new Dictionary<string, Thing>(1000);

        private float periodicUpdateCounter = 1; // start from 1 to have first update sooner
        void Update()
        {
            periodicUpdateCounter += Time.deltaTime;

            if (periodicUpdateCounter < 2f)
                return;

            periodicUpdateCounter = 0;

            int collidersCount = Physics.OverlapSphereNonAlloc(
                transform.parent.position, 5f, nearbyColliders);
            nearbyThings.Clear();
            for (int i = 0; i < collidersCount; i++)
            {
                var c = nearbyColliders[i];
                // limit to descendands of Structure, but maybe need to look through all Thing's.
                if (c.TryGetComponent<Structure>(out var thing))
                {
                    if (thing.isActiveAndEnabled && thing.DisplayName.Contains("#AR",
                    StringComparison.InvariantCultureIgnoreCase))
                    {
                        nearbyThings[utils.GetId(thing)] = thing;
                    }
                }
            }
        }

        public int GetAll(Thing[] array)
        {
            var i = 0;
            foreach (var th in nearbyThings.Values)
            {
                if (i >= array.Length)
                    return i;
                array[i++] = th;
            }

            return i;
        }

        public void GetAll(List<Thing> array)
        {
            array.Clear();
            array.AddRange(nearbyThings.Values);
        }

        public void GetAll(Dictionary<string, Thing> map)
        {
            map.Clear();
            foreach (var entry in nearbyThings)
            {
                map[entry.Key] = entry.Value;
            }
        }

        void OnDestroy()
        {
        }
    }
}
