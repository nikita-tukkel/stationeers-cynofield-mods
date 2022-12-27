using Assets.Scripts;
using Assets.Scripts.Objects;
using UnityEngine;
using System.Collections;

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
                    return;
            }

            var ann = annotations.Dequeue() as InWorldAnnotation;
            annotations.Enqueue(ann);
            ann.ShowNear(thing, thingId, hit);
        }

        string GetId(Thing thing) { return thing.NetworkId.ToString(); }
    }
}
