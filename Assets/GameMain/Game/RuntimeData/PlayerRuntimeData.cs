using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Monster
{
    public class PlayerRuntimeData
    {
        // 저장된 필드 정보가 존재하는지 확인합니다.
        public bool mHasSavedFieldState { get; private set; }
        
        // Player가 마지막으로 있던 Grid Cell입니다.
        public Vector3Int mCellPosition { get; private set; }

        // Player가 마지막으로 바라보던 방향입니다.
        public Vector2 mDirection { get; private set; }

        public PlayerRuntimeData()
        {
            mHasSavedFieldState = false;
            mCellPosition = Vector3Int.zero;
            mDirection = Vector2.down;
        }

        // <summary>
        // Battle Scene으로 넘어가기 전에
        // 현재 Player의 필드 상태를 저장합니다.
        // </summary>
        public void SaveFieldState(Vector3Int cellPosition, Vector2 direction)
        {
            mCellPosition = cellPosition;

            if (direction != Vector2.zero)
            {
                mDirection = direction;
            }

            mHasSavedFieldState = true;
        }

        // <summary>
        // 저장된 필드 정보를 초기화합니다.
        // </summary>
        public void Clear()
        {
            mHasSavedFieldState = false;
            mCellPosition = Vector3Int.zero;
            mDirection = Vector2.down;
        }
    }
}