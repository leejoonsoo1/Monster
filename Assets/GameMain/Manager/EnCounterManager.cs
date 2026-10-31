using UnityEngine;

namespace Monster
{
    public class EnCounterManager
    {
        private const int StepsPerEncounter = 5;
        private TilemapManager mTilemapManager;
        private int mGrassStepCount;

        public void Initialize(TilemapManager tilemapManager)
        {
            mTilemapManager = tilemapManager;
            mGrassStepCount = 0;
        }

        public bool TryEncounter(Vector3Int cellPosition)
        {
            if (mTilemapManager == null)
            {
                return false;
            }

            // 현재 위치가 수풀이 아니면 인카운터 발생 안 함
            if (mTilemapManager.IsGrassTile(cellPosition) != true)
            {
                return false;
            }

            // 수풀 위로 이동을 완료한 횟수만 누적합니다.
            mGrassStepCount++;

            if (mGrassStepCount < StepsPerEncounter)
            {
                return false;
            }

            mGrassStepCount = 0;
            return true;
        }

        public void Shutdown()
        {
            mTilemapManager = null;
            mGrassStepCount = 0;
        }
    }
}
