using PracticeMath.Core;
using UnityEngine;

namespace PracticeMath.Navigation
{
    /// <summary>Global hub grade and launch intent across scenes.</summary>
    public sealed class AppSessionContext : MonoBehaviour
    {
        private const string PrefGrade = "practice_math.hub_grade";

        public static AppSessionContext Instance { get; private set; }

        [SerializeField] private GradeLevel defaultGrade = GradeLevel.Grade3;

        public GradeLevel SelectedGrade { get; private set; }
        public LearningModule ActiveModule { get; set; } = LearningModule.Practice;

        /// <summary>Times table row to drill, or -1 for mixed 0–12.</summary>
        public int TimesTableFocus { get; set; } = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null)
                return;
            var go = new GameObject(nameof(AppSessionContext));
            go.AddComponent<AppSessionContext>();
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
            SelectedGrade = (GradeLevel)PlayerPrefs.GetInt(PrefGrade, (int)defaultGrade);
            ClampGrade();
        }

        public void SetSelectedGrade(GradeLevel grade)
        {
            SelectedGrade = grade;
            ClampGrade();
            PlayerPrefs.SetInt(PrefGrade, (int)SelectedGrade);
            PlayerPrefs.Save();
        }

        private void ClampGrade()
        {
            if ((int)SelectedGrade < (int)GradeLevel.Grade1)
                SelectedGrade = GradeLevel.Grade1;
            if ((int)SelectedGrade > (int)GradeLevel.Grade4)
                SelectedGrade = GradeLevel.Grade4;
        }
    }
}
