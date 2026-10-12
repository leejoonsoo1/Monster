using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleUI : UIFormLogic
    {
        // Battle UI의 크기입니다.
        private const float mWidth  = 1440f;
        private const float mHeight = 810f;

        // 상대 몬스터 체력 UI입니다.
        [SerializeField]
        private RectTransform mEnemyStatusRoot;

        // 내 몬스터 체력 UI입니다.
        [SerializeField]
        private RectTransform mPlayerStatusRoot;

        // 하단 명령 UI 영역입니다.
        [SerializeField]
        private RectTransform mBottomRoot;

        [SerializeField]
        private Image mEnemyHPFill;

        // 내 몬스터 체력 UI입니다.
        [SerializeField]
        private Image mPlayerHPFill;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            Log.Info("BattleUI : Open");

            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                Log.Error("BattleUI : RectTransform을 찾을 수 없습니다.");

                return;
            }

            // BattleBackgroundUI와 같은 크기와 위치로 설정합니다.
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot     = new Vector2(0.5f, 1f);

            rectTransform.anchoredPosition  = Vector2.zero;
            rectTransform.sizeDelta         = new Vector2(mWidth, mHeight);
            rectTransform.localScale        = Vector3.one;

            // 상대 체력 UI 배치
            if (mEnemyStatusRoot != null)
            {
                // 왼쪽 상단 기준
                mEnemyStatusRoot.anchorMin  = new Vector2(0f, 1f);
                mEnemyStatusRoot.anchorMax  = new Vector2(0f, 1f);
                mEnemyStatusRoot.pivot      = new Vector2(0f, 1f);

                // 왼쪽 상단에서의 위치
                mEnemyStatusRoot.anchoredPosition   = new Vector2(0f, -80f);
                mEnemyStatusRoot.sizeDelta          = new Vector2(600f, 160f);
            }

            // 내 체력 UI 배치
            if (mPlayerStatusRoot != null)
            {
                // 오른쪽 하단 기준
                mPlayerStatusRoot.anchorMin = new Vector2(1f, 0f);
                mPlayerStatusRoot.anchorMax = new Vector2(1f, 0f);
                mPlayerStatusRoot.pivot     = new Vector2(1f, 0f);

                // 오른쪽 하단에서의 위치
                mPlayerStatusRoot.anchoredPosition  = new Vector2(0f, 300f);
                mPlayerStatusRoot.sizeDelta         = new Vector2(600f, 180f);
                mPlayerStatusRoot.localScale        = UnityEngine.Vector3.one; 
            }

            if (mBottomRoot != null)
            {
                mBottomRoot.anchorMin   = new Vector2(0.5f, 1f);
                mBottomRoot.anchorMax   = new Vector2(0.5f, 1f);
                mBottomRoot.pivot       = new Vector2(0.5f, 1f);

                mBottomRoot.anchoredPosition    = new Vector2(0f, -810f);
                mBottomRoot.sizeDelta           = new Vector2(1440f, 270f);
            }

            Transform enemyStatus1 = transform.Find("EnemyStatus1");
            Transform enemyStatus2 = transform.Find("EnemyStatus2");

            Transform playerStatus1 = transform.Find("PlayerStatus1");
            Transform playerStatus2 = transform.Find("PlayerStatus2");

            // 상대 몬스터 1 체력바 활성화
            if (enemyStatus1 != null)
            {
                enemyStatus1.gameObject.SetActive(true);
            }

            if (enemyStatus2 != null)
            {
                enemyStatus2.gameObject.SetActive(false);
            }

            if (playerStatus1 != null)
            {
                playerStatus1.gameObject.SetActive(true);
            }

            if (playerStatus2 != null)
            {
                playerStatus2.gameObject.SetActive(false);
            }

            // HP 게이지 초기화
            InitializeHPBar(mEnemyHPFill);
            InitializeHPBar(mPlayerHPFill);

            // 현재는 데이터 연동 전이므로 두 체력바를 100%로 표시합니다.
            SetHP(100, 100, 70, 70);

            Log.Info("BattleUI : Single Battle UI 활성화 완료");
        }

        // HP 게이지의 표시 방식을 설정합니다
        private void InitializeHPBar(Image hpFill)
        {
            if (hpFill == null)
            {
                Log.Warning("BattleUI : HP Fill Image가 연결되지 않았습니다.");

                return;
            }

            // HP가 줄어들 때 이미지가 왼쪽부터 남도록 설정합니다.
            hpFill.type         = Image.Type.Filled;
            hpFill.fillMethod   = Image.FillMethod.Horizontal;
            hpFill.fillOrigin   = (int)Image.OriginHorizontal.Left;

            // 처음에는 체력을 가득 체웁니다.
            hpFill.fillAmount = 1f;
        }

        // 상대와 아군의 현재 HP를 UI에 반영합니다.
        public void SetHP(int enemyCurrentHP, int enemyMaxHP, int playerCurrentHP, int playerMaxHP)
        {
            if (mEnemyHPFill != null && enemyMaxHP > 0)
            {
                float enemyHPRatio = Mathf.Clamp01((float)enemyCurrentHP / enemyMaxHP);

                mEnemyHPFill.fillAmount = enemyHPRatio;
            }

            if (mPlayerHPFill != null && playerMaxHP > 0)
            {
                float playerHPRatio = Mathf.Clamp01((float)playerCurrentHP / playerMaxHP);

                mPlayerHPFill.fillAmount = playerHPRatio;
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            Log.Info("BattleUI : Close");

            base.OnClose(isShutdown, userData);
        }
    }
}