using TMPro;
using UnityEngine;

namespace Monster
{
    public class Warp : MonoBehaviour
    {
        [Header("충돌체")]
        [SerializeField] public BoxCollider2D warpCollider;

        [Header("워프 위치")]
        [SerializeField] private Transform targetPosition;

        private Player player;

        private void Awake()
        {
            player = FindAnyObjectByType<Player>();
            warpCollider = GetComponent<BoxCollider2D>();
        }

        public void WarpPlayer()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<Player>();
            }

            if (player == null)
            {
                Debug.LogWarning("Player를 찾을 수 없습니다.");

                return;
            }

            if (targetPosition == null)
            {
                Debug.LogWarning("Player를 찾을 수 없습니다.");

                return;
            }

            player.transform.position = targetPosition.position;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<Player>() != null)
            {
                WarpPlayer();
            }
        }
    }
}