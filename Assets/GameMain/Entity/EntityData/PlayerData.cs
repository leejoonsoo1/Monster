using System;
using UnityEngine;

namespace Monster
{
    // PlayerData의 역할은 플레이어 엔티티를 생성하거나
    // 초기화할 때 필요한 데이터를 담아 전달하는 데이터 클래스
    [Serializable]
    public class PlayerData : TargetTableObjectData
    {
        // [SerializeField] private int mMaxHP = 100;
        [SerializeField] private float mMoveSpeed = 9.5f;

        // Player가 생성될 때 바라볼 방향입니다.
        public Vector2 Direction { get; private set; }

        public Vector3 Position { get; private set; }

        public PlayerData(int id, int typeId, Vector3 position) : base(id, typeId, EObjectState.Idle)
        {
            Position = position;

            // 방향을 전달하지 않았으면 기본 아래 방향
            Direction = Vector2.down;
        }

        // 위치 + 방향을 같이 전달받는 생성자
        public PlayerData(
            int id,
            int typeId,
            Vector3 position,
            Vector2 direction) 
            : base (
                  id,
                  typeId,
                  EObjectState.Idle)
        {
            Position = position;

            if (direction == Vector2.zero)
            {
                Direction = Vector2.down;
            }
            else
            {
                Direction = direction;
            }
        }

        public float MoveSpeed
        {
            get => mMoveSpeed;
            set => mMoveSpeed = Mathf.Max(1f, value);
        }
    }
}