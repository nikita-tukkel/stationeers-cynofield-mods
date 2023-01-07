using cynofield.mods.utils;
using UnityEngine;

namespace cynofield.mods.ui
{
    /// <summary>
    /// Detached hidden objects for "reparent to hide" visibility control.
    /// This approach allows to use recursive show/hide on the original hierarchy of the hidden object.
    /// </summary>
    public class HiddenPool : MonoBehaviour
    {
        internal static HiddenPool Instance;
        public static void Create()
        {
            if (Instance == null)
                Destroy();

            Instance = Utils.CreateGameObject<HiddenPool>();
            Instance.Init();
        }

        public static void Destroy()
        {
            if (Instance == null)
                return;

            Instance = null;
        }

        private void Init()
        {
            gameObject.SetActive(false);
        }
    }

    public class HiddenPoolComponent : MonoBehaviour
    {
        private Transform originalParent = null;
        private VisibilityState state = VisibilityState.UNKNOWN;

        public enum VisibilityState
        {
            HIDDEN,
            VISIBLE,
            UNKNOWN,
        }

        void Start()
        {
            SaveParent();
        }

        private void SaveParent()
        {
            var pool = HiddenPool.Instance;
            var parent = gameObject.transform.parent;
            if (pool == null || parent != pool.transform) // not already in the hidden pool
            {
                originalParent = parent;
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (isVisible) Show();
            else Hide();
        }

        public void Hide()
        {
            var pool = HiddenPool.Instance;
            if (pool == null)
                return;

            if (state == VisibilityState.HIDDEN)
                return;

            SaveParent();
            gameObject.transform.SetParent(pool.transform, false);

            state = VisibilityState.HIDDEN;
            Utils.Hide(gameObject);
        }

        public void Show() => Show(originalParent);
        public void Show(Transform parent)
        {
            if (state == VisibilityState.VISIBLE)
                return;

            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
                originalParent = null;
            }

            state = VisibilityState.VISIBLE;
            Utils.Show(gameObject);
        }
    }
}
