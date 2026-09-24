using System;
using PracticeMath.UI;
using UnityEngine;

namespace PracticeMath.Navigation
{
    /// <summary>Persisted UI color scheme selection (hub and other themed screens).</summary>
    public sealed class AppThemeContext : MonoBehaviour
    {
        private const string PrefScheme = "practice_math.color_scheme";

        public static AppThemeContext Instance { get; private set; }

        public event Action Changed;

        public int SelectedSchemeIndex { get; private set; }

        public UiColorSchemePalette CurrentPalette => UiColorSchemeCatalog.Get(SelectedSchemeIndex);

        public UiThemeResolvedRoles CurrentRoles => CurrentPalette.ResolveRoles();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;
            var go = new GameObject(nameof(AppThemeContext));
            go.AddComponent<AppThemeContext>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SelectedSchemeIndex = Mathf.Clamp(PlayerPrefs.GetInt(PrefScheme, 0), 0, UiColorSchemeCatalog.SchemeCount - 1);
        }

        public void SetSelectedScheme(int index)
        {
            index = Mathf.Clamp(index, 0, UiColorSchemeCatalog.SchemeCount - 1);
            if (SelectedSchemeIndex == index)
                return;

            SelectedSchemeIndex = index;
            PlayerPrefs.SetInt(PrefScheme, SelectedSchemeIndex);
            PlayerPrefs.Save();
            UiEventSystemUtility.ClearCurrentSelection();
            Changed?.Invoke();
        }
    }
}
