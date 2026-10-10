using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class UGuiGroupHelper : UIGroupHelperBase
    {
        // UI Group의; Depth마다 Sorting Order를 일정 간격으로 배치하기 위한 값이다.
        private const int DepthFactor = 1000;

        // 현재 UI Group의 Depth를 저장합니다.
        private int mDepth = 0;

        // 현재 UI Group의 Depth를 저장합니다.
        private Canvas mCanvas;

        private void Awake()
        {
            // 현재 UI Group GameObject에 붙어 있는 Canvas 컴포넌트를 가져옵니다.
            mCanvas = GetComponent<Canvas>();

            // Canvas가 존재하지 않는 경우 현재 UI Group GameObject에 Canvas를 추가합니다.
            if (mCanvas == null)
            {
                mCanvas = gameObject.AddComponent<Canvas>();
            }

            // UI를 카메라와 관계없이 화면 위에 직접 출력하도록 설정합니다.
            mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 각 UI Group마다 별도의 Sorting Order를 사용할 수 있도록 설정합니다.
            mCanvas.overrideSorting = true;

            // 버튼 등 UI 입력 처리를 담당하는 GraphicRaycaster를 가져옵니다.
            GraphicRaycaster graphicRaycaster = GetComponent<GraphicRaycaster>();

            // GraphicRaycaster가 없다면 현재 UI Group GameObject에 추가합니다.
            if (graphicRaycaster == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void Start()
        {
            // Canvas가 정상적으로 존재하는 경우 현재 UI Group의 Depth에 맞게 Sorting Order를 설정합니다.
            if (mCanvas != null)
            {
                mCanvas.overrideSorting = true;
                mCanvas.sortingOrder = DepthFactor * mDepth;
            }

            // UI Group GameObject의 RectTransform을 가져옵니다.
            RectTransform rectTransform = transform as RectTransform;

            // RectTransform이 아니라면 UI 크기를 설정할 수 없으므로 종료합니다.
            if (rectTransform == null)
            {
                return;
            }

            // UI Group이 부모 영역 전체를 사용하도록 Anchor를 Stretch 상태로 설정합니다.
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;

            // 부모 RectTransform을 기준으로 좌우 여백을 0으로 설정합니다.
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Scale도 기본값으로 맞춥니다.
            rectTransform.localScale = Vector3.one;
        }

        public override void SetDepth(int depth)
        {
            // UGF에서 전달받은 UI Group Depth를 저장합니다.
            mDepth = depth;

            // Canvas가 아직 생성되지 않았다면 Sorting Order를 설정할 수 없으므로 종료합니다.
            if (mCanvas == null)
            {
                return;
            }

            // 이 Canvas에서 독립적인 Sorting Order를 사용합니다.
            mCanvas.overrideSorting = true;

            // UI Group Depth를 실제 Canvas Sorting Order로 변환합니다.
            mCanvas.sortingOrder    = DepthFactor * depth;
        }
    }
}