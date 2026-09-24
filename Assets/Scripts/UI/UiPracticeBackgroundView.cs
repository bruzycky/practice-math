using UnityEngine;
using UnityEngine.UI;

namespace PracticeMath.UI
{
    /// <summary>Single full-screen bg_01 image on the canvas root (not on nested UI prefabs).</summary>
    [DisallowMultipleComponent]
    public sealed class UiPracticeBackgroundView : MonoBehaviour
    {
        public const string ObjectName = "ScreenBackground";

        [SerializeField] private Image backgroundImage;

        /// <summary>Exactly one background on a <see cref="Canvas"/> root.</summary>
        public static UiPracticeBackgroundView Ensure(Transform canvasRoot)
        {
            if (canvasRoot == null)
                return null;

            if (canvasRoot.GetComponent<Canvas>() == null)
                return null;

            var view = UiBackgroundCleanup.DedupeOnCanvas(canvasRoot);
            if (view == null)
                view = CreateBackground(canvasRoot);
            else
                view.transform.SetAsFirstSibling();

            view.RefreshSprite();
            HideLegacyDecorativeBackground(canvasRoot);
            return view;
        }

        private static UiPracticeBackgroundView CreateBackground(Transform canvasRoot)
        {
            var go = new GameObject(ObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UiPracticeBackgroundView));
            go.transform.SetParent(canvasRoot, false);
            go.transform.SetAsFirstSibling();

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = false;
            img.type = Image.Type.Simple;
            img.color = Color.white;

            var view = go.GetComponent<UiPracticeBackgroundView>();
            view.backgroundImage = img;
            return view;
        }

        private static void HideLegacyDecorativeBackground(Transform canvasRoot)
        {
            foreach (Transform child in canvasRoot)
            {
                if (child.name != "Image")
                    continue;
                var img = child.GetComponent<Image>();
                if (img == null || img.sprite == null)
                    continue;
                var assets = PracticeMathUiAssets.Instance;
                if (assets != null && assets.ScreenBackground != null && img.sprite == assets.ScreenBackground)
                    child.gameObject.SetActive(false);
            }
        }

        private void Reset()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }

        public void RefreshSprite()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
                return;

            var assets = PracticeMathUiAssets.Instance;
            if (assets != null && assets.ScreenBackground != null)
                backgroundImage.sprite = assets.ScreenBackground;
        }
    }
}
