using UnityEngine;

namespace Monster
{
    public class EnCounterManager
    {
        private const float EnCounterChance = 0.15f;
        private TilemapManager mTilemapManager;

        public void Initialize(TilemapManager tilemapManager)
        {
            mTilemapManager = tilemapManager;
        }

        public bool TryEncounter(Vector3Int cellPosition)
        {
            if (mTilemapManager == null)
            {
                return false;
            }

            // 현재 위치가 수풀이 아니면 인카운터 발생 안 함
            if (!mTilemapManager.IsGrassTile(cellPosition))
            {
                return false;
            }

            // 수풀이라면 확률 판정
            return Random.value <= EnCounterChance;
        }

        public void Shutdown()
        {
            mTilemapManager = null;
        }
    }
}