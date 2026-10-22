using System;
using UnityEngine;
using UnityEngine.UI;
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

        [SerializeField] private Sprite mFightNormalSprite;
        [SerializeField] private Sprite mBagNormalSprite;
        [SerializeField] private Sprite mPokemonNormalSprite;
        [SerializeField] private Sprite mRunNormalSprite;

        [SerializeField] private Sprite mFightSelectedSprite;
        [SerializeField] private Sprite mBagSelectedSprite;
        [SerializeField] private Sprite mPokemonSelectedSprite;
        [SerializeField] private Sprite mRunSelectedSprite;

        // 버튼 상태를 BattleUI에서 직접 관리합니다.
        private Image[] mButtonImages;
        private Sprite[] mNormalSprites;
        private Sprite[] mSelectedSprites;
        private int mSelectedIndex = 0;

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
                RectTransform parentRect    = transform.parent as RectTransform;

                if (parentRect != null)
                {
                    float centerX           = (0.5f - parentRect.pivot.x) * parentRect.rect.width;
                    float topY              = (1f - parentRect.pivot.y) * parentRect.rect.height;
                    transform.localPosition = new Vector3(centerX, topY, 0f);
                }
                else
                {
                    Log.Warning("BattleUI : 부모 RectTransform을 찾지 못했습니다.");
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
            //SetStatusVisible(mEnemyStatus2,     false);
            SetStatusVisible(mPlayerStatus1,    true);
            //SetStatusVisible(mPlayerStatus2,    false);

            // Inspector 연결 없이 부모 Root에서 2번 상태창을 찾습니다.
            Transform enemyStatus2  = mEnemyStatus2;
            Transform playerStatus2 = mPlayerStatus2;

            if (enemyStatus2 == null && mEnemyStatusRoot != null)
            {
                enemyStatus2 = mEnemyStatusRoot.Find("EnemyStatus2");
            }

            if (playerStatus2 == null && mPlayerStatusRoot != null)
            {
                playerStatus2 = mPlayerStatusRoot.Find("PlayerStatus2");
            }

            // 싱글 배틀에서는 2번 상태창을 비활성화합니다.
            SetStatusVisible(enemyStatus2,  false);
            SetStatusVisible(playerStatus2, false);

            // 하단 버튼의 기본 Sprite와 선택 Sprite를 초기화합니다.
            InitializeButtons();

            Log.Info("BattleUI : Single Battle UI 활성화 완료");
        }

        // BattleBottomUI의 방향키 입력 처리를 통합합니다.
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            if (mButtonImages == null)
            {
                return;
            }

            int nextIndex = mSelectedIndex;

            if (Input.GetKeyDown(KeyCode.LeftArrow) == true)
            {
                if (nextIndex % 2 == 1)
                {
                    nextIndex--;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) == true)
            {
                if (nextIndex % 2 ==0)
                {
                    nextIndex++;
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) == true)
            {
                if (nextIndex >= 2)
                {
                    nextIndex -= 2;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) == true)
            {
                if (nextIndex < 2)
                {
                    nextIndex += 2;
                }
            }

            if (nextIndex == mSelectedIndex)
            {
                return;
            }

            // 변경 전 버튼의 인덱스를 저장합니다.
            int previousIndex = mSelectedIndex;
            mSelectedIndex = nextIndex;

            // 이전 버튼은 지정된 일반 Sprite로 복원합니다.
            SetButtonSprite(previousIndex, false);

            // 새로 선택한 버튼만 선택 이미지로 변경합니다.
            SetButtonSprite(mSelectedIndex, true);

            Log.Info($"BattleUI : Selected Index = {mSelectedIndex}");
        }

        // BottomUI 자식 네 개를 버튼 순서대로 연결합니다.
        private void InitializeButtons()
        {
            mButtonImages = null;
            mNormalSprites = null;
            mSelectedSprites = null;

            if (mBottomRoot == null || mBottomRoot.childCount < 4)
            {
                Log.Error("BattleUI : BottomUI 또는 버튼 4개를 찾을 수 없습니다.");

                return;
            }

            float buttonWidth   = 340f;
            float buttonHeight  = 120f;

            Vector2[] positions =
            {
                new Vector2(170f, 65f),
                new Vector2(530f, 65f),
                new Vector2(170f, -65f),
                new Vector2(530f, -65f)
            };

            Image[] buttonImages = new Image[4];

            string[] buttonNames =
            {
                "FightButton",
                "BagButton",
                "PokemonButton",
                "RunButton"
            };

            Sprite[] normalSprites =
            {
                mFightNormalSprite,
                mBagNormalSprite,
                mPokemonNormalSprite,
                mRunNormalSprite
            };


            Sprite[] selectedSprites =
            {
                mFightSelectedSprite,
                mBagSelectedSprite,
                mPokemonSelectedSprite,
                mRunSelectedSprite
            };

            // 일반 Sprite가 연결됐는지 먼저 확인합니다.
            for (int Index = 0; Index < positions.Length; Index++)
            {
                Transform buttonTransform = mBottomRoot.Find(buttonNames[Index]);

                RectTransform buttonRect = buttonTransform as RectTransform;

                if (buttonRect == null)
                {
                    Log.Error($"BattleUI : {buttonNames[Index]}을 찾을 수 없습니다.");

                    return;
                }

                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                buttonRect.pivot     = new Vector2(0.5f, 0.5f);

                buttonRect.anchoredPosition = positions[Index];
                buttonRect.sizeDelta        = new Vector2(buttonWidth, buttonHeight);

                buttonRect.localScale = Vector3.one;

                Image buttonImage = buttonRect.GetComponent<Image>();

                if (buttonImage == null)
                {
                    Log.Error($"BattleUI : {buttonNames[Index]}에 Image가 없습니다.");

                    return;
                }

                Button button = buttonRect.GetComponent<Button>();

                if (button != null)
                {
                    button.transition = Selectable.Transition.None;
                }

                if (button != null)
                {
                    button.transition = Selectable.Transition.None;
                }

                buttonImage.color   = Color.white;
                buttonImages[Index] = buttonImage;

                if (normalSprites[Index] == null)
                {
                    normalSprites[Index] = buttonImage.sprite;

                    Log.Warning($"BattleUI:{buttonNames[Index]} 일반 Sprite 미연결." +
                        $"현재 Source Image를 사용합니다.");
                }

                // 복원할 기본 이미지조차 없다면 오류를 출력합니다.
                if (normalSprites[Index] == null)
                {
                    Log.Error($"BattleUI : {buttonNames[Index]}에 Sprite가 없습니다.");

                    return;
                }

                if (selectedSprites[Index] == null)
                {
                    Log.Warning($"BattleUI : {buttonNames[Index]} 선택 Sprite 미연결.");
                }
            }

            mButtonImages       = buttonImages;
            mNormalSprites      = normalSprites;
            mSelectedSprites    = selectedSprites;

            mSelectedIndex    = 0;

            for (int index = 0; index < mButtonImages.Length; index++)
            {
                SetButtonSprite(index, index == mSelectedIndex);
            }

            Log.Info("BattleUI : 버튼 초기화 완료");
        }

        // 지정한 버튼 하나의 Sprite만 변경합니다.
        private void SetButtonSprite(int index, bool isSelected)
        {
            if (mButtonImages == null || 
                mNormalSprites == null || 
                mSelectedSprites == null)
            {
                return;
            }

            if (index < 0 || index >= mButtonImages.Length)
            {
                return;
            }

            Image buttonImage = mButtonImages[index];

            if (buttonImage == null)
            {
                return;
            }

            Sprite targetSprite = mNormalSprites[index];

            if (isSelected == true &&
                mSelectedSprites[index] != null)
            {
                targetSprite = mSelectedSprites[index];
            }

            // Sprite가 실제로 다를 때만 교체합니다.
            if (buttonImage.sprite != targetSprite)
            {
                buttonImage.sprite = targetSprite;
            }

            // Sprite를 교체할 때 항상 원래 색상을 유지합니다.
            buttonImage.color = Color.white;
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
                rect.pivot      = pivot;
                rect.sizeDelta  = size;

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

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            // BattleUI가 닫힐 때 버튼 Sprite를 기본 상태로 되돌립니다.
            if (mButtonImages != null && mNormalSprites != null)
            {
                for (int index = 0; index < mButtonImages.Length; index++)
                {
                    Image buttonImages = mButtonImages[index];

                    if (mButtonImages[index] != null)
                    {
                        buttonImages.sprite = mNormalSprites[index];
                        buttonImages.color  = Color.white;
                    }
                }
            }

            mButtonImages       = null;
            mNormalSprites      = null;
            mSelectedSprites    = null;

            mSelectedIndex      = 0;

            Log.Info("BattleUI : Close");

            base.OnClose(isShutdown, userData);
        }

        // HP 게이지 Image 연결 전이므로 기존 빈 함수를 유지합니다.
        public void SetHP(int enemyCurrentHP, int enemyMaxHP, int playerCurrentHP, int playerMaxHP)
        {
        }
    }
}
