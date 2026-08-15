using TMPro;
using UnityEngine;

namespace Monster
{
    public class Warp : MonoBehaviour
    {
        [Header("충돌체")]
        [SerializeField] public BoxCollider2D mWarpCollider;

        [Header("워프 위치")]
        [SerializeField] private Transform mTargetPosition;

        private Player mPlayer;

        private void Awake()
        {
            mPlayer         = FindAnyObjectByType<Player>();
            mWarpCollider   = GetComponent<BoxCollider2D>();
        }

        public void WarpPlayer()
        {
            if (mPlayer == null)
            {
                mPlayer = FindAnyObjectByType<Player>();
            }

            if (mPlayer == null)
            {
                Debug.LogWarning("Player를 찾을 수 없습니다.");

                return;
            }

            if (mTargetPosition == null)
            {
                Debug.LogWarning("Player를 찾을 수 없습니다.");

                return;
            }

            mPlayer.transform.position = mTargetPosition.position;
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