using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleBackgroundManager
    {
        private readonly Dictionary<string, string> mBackgroundPaths = 
            new Dictionary<string, string>();

        public void Initialize()
        {
            mBackgroundPaths.Add("Route", "Route_bg");
        }

        // <summary>
        // 현재 Map 이름에 맞는 Battle Background 경로를 반환합니다.
        // </summary>
        public string GetBackgroundAssetName(string mapName)
        {
            if (string.IsNullOrEmpty(mapName) == true)
            {
                Log.Error("BattleBackgroundManager : Map 이름이 없습니다.");

                return string.Empty;
            }

            if (mBackgroundPaths.TryGetValue(mapName, out string backgroundAssetName) != true)
            {
                Log.Error($"BattleBackgroundManager : " + $"등록되지 않은 Map입니다. Map = {mapName}");

                return string.Empty;
            }

            return backgroundAssetName;
        }

        public void shutdown()
        {
            mBackgroundPaths.Clear();
        }
    }
}