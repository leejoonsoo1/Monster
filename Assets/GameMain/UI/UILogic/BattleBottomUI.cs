using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleBottomUI : UIFormLogic
    {
        // BattleBattomUI의 크기입니다.
        private const float mWidth = 1440f;
        private const float mHeight = 270f;

        // 각 버튼의 선택 상태 Sprite입니다.
        [SerializeField]
        private Sprite mFightSelectedSprite;

        [SerializeField]
        private Sprite mBagSelectedSprite;

        [SerializeField]
        private Sprite mPokemonSelectedSprite;

        [SerializeField]
        private Sprite mRunSelectedSprite;

        // 버튼 이미지와 일반 상태 Sprite를 저장합니다.
        private Image[] mButtonImages;
        private Sprite[] mNormalSprites;
        private Sprite[] mSelectedSprites;

        // 현재 선택된 버튼의 인덱스입니다.
        // 0: 싸운다, 1: 가방, 2: 포켓몬, 3: 도망
        private int mSelectedIndex = 0;

        protected internal override void OnOpen(object userData)
        {
            // 부모 UIFormLogic의 OnOpen을 먼저 호출합니다.
            base.OnOpen(userData);

            Log.Info("BattleBottomUI : Open");

            // 현재 BattleBottomUI의 RectTransform을 가져옵니다.
            RectTransform rectTransform = transform as RectTransform;

            // RectTransform을 찾지 못했다면 UI의 위치와 크기를 설정할 수 없으므로 종료합니다.
            if (rectTransform == null)
            {
                Log.Error("BattleBottomUI : RectTransform을 찾을 수 없습니다.");

                return;
            }

            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);

            // 이렇게 하면 Pos Y = 0일 때 UI가 화면 가장 아래쪽에 정확히 붙습니다.
            rectTransform.pivot     = new Vector2(0.5f, 1f);

            // UI Group의 가장 아래쪽에 붙입니다.
            rectTransform.anchoredPosition  = new Vector2(0f, -810f);

            // BattleBottomUI의 크기를 960x180으로 고정합니다.
            rectTransform.sizeDelta         = new Vector2(mWidth, mHeight);

            // Prefab이 저장되어 있던 Scale 값과 관계없이 기본 Scale로 맞춥니다.
            rectTransform.localScale        = Vector3.one;
            
            // 버튼 이미지가 4개 있는지 확인합니다.
            if (transform.childCount < 4)
            {
                Log.Error("BattleBottomUI : 버튼 이미지가 부족합니다.");

                return;
            }

            // 각 버튼의 크기입니다.
            float mButtonWidth   = 380f;
            float mButtonHeight  = 135f;

            Vector2[] position =
            {
                new Vector2(180f, 66f),     // 싸운다.
                new Vector2(540f, 66f),     // 가방
                new Vector2(180f, -66f),    // 포켓몬
                new Vector2(540f, -66f)     // 도망
            };

            // 버튼 Image 및 Sprite 배열을 초기화합니다.
            mButtonImages   = new Image[4];
            mNormalSprites  = new Sprite[4];

            mSelectedSprites = new Sprite[]
            {
                mFightSelectedSprite,
                mBagSelectedSprite,
                mPokemonSelectedSprite,
                mRunSelectedSprite
            };

            // 4개 버튼의 RectTransform을 자동 설정합니다.
            for (int child = 0; child < position.Length; child++)
            {
                RectTransform buttonRect =
                    transform.GetChild(child) as RectTransform;

                if (buttonRect == null)
                {
                    continue;
                }

                buttonRect.anchorMin    = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax    = new Vector2(0.5f, 0.5f);
                buttonRect.pivot        = new Vector2(0.5f, 0.5f);

                buttonRect.anchoredPosition = position[child];
                buttonRect.sizeDelta        = new Vector2(mButtonWidth, mButtonHeight);
                buttonRect.localScale = Vector3.one;

                // 각 버튼의 Image를 가져옵니다.
                Image buttonImage = buttonRect.GetComponent<Image>();

                if (buttonImage == null)
                {
                    Log.Error($"BattleBottomUI : Image가 없습니다. Index = {child}");

                    return;
                }

                // 일반 상태 이미지를 보관합니다.
                mButtonImages[child]    = buttonImage;
                mNormalSprites[child]   = buttonImage.sprite;
            }

            // 처음에는 싸운다 버튼을 선택합니다.
            mSelectedIndex = 0;

            UpdateSelectedButton();

            Log.Info("ButtonBottomUI : 버튼 초기화 완료");
        }

        // 방향키로 현재 선택된 버튼을 변경합니다.
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (mButtonImages == null)
            {
                return;
            }

            int nextIndex = mSelectedIndex;

            // 왼쪽 버튼 선택
            if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
            {
                if (nextIndex % 2 == 1)
                {
                    nextIndex--;
                }
            }
            // 오른쪽 버튼 선택
            else if (Input.GetKeyDown(KeyCode.RightArrow) == true)
            {
                if (nextIndex % 2 == 0)
                {
                    nextIndex++;
                }
            }
            // 위쪽 버튼 선택
            else if (Input.GetKeyDown(KeyCode.UpArrow) == true)
            {
                if (nextIndex >= 2)
                {
                    nextIndex -= 2;
                }
            }
            // 아래쪽 버튼 선택
            else if (Input.GetKeyDown(KeyCode.DownArrow) == true)
            {
                if (nextIndex < 2)
                {
                    nextIndex += 2;
                }
            }

            // 선택된 버튼에 변화가 없다면 종료합니다.
            if (nextIndex == mSelectedIndex)
            {
                return;
            }

            mSelectedIndex = nextIndex;

            // 선택 이미지를 갱신합니다.
            UpdateSelectedButton();
        }

        // 선택된 버튼만 어두운 Sprite로 변경합니다.
        private void UpdateSelectedButton()
        {
            if (mButtonImages == null || 
                mNormalSprites == null ||
                mSelectedSprites == null)
            {
                return;
            }

            for (int index = 0; index < mButtonImages.Length; index++)
            {
                Image buttonImage = mButtonImages[index];

                if (buttonImage == null)
                {
                    continue;
                }

                if (index == mSelectedIndex)
                {
                    // 선택된 버튼은 어두운 이미지를 사용합니다.
                    if (mSelectedSprites[index] != null)
                    {
                        buttonImage.sprite = mSelectedSprites[index];
                    }
                }
                else
                {
                    // 나머지 버튼은 일반 이미지로 되돌립니다.
                    buttonImage.sprite = mNormalSprites[index];
                }
            }

            Log.Info($"BattleBottomUI : Selected Index = {mSelectedIndex}");
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            // BattleBottomUI가 닫혔음을 출력합니다.
            Log.Info("BattleBottomUI : Close");

            // 배틀 UI가 닫힐 때
            // 모든 버튼을 일반 상태로 이미지로 복원합니다.
            if (mButtonImages != null &&
                mNormalSprites != null)
            {
                for (int index = 0; index < mButtonImages.Length; index++)
                {
                    if (mButtonImages[index] == null)
                    {
                        continue;
                    }

                    mButtonImages[index].sprite = mNormalSprites[index];
                }
            }

            mButtonImages       = null;
            mNormalSprites      = null;
            mSelectedSprites    = null;

            mSelectedIndex = 0;

            // 부모 UIFormLogic의 OnClose를 호출합니다.
            base.OnClose(isShutdown, userData);
        }
    }
}