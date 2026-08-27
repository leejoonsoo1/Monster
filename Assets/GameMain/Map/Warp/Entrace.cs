using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Monster
{
    public class OpeenTheDoor : MonoBehaviour
    {
        [Header("워프 위치")]
        [SerializeField] private Transform mTargetPosition;

        private Player mPlayer;
        private Grid mGrid;
        private bool mIsPlayerInRange;

        private void Awake()
        {
            mGrid = FindAnyObjectByType<Grid>();
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            Player player = collision.GetComponent<Player>();

            if (player == null)
            {
                return;
            }

            // 문 앞에 들어온 Player 저장
            mPlayer = player;

            // 상호작용 가능한 상태
            mIsPlayerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Player player = collision.GetComponent<Player>();

            if (player == null)
            {
                return;
            }

            // 문 범위를 벗어나면 상호작용 해제
            mIsPlayerInRange = false;
            mPlayer = null;
        }

        private void Update()
        {
            if (!mIsPlayerInRange)
            {
                return;
            }

            if (mPlayer == null)
            {
                return;
            }

            // G를 눌렀을 때만 워프
            if (Input.GetKeyDown(KeyCode.G))
            {
                WarpPlayer();
            }
        }

        private void WarpPlayer()
        {
            if (mTargetPosition == null)
            {
                Debug.LogWarning("워프 위치가 설정되지 않았습니다.");

                return;
            }

            if (mGrid == null)
            {
                Debug.LogWarning("Grid를 찾을 수 없습니다.");

                return;
            }

            // 워프 위치가 속한 Grid 셀을 구합니다.
            Vector3Int targetCell = mGrid.WorldToCell(mTargetPosition.position);

            // 해당 셀의 정확한 중앙 좌표를 구합니다.
            Vector3 targetCenter = mGrid.GetCellCenterWorld(targetCell);

            // 워프 위치 자체가 아니라 
            // Grid 셀의 정중앙으로 Player를 이동시킵니다.
            mPlayer.transform.position = new Vector3(targetCenter.x, targetCenter.y, mPlayer.transform.position.z);
        }
    }
}