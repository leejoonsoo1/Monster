using UnityEngine.SceneManagement;
using UnityGameFramework.Runtime;

namespace Monster
{
    // <summary>
    // 게임에서 사용하는 씬 전환을 관리하는 클래스.
    // Logo, Main, Battle 같은 씬 로드를 담당.
    // </summary>
    public class GameSceneManager
    {
        // <summary>
        // Logo 씬 이름.
        // </summary>
        private const string mLogoSceneName = "Logo";

        // <summary>
        // Main 씬 이름.
        // </summary>
        private const string mMainSceneName = "Main";

        // <summary>
        // Battle 씬 이름.
        // </summary>
        private const string mBattleSceneName = "Battle";

        // <summary>
        // Logo 씬을 로드
        // </summary>
        public void LoadLogoScene()
        {
            Log.Info("Load Logo Scene");

            SceneManager.LoadScene(mLogoSceneName);
        }

        // <summary>
        // Main 씬을 로드
        // </summary>
        public void LoadMainScene()
        {
            Log.Info("Load Main Scene");

            SceneManager.LoadScene(mMainSceneName);
        }

        // <summary>
        // Battle 씬을 로드
        // </summary>
        public void LoadBattleScene()
        {
            Log.Info("Load Battle Scene");

            SceneManager.LoadScene(mBattleSceneName);
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
