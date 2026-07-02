using System;
using UnityEngine;

namespace Monster
{
    // PlayerData의 역할은 플레이어 엔티티를 생성하거나 초기화할 때 필요한 데이터를 담아 전달하는 데이터 클래스
    [Serializable]
    public class PlayerData : TargetableObjectData
    {
        //[SerializeField] private int mMaxHP = 100;
        [SerializeField] private float mMoveSpeed = 5f;

        public Vector3 Position { get; private set; }

        public PlayerData(int entityId, int typeId, Vector3 position) : base(entityId, typeId)
        {
            Position = position;
        }

        public float MoveSpeed
        {
            get => mMoveSpeed;
            set => mMoveSpeed = Mathf.Max(1f, value);
        }
    }
}
