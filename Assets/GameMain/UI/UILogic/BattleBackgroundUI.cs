using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleBackgroundUI : UIFormLogic
    {
        // Bttle Background의 가로 크기입니다.
        private const float mWidth = 1440f;

        // Battle Background의 세로 크기입니다.
        private const float mHeight = 810;

        // 배경 이미지가 들어갈 RectTransform입니다.
        // Prefab에 별도의 BackgroundRoot가 있다면 Inspector에서 연결합니다.
        //[SerializeField]
        //private RectTransform mBackgroundRoot;
        protected internal override void OnOpen(object userData)
        {
            // 부모호출
            base.OnOpen(userData);

            Log.Info("BattleBackgroundUI : Open");

            // 현재 배경 UI Form의 RectTransform을 가져옵니다.
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                Log.Error("BattleBackgroundUI : RectTransform을 찾을 수 없습니다.");

                return;
            }

            // =========================================
            // Anchor 설정
            // =========================================

            // UI Group의 위쪽 중앙을 기준으로 설정합니다.
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);

            rectTransform.pivot = new Vector2(0.5f, 1f);

            // =========================================
            // 위치 설정
            // =========================================

            // 화면 상단 중앙에 배치합니다.
            rectTransform.anchoredPosition = Vector2.zero;

            // =========================================
            // 크기 설정
            // =========================================
            rectTransform.sizeDelta = new Vector2(mWidth, mHeight);
            rectTransform.localScale = Vector3.one;
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {

            Log.Info("BattleBackgroundUI : Close");

            // 부모 UIFormLogic의 종료 처리를 호출합니다.
            base.OnClose(isShutdown, userData);
        }
    }
}