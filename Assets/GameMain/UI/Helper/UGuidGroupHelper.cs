using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class UGuiGroupHelper : UIGroupHelperBase
    {
        private const int DepthFactor = 1000;

        private int mDepth = 0;
        private Canvas mCanvas;

        private void Awake()
        {
            // UI Group에 Canvas가 없으면 생성합니다.
            mCanvas = GetComponent<Canvas>();

            if (mCanvas == null)
            {
                mCanvas = gameObject.AddComponent<Canvas>();
            }

            // UI를 화면에 랜더링합니다.
            mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mCanvas.overrideSorting = true;

            // 버튼 등 UI 입력 처리를 위한 GraphicRaycaster를 추가합니다.
            GraphicRaycaster graphicRaycaster = GetComponent<GraphicRaycaster>();

            if (graphicRaycaster == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void Start()
        {
            if (mCanvas != null)
            {
                mCanvas.overrideSorting = true;
                mCanvas.sortingOrder = DepthFactor * mDepth;
            }

            // UI Group을 화면 전체 크기로 맞춥니다.
            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchorMin         = Vector2.zero;
            rectTransform.anchorMax         = Vector2.one;

            rectTransform.anchoredPosition  = Vector2.zero;
            rectTransform.sizeDelta         = Vector2.zero;
        }

        public override void SetDepth(int depth)
        {
            mDepth = depth;

            if (mCanvas == null)
            {
                return;
            }

            mCanvas.overrideSorting = true;
            mCanvas.sortingOrder    = DepthFactor * depth;
        }
    }
}