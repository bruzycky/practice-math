using PracticeMath.Core;
using UnityEngine.SceneManagement;

namespace PracticeMath.Navigation
{
    public static class GameSceneLoader
    {
        public static void LoadHome()
        {
            SceneManager.LoadScene(GameScenes.Home);
            SceneUiBootstrap.BootstrapActiveScene();
        }

        public static void Launch(LearningModule module)
        {
            var ctx = AppSessionContext.Instance;
            if (ctx == null)
                return;

            ctx.ActiveModule = module;

            switch (module)
            {
                case LearningModule.Practice:
                case LearningModule.Quiz:
                    SceneManager.LoadScene(GameScenes.PracticeMath);
                    break;
                case LearningModule.TimesTables:
                    SceneManager.LoadScene(GameScenes.TimesTables);
                    break;
                case LearningModule.Geometry:
                case LearningModule.Patterns:
                case LearningModule.Money:
                case LearningModule.Data:
                    SceneManager.LoadScene(GameScenes.MultipleChoice);
                    break;
                default:
                    SceneManager.LoadScene(GameScenes.Home);
                    break;
            }

            SceneUiBootstrap.BootstrapActiveScene();
        }
    }
}
