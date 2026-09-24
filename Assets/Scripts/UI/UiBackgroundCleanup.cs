using System.Collections.Generic;
using UnityEngine;

namespace PracticeMath.UI
{
    /// <summary>One full-screen bg per canvas — removes legacy CanvasBackground and duplicate ScreenBackground nodes.</summary>
    public static class UiBackgroundCleanup
    {
        public const string LegacyCanvasBackgroundName = "CanvasBackground";

        /// <summary>Keeps at most one <see cref="UiPracticeBackgroundView"/> direct child of the canvas; removes all other background layers.</summary>
        public static UiPracticeBackgroundView DedupeOnCanvas(Transform canvasRoot)
        {
            if (canvasRoot == null)
                return null;

            UiPracticeBackgroundView keeper = null;
            var destroy = new List<GameObject>();

            foreach (Transform child in canvasRoot)
            {
                if (child.name == LegacyCanvasBackgroundName)
                {
                    destroy.Add(child.gameObject);
                    continue;
                }

                if (child.name != UiPracticeBackgroundView.ObjectName)
                    continue;

                if (keeper == null)
                    keeper = child.GetComponent<UiPracticeBackgroundView>();
                else
                    destroy.Add(child.gameObject);
            }

            foreach (var view in canvasRoot.GetComponentsInChildren<UiPracticeBackgroundView>(true))
            {
                if (view.transform.parent == canvasRoot)
                    continue;
                destroy.Add(view.gameObject);
            }

            foreach (Transform t in canvasRoot.GetComponentsInChildren<Transform>(true))
            {
                if (t == canvasRoot || t.parent == canvasRoot)
                    continue;
                if (t.name == UiPracticeBackgroundView.ObjectName || t.name == LegacyCanvasBackgroundName)
                    destroy.Add(t.gameObject);
            }

            foreach (var go in destroy)
            {
                if (go == null)
                    continue;
                if (keeper != null && keeper.gameObject == go)
                    continue;
                DestroyObject(go);
            }

            return keeper;
        }

        /// <summary>Strip background objects from UI sub-prefabs (settings overlay, etc.) that are not the scene canvas.</summary>
        public static void RemoveBackgroundLayersFromSubtree(Transform root)
        {
            if (root == null)
                return;

            var destroy = new List<GameObject>();
            foreach (var view in root.GetComponentsInChildren<UiPracticeBackgroundView>(true))
                destroy.Add(view.gameObject);

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == UiPracticeBackgroundView.ObjectName || t.name == LegacyCanvasBackgroundName)
                    destroy.Add(t.gameObject);
            }

            foreach (var go in destroy)
                DestroyObject(go);
        }

        private static void DestroyObject(Object obj)
        {
            if (obj == null)
                return;
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }
}
