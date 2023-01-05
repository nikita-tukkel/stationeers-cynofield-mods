using Assets.Scripts.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace cynofield.mods.utils
{
    public interface IHierarchy : IEnumerable
    {
        IHierarchy Parent { get; }
    }

    public static class HierarchyExtensions
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static void Destroy(this IHierarchy hierarchy)
        {
            if (hierarchy == null)
                return;
            hierarchy.Traverse(
                (o) => o as IHierarchy,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.Destroy(obj);
                            break;
                        case Component obj:
                            Utils.Destroy(obj);
                            break;
                    }
                });
        }

        public static void Hide(this IHierarchy hierarchy)
        {
            if (hierarchy == null)
                return;
            hierarchy.Traverse(
                (o) => o as IHierarchy,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.Hide(obj);
                            break;
                        case Component obj:
                            Utils.Hide(obj);
                            break;
                    }
                });
        }

        public static void Show(this IHierarchy hierarchy)
        {
            if (hierarchy == null)
                return;
            hierarchy.Traverse(
                (o) => o as IHierarchy,
                (o) =>
                {
                    switch (o)
                    {
                        case GameObject obj:
                            Utils.Show(obj);
                            break;
                        case Component obj:
                            Utils.Show(obj);
                            break;
                    }
                });
        }

        public delegate IEnumerable TraverseDelegate(object node);
        public delegate void ActionDelegate(object node);

        /// <summary>
        /// Traverse the hierarchy, providing null, duplicates and infinite loop safety.
        /// `method` is applied only once to each item in the hierarchy.
        /// `traverser` is applied only once to each item in the hierarchy.
        /// `method` is applied to parent only after it was applied to all children and children of children.
        /// Contains infinite loop protection for wild corner cases like infinite IEnumerator.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="traverser">leafs must return `null`, nodes must return IEnumerable</param>
        /// /// <param name="method"></param>
        public static void Traverse(this IEnumerable hierarchy,
        TraverseDelegate traverser, ActionDelegate method)
        {
            var handled = new HashSet<object>();
            var traversed = new HashSet<object>();
            var planned = new Stack();
            planned.Push(hierarchy);
            int watchdogCount = 0;
            int methodCalls = 0;
            while (planned.Count > 0)
            {
                if (watchdogCount++ > 500_000)
                {
                    throw new InvalidOperationException($"Infinite traverse error for {hierarchy}");
                }

                var node = planned.Peek();
                if (node == null || handled.Contains(node))
                {
                    planned.Pop();
                    continue;
                }

                if (traversed.Contains(node))
                {
                    planned.Pop();
                    handled.Add(node);
                    method(node);
                    methodCalls++;
                }
                else
                {
                    traversed.Add(node);
                    var nextLevel = traverser(node);
                    if (nextLevel is IEnumerable)
                    {
                        var iter = (nextLevel as IEnumerable).GetEnumerator();
                        if (iter != null)
                        {
                            while (iter.MoveNext()) // starts before the first element on enumerator creation
                            {
                                var current = iter.Current;
                                if (current == null || traversed.Contains(current))
                                    continue;
                                planned.Push(current);
                            }
                        }
                    }
                }
            }

            //Log.Debug(() => $"Traverse iterations={watchdogCount}, method calls={methodCalls}");
        }
    }

    public class Utils
    {
        public static string PrintHierarchy(GameObject root, string ident = "")
        {
            string result = ident + "---" + root + "\n";
            result += PrintHierarchyInternal(root, ident);
            return result;
        }

        private static string PrintHierarchyInternal(GameObject root, string ident = "")
        {
            string result = "";
            result += PrintComponents(root, ident);
            for (var i = 0; i < root.transform.childCount; i++)
            {
                var child = root.transform.GetChild(i).gameObject;
                var childIdent = ident + "\t";
                result += childIdent + "---" + child + "\n";
                result += PrintHierarchyInternal(child, childIdent);
            }
            return result;
        }

        public static string PrintComponents(GameObject root, string ident = "")
        {
            string result = "";
            foreach (var c in root.GetComponents<Component>())
            {
                result += ident + c.GetType() + "\n";
            }
            return result;
        }

        public static T CreateGameObject<T>() where T : Component => CreateGameObject<T>((Transform)null);
        public static T CreateGameObject<T>(GameObject parent) where T : Component => CreateGameObject<T>(parent ? parent.transform : null);
        public static T CreateGameObject<T>(Component parent) where T : Component => CreateGameObject<T>(parent ? parent.transform : null);
        public static T CreateGameObject<T>(Transform parent) where T : Component
        {
            var result = new GameObject().AddComponent<T>();
            result.name = $"{typeof(T)}";
            result.gameObject.SetActive(false);
            if (parent != null)
                result.transform.SetParent(parent, false);
            return result;
        }
        public static GameObject CreateGameObject(GameObject parent)
        {
            var result = new GameObject();
            result.SetActive(false);
            if (parent != null)
                result.transform.SetParent(parent.transform, false);
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
        public static void Show(Component obj) => Show(obj ? obj.gameObject : null);
        public static void Show(GameObject obj)
        {
            // SetActive doesn't make initially inactive objects active,
            //  so have to use deprecated method, or recurse children manually.
            if (obj != null)
                obj.SetActiveRecursively(true);
        }

        public static void Hide(Component obj) => Hide(obj ? obj.gameObject : null);
        public static void Hide(GameObject obj)
        {
            if (obj != null)
                obj.SetActiveRecursively(false);
        }

        public static string GetId(Thing thing) { return thing == null ? "" : thing.ReferenceId.ToString(); }

        public static VerticalLayoutGroup VL(Component parent)
        {
            VerticalLayoutGroup layout;
            if (parent is Canvas)
            {
                // Don't create intermediate GameObject when adding into Canvas
                layout = parent.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            else
            {
                layout = VL(parent.gameObject);
            }
            return VLInit(layout);
        }
        public static VerticalLayoutGroup VL(GameObject parent)
        {
            var layout = CreateGameObject<VerticalLayoutGroup>(parent);
            return VLInit(layout);
        }
        private static VerticalLayoutGroup VLInit(VerticalLayoutGroup layout)
        {
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(5, 5, 5, 5);
            return layout;
        }
        public static HorizontalLayoutGroup HL(Component parent)
        {
            return HLInit(HL(parent.gameObject));
        }
        public static HorizontalLayoutGroup HL(GameObject parent)
        {
            var layout = CreateGameObject<HorizontalLayoutGroup>(parent);
            return HLInit(layout);
        }
        private static HorizontalLayoutGroup HLInit(HorizontalLayoutGroup layout)
        {
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
            layout.spacing = 10;
            layout.padding = new RectOffset(5, 5, 5, 5);
            return layout;
        }
    }
}
