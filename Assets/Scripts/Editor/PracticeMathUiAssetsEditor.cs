#if UNITY_EDITOR
using PracticeMath.UI;
using UnityEditor;
using UnityEngine;

namespace PracticeMath.Editor
{
    public static class PracticeMathUiAssetsEditor
    {
        private const string AssetPath = "Assets/Resources/PracticeMathUiAssets.asset";
        private const string BackgroundSpritePath = "Assets/ArtAssets/Textures/bg_01.png";

        [MenuItem("Practice Math/UI/Ensure UI Assets (background sprite)")]
        public static void EnsureUiAssets()
        {
            EnsureResourcesFolder();

            var asset = AssetDatabase.LoadAssetAtPath<PracticeMathUiAssets>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PracticeMathUiAssets>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
            if (sprite == null)
            {
                var objects = AssetDatabase.LoadAllAssetsAtPath(BackgroundSpritePath);
                foreach (var o in objects)
                {
                    if (o is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            var so = new SerializedObject(asset);
            so.FindProperty("screenBackground").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(
                "Practice Math UI",
                "PracticeMathUiAssets is ready under Assets/Resources/.\n\n" +
                "All scenes use bg_01 as the shared screen background.",
                "OK");
        }

        public static void EnsureUiAssetsSilent()
        {
            EnsureResourcesFolder();

            var asset = AssetDatabase.LoadAssetAtPath<PracticeMathUiAssets>(AssetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PracticeMathUiAssets>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath);
            if (sprite == null)
            {
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(BackgroundSpritePath))
                {
                    if (o is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            var so = new SerializedObject(asset);
            so.FindProperty("screenBackground").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureResourcesFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
        }
    }
}
#endif
