
using Assets.Scripts.Objects;
using UnityEngine;

namespace cynofield.mods.utils
{
    public class Utils
    {
        public static T CreateGameObject<T>(GameObject parent) where T : Component { return CreateGameObject<T>(parent.transform); }
        public static T CreateGameObject<T>(Component parent) where T : Component { return CreateGameObject<T>(parent.transform); }
        public static T CreateGameObject<T>(Transform parent) where T : Component
        {
            var result = new GameObject().AddComponent<T>();
            result.gameObject.SetActive(false);
            if (parent != null)
                result.transform.SetParent(parent, false);
            return result;
        }

        public static void Destroy(Component obj)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }
        public static void Destroy(GameObject obj)
        {
            if (obj != null)
                UnityEngine.Object.Destroy(obj);
        }

#pragma warning disable CS0618
        public static void Show(Component obj) { Show(obj.gameObject); }
        public static void Show(GameObject obj)
        {
            // SetActive doesn't make initially inactive objects active,
            //  so have to use deprecated method, or recurse children manually.
            obj.SetActiveRecursively(true);
        }

        public static void Hide(Component obj) { Hide(obj.gameObject); }
        public static void Hide(GameObject obj)
        {
            obj.SetActiveRecursively(false);
        }


        public string GetId(Thing thing) { return thing == null ? "" : thing.ReferenceId.ToString(); }
    }
}
