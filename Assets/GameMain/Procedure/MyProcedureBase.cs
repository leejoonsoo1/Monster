using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace Monster
{
    public abstract class MyProcedureBase : GameFramework.Procedure.ProcedureBase
    {
        // <summary>
        // Logo 씬 이름.
        // </summary>
        private const string LogoSceneName = "Logo";

        // <summary>
        // Main 씬 이름.
        // </summary>
        private const string MainSceneName = "Main";

        // <summary>
        // Battle 씬 이름.
        // </summary>
        private const string BattleSceneName = "Battle";

        // <summary>
        // Logo 씬을 로드
        // </summary>
        public void LoadLogoScene()
        {
            Log.Info("Load Logo Scene");

            SceneManager.LoadScene(LogoSceneName);
        }

        // <summary>
        // Main 씬을 로드
        // </summary>
        public void LoadMainScene()
        {
            Log.Info("Load Main Scene");

            SceneManager.LoadScene(MainSceneName);
        }

        // <summary>
        // Battle 씬을 로드
        // </summary>
        public void LoadBattleScene()
        {
            Log.Info("Load Battle Scene");

            SceneManager.LoadScene(BattleSceneName);
        }

        // <summary>
        // 씬 이름을 받아 해당 씬을 로드
        // </summary>
        // <param name="sceneName">로드할 씬 이름</param>
        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Log.Warning("Scene name is  null or empty");

                return;
            }

            Log.Info("Load Scene : {0}", sceneName);
            SceneManager.LoadScene(sceneName);
        }
    }
}
