using Unity.VisualScripting;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{ 
    public class Door : EntityLogic
    {
        [Header("문 충돌체")]
        [SerializeField] private BoxCollider2D mDoorCollider;

        private void Awake()
        {
            mDoorCollider = GetComponent<BoxCollider2D>();

            if (mDoorCollider == null)
            {
                mDoorCollider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        public void ColliderOn()
        {
            if (mDoorCollider != null)
            {
                mDoorCollider.enabled = true;
            }
        }

        public void ColliderOff()
        {
            if (mDoorCollider != null)
            {
                mDoorCollider.enabled = false;
            }
        }
    }
}
