using Assets.Scripts;
using Assets.Scripts.Objects;
using UnityEngine;
using System.Collections;

namespace cynofield.mods
{
    public class AugmentedDisplayInWorld : MonoBehaviour
    {
        public static AugmentedDisplayInWorld Instance;

        public static void Create(ThingsUi thingsUi)
        {
            Instance = new GameObject("root").AddComponent<AugmentedDisplayInWorld>();
            Instance.thingsUi = thingsUi;
            //ConsoleWindow.Print($"AugmentedDisplayInWorld create {Instance} {Instance.gameObject}");
        }

        public static void Destroy()
        {
            if (Instance == null)
                return;

            foreach (var ann in Instance.annotations)
            {
                (ann as InWorldAnnotation).Destroy();
            }
            Instance.annotations.Clear();
            UnityEngine.Object.Destroy(Instance);
            UnityEngine.Object.Destroy(Instance.gameObject);
            Instance.gameObject.SetActive(false);
            Instance = null;
        }

        private ThingsUi thingsUi;
        private readonly Queue annotations = new Queue();

        void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                var a = new GameObject().AddComponent<InWorldAnnotation>();
                a.Inject(thingsUi);
                annotations.Enqueue(a);
            }
        }

        private float periodicUpdateCounter;
        void Update()
        {
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
        }

        public void Show(Thing thing, RaycastHit hit)
        {
            if (thing == null)
                return;

            var thingId = GetId(thing);

            foreach (var obj in annotations)
            {
                // return if there is already shown annotation for this thing
                var a = (obj as InWorldAnnotation);
                if (a.id == thingId && a.IsActive())
                    return; // TODO update description?
            }

            var ann = annotations.Dequeue() as InWorldAnnotation;
            annotations.Enqueue(ann);
            ann.ShowNear(thing, thingId, hit);
        }

        string GetId(Thing thing) { return thing.NetworkId.ToString(); }
    }
}
