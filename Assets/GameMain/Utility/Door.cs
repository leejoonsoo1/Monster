using Unity.VisualScripting;
using UnityEngine;

namespace Monster
{ 
    public class Door : MonoBehaviour
    {
        [Header("문 충돌체")]
        [SerializeField] public BoxCollider2D doorCollider;

        private void Awake()
        {
            doorCollider = GetComponent<BoxCollider2D>();

            if (doorCollider == null)
            {
                doorCollider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        public void ColliderOn()
        {
            if (doorCollider != null)
            {
                doorCollider.enabled = true;
            }
        }

        public void ColliderOff()
        {
            if (doorCollider != null)
            {
                doorCollider.enabled = false;
            }
        }
    }
}
