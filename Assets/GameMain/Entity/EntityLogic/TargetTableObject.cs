using UnityEngine;
using UnityGameFramework.Runtime;
using GameFramework.Event;

namespace Monster
{
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
