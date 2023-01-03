using cynofield.mods.utils;
using UnityEngine;

namespace cynofield.mods.ui
{
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

        public void Show()
        {
            if (state == VisibilityState.VISIBLE)
                return;

            if (originalParent != null)
            {
                gameObject.transform.SetParent(originalParent, false);
                originalParent = null;
            }

            state = VisibilityState.VISIBLE;
            Utils.Show(gameObject);
        }
    }
}