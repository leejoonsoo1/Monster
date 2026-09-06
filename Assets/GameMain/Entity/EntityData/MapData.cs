using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityGameFramework.Runtime;

namespace Monster
{
    // PlayerData의 역할은 플레이어 엔티티를 생성하거나 초기화할 때 필요한 데이터를 담아 전달하는 데이터 클래스
    [Serializable]
    public class MapData : EntityData
    {
        //[SerializeField] private int mMaxHP = 100;
        [SerializeField] private float mMoveSpeed = 9.5f;

        private MapData mMapData;

        public MapData(int id, int typeId, Vector3 position, Quaternion rotation) : base(id, typeId)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}