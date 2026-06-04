using System;
using UnityEngine;

namespace Monster
{
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
