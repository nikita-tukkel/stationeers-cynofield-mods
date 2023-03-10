using System.Collections.Generic;
using Assets.Scripts.Objects;
using UnityEngine;
using System;
using cynofield.mods.ui;

namespace cynofield.mods.utils
{
    public class NearbyObjects : MonoBehaviour
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static NearbyObjects Create(Transform parent)
        {
            var instance = Utils.CreateGameObject<NearbyObjects>(parent);
            instance.gameObject.SetActive(true);
            return instance;
        }

        private readonly Collider[] nearbyColliders = new Collider[1000];
        private readonly Dictionary<string, Thing> nearbyThings = new Dictionary<string, Thing>(1000);

        private float periodicUpdateCounter = 1.5f; // start not from 0 to have first update sooner
        void Update()
        {
            if (WorldManager.IsGamePaused)
                return;

            periodicUpdateCounter += Time.deltaTime;

            if (periodicUpdateCounter < 2f)
                return;
            periodicUpdateCounter = 0;

            //Log.Debug(() => $"Update({this.GetHashCode()}), time={Time.time}");
            int collidersCount = Physics.OverlapSphereNonAlloc(
                transform.parent.position, 20f, nearbyColliders);
            //Log.Debug(() => $"Update({this.GetHashCode()}), found {collidersCount}");

            nearbyThings.Clear();
            for (int i = 0; i < collidersCount; i++)
            {
                var c = nearbyColliders[i];
                // limit to descendands of Structure, but maybe need to look through all Thing's.
                if (c.TryGetComponent<Structure>(out var thing))
                {
                    //Log.Debug(() => $"Update {thing.DisplayName}");
                    if (thing.DisplayName.Contains(AugmentedDisplayInWorld.AR_TAG, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var id = Utils.GetId(thing);
                        //Log.Debug(() => $"Update({this.GetHashCode()}) {thing.DisplayName}, id={id}");
                        nearbyThings[id] = thing;
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
    }
}
