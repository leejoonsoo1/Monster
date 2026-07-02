using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework.Event;

namespace Monster
{
    // TargetTableObject의 역할은 플레이어, 몬스터, NPC 등 다른 객체와 상호작용(충돌, 타겟팅)이 가능한 모든 엔티티의 공통 부모 클래스
    public class TargetTableObject : EntityLogic
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
        }

        protected internal override void OnShow(object userData)
        {
            base.OnShow(userData);
            mTargetableObjectData = userData as TargetableObjectData;

            if (mTargetableObjectData == null)
            {
                Log.Error("Targetable object data is invalid.");
                return;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Entity otherEntity = other.gameObject.GetComponent<Entity>();
            if (otherEntity == null)
            {
                return;
            }

            if (otherEntity.Logic is TargetTableObject && otherEntity.Id >= Entity.Id)
            {
                return;
            }
        }

        [SerializeField]
        private TargetableObjectData mTargetableObjectData = null;
    }
}
