
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleUI : UIFormLogic
    {
        private const float mWidth = 1440f;
        private const float mHeight = 1080f;

        [SerializeField] private Transform mEnemyStatusRoot;
        [SerializeField] private Transform mPlayerStatusRoot;
        [SerializeField] private Transform mBottomRoot;

        [SerializeField] private RectTransform mEnemyStatus1;
        [SerializeField] private RectTransform mEnemyStatus2;
        [SerializeField] private RectTransform mPlayerStatus1;
        [SerializeField] private RectTransform mPlayerStatus2;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            Log.Info("BattleUI : Open");

            RectTransform rootRect = transform as RectTransform;

            if (rootRect != null)
            {
                rootRect.anchorMin          = new Vector2(0.5f, 1f);
                rootRect.anchorMax          = new Vector2(0.5f, 1f);
                rootRect.pivot              = new Vector2(0.5f, 1f);
                rootRect.anchoredPosition   = Vector2.zero;
                rootRect.sizeDelta          = new Vector2(mWidth, mHeight);
                rootRect.localScale         = Vector3.one;
            }
            else
            {
                RectTransform parentRect =
                    transform.parent as RectTransform;

                if (parentRect != null)
                {
                    float centerX =
                        (0.5f - parentRect.pivot.x) *
                        parentRect.rect.width;

                    float topY =
                        (1f - parentRect.pivot.y) *
                        parentRect.rect.height;

                    transform.localPosition =
                        new Vector3(centerX, topY, 0f);
                }
                else
                {
                    Log.Warning(
                        "BattleUI : 부모 RectTransform을 찾지 못했습니다.");
                }

                transform.localScale = Vector3.one;
            }

            SetUIRect(mEnemyStatusRoot,     new Vector2(0f, 1f),    new Vector2(0f, 1f),    new Vector2(0f, -80f),  new Vector2(600f, 160f),    new Vector2(-mWidth * 0.5f, -80f));
            SetUIRect(mPlayerStatusRoot,    new Vector2(1f, 0f),    new Vector2(1f, 0f),    new Vector2(0f, 300f),  new Vector2(600f, 180f),    new Vector2(mWidth * 0.5f, -mHeight + 300f));
            SetUIRect(mBottomRoot,          new Vector2(0.5f, 1f),  new Vector2(0.5f, 1f),  new Vector2(0f, -810f), new Vector2(1440f, 270f),   new Vector2(0f, -810f));

            // 상대 체력바를 왼쪽 상단 영역의 가운데에 배치합니다.
            SetUIRect(mEnemyStatus1, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 120f), new Vector2(300f, -80f));

            // 아군 체력바를 오른쪽 하단 영역의 가운데에 배치합니다.
            SetUIRect(mPlayerStatus1, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 120f), new Vector2(-300f, 90f));

            SetStatusVisible(mEnemyStatus1,     true);
            SetStatusVisible(mEnemyStatus2,     false);
            SetStatusVisible(mPlayerStatus1,    true);
            SetStatusVisible(mPlayerStatus2,    false);

            // Inspector 연결 없이 부모 Root에서 2번 상태창을 찾습니다.
            Transform enemyStatus2  = null;
            Transform playerStatus2 = null;

            if (mEnemyStatusRoot != null)
            {
                enemyStatus2 = mEnemyStatusRoot.Find("EnemyStatus2");
            }

            if (mPlayerStatusRoot != null)
            {
                playerStatus2 = mPlayerStatusRoot.Find("PlayerStatus2");
            }

            // 싱글 배틀에서는 2번 상태창을 비활성화합니다.
            SetStatusVisible(enemyStatus2,  false);
            SetStatusVisible(playerStatus2, false);

            // 2번 상태창을 찾지 못하면 경로를 출력합니다.
            if (enemyStatus2 == null)
            {
                Log.Warning("BattleUI : EnemyStatus2를 찾을 수 없습니다.");
            }

            if (playerStatus2 == null)
            {
                Log.Warning("BattleUI : PlayerStatus2를 찾을 수 없습니다.");
            }

            Log.Info("BattleUI : Single Battle UI 활성화 완료");
        }

        private void SetUIRect(Transform target, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size, Vector2 localPosition)
        {
            if (target == null)
            {
                return;
            }

            RectTransform rect          = target as RectTransform;
            RectTransform parentRect    = target.parent as RectTransform;

            if (rect != null)
            {
                rect.pivot = pivot;
                rect.sizeDelta = size;

                if (parentRect != null)
                {
                    rect.anchorMin          = anchor;
                    rect.anchorMax          = anchor;
                    rect.anchoredPosition   = anchoredPosition;
                }
                else
                {
                    rect.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
                }
            }
            else
            {
                // 일반 Transform에도 위치를 실제 적용합니다.
                target.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
            }

            // RectTransform과 Transform 모두 Scale을 초기화합니다.
            target.localScale = Vector3.one;
        }

        // 중복되어 있던 SetStatusVisible()을 하나로 통합했습니다.
        private void SetStatusVisible(Transform status, bool isVisible)
        {
            if (status == null)
            {
                return;
            }

            if (status.gameObject.activeSelf != isVisible)
            {
                status.gameObject.SetActive(isVisible);
            }
        }

        // HP 게이지 Image 연결 전이므로 기존 빈 함수를 유지합니다.
        public void SetHP(int enemyCurrentHP, int enemyMaxHP, int playerCurrentHP, int playerMaxHP)
        {
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            Log.Info("BattleUI : Close");

            base.OnClose(isShutdown, userData);
        }
    }
}
