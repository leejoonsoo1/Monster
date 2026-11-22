using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class BattleUI : UIFormLogic
    {
        private const float mWidth = 1440f;
        private const float mHeight = 1080f;

        // C키를 누르지 않았을 때 메시지를 표시하는 시간입니다.
        private const float mMessageDuration = 5f;

        [SerializeField] private Transform mEnemyStatusRoot;
        [SerializeField] private Transform mPlayerStatusRoot;
        [SerializeField] private Transform mBottomRoot;
        [SerializeField] private Transform mFightMenu;

        // 기본 전투 명령 안내 라벨
        [SerializeField] private TMP_Text mCommandLabel;

        [SerializeField] private Sprite[] mFightCursorSprites;

        // 기술 버튼의 이미지와 라벨을 저장합니다.
        private readonly Image[] mSkillImages = new Image[4];
        private readonly TMP_Text[] mSkillLabels = new TMP_Text[4];
        private readonly Dictionary<string, Sprite> mFightCursorByName = new Dictionary<string, Sprite>();
        private IReadOnlyList<PokemonSkillInfo> mFightSkills;
        private int mSelectedSkillIndex = -1;

        // 전투 메세지 배경 및 텍스트
        [SerializeField] private Transform  mMessagePanel;
        [SerializeField] private TMP_Text   mMessageLabel;

        [SerializeField] private RectTransform mEnemyStatus1;
        [SerializeField] private RectTransform mEnemyStatus2;
        [SerializeField] private RectTransform mPlayerStatus1;
        [SerializeField] private RectTransform mPlayerStatus2;

        // 싱글 배틀에 참가하는 상대와 아군 포켓몬의 상태창입니다.
        [SerializeField] private PokemonStatusView mEnemyStatusView = new PokemonStatusView();
        [SerializeField] private PokemonStatusView mPlayerStatusView = new PokemonStatusView();

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

        // 메시지 출력 상태
        private bool mIsIntroPlaying        = false;
        private int mIntroMessageIndex      = 0;
        private float mIntroElapsedSeconds  = 0f;
        private string mIntroPlayerName;

        // 도망 메시지 출력 상태
        private bool mIsEscapePlaying = false;

        // 도망 메시지 경과 시간
        private float mEscapeElapsedSeconds = 0f;

        // 도망 메시지가 끝날 때까지 배틀 진행을 기다리게 합니다.
        internal bool IsEscapePlaying => mIsEscapePlaying;

        private MyProcedureBattle mBattleProcedure;

        private int mSelectedIndex      = 0;
        private bool mIsFightMenuOpen   = false;

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            Log.Info("BattleUI : Open");

            RectTransform rootRect = transform as RectTransform;

            if (rootRect != null)
            {
                // 백그라운드와 같은 너비와 상단 중앙 기준을 사용하여 좌우 끝을 맞춥니다.
                rootRect.anchorMin          = new Vector2(0.5f, 1f);
                rootRect.anchorMax          = new Vector2(0.5f, 1f);
                rootRect.pivot              = new Vector2(0.5f, 1f);
                rootRect.anchoredPosition   = Vector2.zero;
                rootRect.sizeDelta          = new Vector2(mWidth, mHeight);
                rootRect.localScale         = Vector3.one;
            }
            else
            {
                RectTransform parentRect = transform.parent as RectTransform;

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

            SetUIRect(mEnemyStatusRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -80f), new Vector2(600f, 160f), new Vector2(-mWidth * 0.5f, -80f));
            SetUIRect(mPlayerStatusRoot, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 300f), new Vector2(600f, 180f), new Vector2(mWidth * 0.5f, -mHeight + 300f));
            SetUIRect(mBottomRoot, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -810f), new Vector2(1440f, 270f), new Vector2(0f, -810f));

            // FightMenu SetUIRect() 호출합니다.
            if (mFightMenu == null)
            {
                mFightMenu = transform.Find("FIghtMenu");
                if (mFightMenu == null)
                {
                    mFightMenu = transform.Find("FightMenu");
                }
            }

            SetUIRect(mFightMenu, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -810f), new Vector2(1440f, 270f), new Vector2(0f, -810f));
            SetFightMenuVisible(false);

            // 기술 버튼과 이름 라벨 위치 설정
            InitializeSkillButtons();

            // 상대 체력바의 왼쪽 끝을 백그라운드의 왼쪽 가장자리에 붙입니다.
            SetUIRect(mEnemyStatus1, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(400f, 120f), new Vector2(0f, -80f));

            // 아군 체력바는 25% 크게 표시하고 오른쪽 끝을 백그라운드의 오른쪽 가장자리에 붙입니다.
            SetUIRect(mPlayerStatus1, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(500f, 150f), new Vector2(0f, 90f));

            SetUIVisible(mEnemyStatus1, true);
            //SetStatusVisible(mEnemyStatus2,     false);
            SetUIVisible(mPlayerStatus1, true);
            //SetStatusVisible(mPlayerStatus2,    false);

            // Inspector 연결 없이 부모 Root에서 2번 상태창을 찾습니다.
            Transform enemyStatus2 = mEnemyStatus2;
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
            SetUIVisible(enemyStatus2, false);
            SetUIVisible(playerStatus2, false);


            // 하단 버튼의 기본 Sprite와 선택 Sprite를 초기화합니다.
            InitializeButtons();

            // 기본 전투 명령 안내 라벨 초기화
            InitializeCommandLabel();

            // BattleProcedure를 가져옵니다.
            mBattleProcedure = GameEntry.GetComponent<ProcedureComponent>().CurrentProcedure as MyProcedureBattle;

            if (mBattleProcedure == null)
            {
                Log.Error("BattleUI : ProcedureComponent가 없습니다.");
            }

            // 전투 시작 안내를 먼저 출력합니다.
            BeginBattleIntro(userData);

            Log.Info("BattleUI : Single Battle UI 활성화 완료");
        }

        // BattleBottomUI의 방향키 입력 처리를 통합합니다.
        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            // 전투 시작 메시지가 우선입니다.
            if (mIsIntroPlaying == true)
            {
                UpdateBattleIntro(realElapseSeconds);

                return;
            }

            // 도망 메시지를 표시하는 동안에는 명령 메뉴 입력을 처리하지 않습니다.
            if (mIsEscapePlaying)
            {
                UpdateBattleEscape(realElapseSeconds);

                return;
            }

            if (mButtonImages == null)
            {
                return;
            }

            // FightMenu가 열려있으면 방향키 입력을 무시합니다.
            if (mIsFightMenuOpen == true)
            {
                UpdateFightMenu();

                // 기존 CommandMenu의 입별 처리는 실행하지 않습니다.
                return;
            }

            // FightMenu가 닫혀 있으면 기존 명령 메뉴를 처리합니다.
            UpdateCommandMenu();
        }

        // 무엇을 할까? 라벨의 위치와 내용을 설정합니다.
        private void InitializeCommandLabel()
        {
            if (mBottomRoot == null)
            {
                Log.Warning("BattleUI : BottomUI가 없습니다.");

                return;
            }

            if (mCommandLabel == null)
            {
                Transform labelTransform = mBottomRoot.Find("CommandLabel");

                if (labelTransform != null)
                {
                    mCommandLabel = labelTransform.GetComponent<TMP_Text>();
                }
            }

            if (mCommandLabel == null)
            {
                Log.Warning("BattleUI : BottomUI/CommandLabel을 찾지 못했습니다.");

                return;
            }

            SetUIRect(mCommandLabel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-470f, 0f), new Vector2(420f, 150f), new Vector2(-470f, 0f));

            mCommandLabel.alignment = TextAlignmentOptions.MidlineLeft;

            mCommandLabel.text = "무엇을 할까?";
        }

        // 기술 버튼 4개와 라벨 위치를 초기화합니다.
        private void InitializeSkillButtons()
        {
            if (mFightMenu == null)
            {
                Log.Error("BattleUI : FightMenu가 없습니다.");

                return;
            }

            // 기술 버튼의 배치 좌표입니다.
            Vector2[] positions =
            {
                new Vector2(-445f, 65f),
                new Vector2(75f, 65f),
                new Vector2(-445f, -65f),
                new Vector2(75f, -65f)
            };

            // 기술 버튼의 크기입니다.
            Vector2 buttonSize = new Vector2(490f, 110f);

            // 기술 이름 라벨의 크기입니다.
            Vector2 labelSize = new Vector2(450f, 90f);

            // 타입 테이블이 반환하는 이름으로 스프라이트를 찾을 수 있게 합니다.
            mFightCursorByName.Clear();
            if (mFightCursorSprites != null)
            {
                foreach (Sprite sprite in mFightCursorSprites)
                {
                    if (sprite != null)
                    {
                        mFightCursorByName[sprite.name] = sprite;
                    }
                }
            }

            for (int index = 0; index < positions.Length; index++)
            {
                mSkillImages[index] = null;
                mSkillLabels[index] = null;

                // SkillButton0~3을 이름으로 검색합니다.
                Transform skillButton = mFightMenu.Find($"SkillButton{index}");

                if (skillButton == null)
                {
                    Log.Warning($"BattleUI : SkillButton{index}가 없습니다.");

                    continue;
                }

                // 기술 버튼 위치 설정
                SetUIRect(skillButton, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), positions[index], buttonSize, positions[index]);

                // 기술 버튼의 Image 컴포넌트를 확인합니다.
                Image skillImage = skillButton.GetComponent<Image>();
                mSkillImages[index] = skillImage;

                if (skillImage == null)
                {
                    Log.Warning($"BattleUI : SkillButton{index}에 Image가 없습니다.");
                }
                else
                {
                    // 원본 Sprite 색상을 유지합니다.
                    skillImage.color = Color.white;

                }

                // 자식 SkillName 라벨 검색
                Transform skillName = skillButton.Find("SkillName");

                if (skillName == null)
                {
                    Log.Warning($"BattleUI : SkillButton{index}/SkillName이 없습니다.");

                    continue;
                }

                // 기술 이름 라벨 위치 설정
                SetUIRect(skillName, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, labelSize, Vector2.zero);

                TMP_Text skillText = skillName.GetComponent<TMP_Text>();

                if (skillText == null)
                {
                    Log.Warning($"BattleUI : SkillButton{index}/SkillName에 TMP_Text가 없습니다.");

                    continue;
                }

                // 텍스트 중앙 정렬 및 크기 설정
                skillText.alignment = TextAlignmentOptions.Center;

                skillText.enableAutoSizing  = true;
                skillText.fontSizeMin       = 28f;
                skillText.fontSizeMax       = 48f;

                skillText.color = Color.black;

                mSkillLabels[index] = skillText;
            }
        }

        // 비어 있는 칸을 건너뛰고 첫 번째 기술을 선택합니다.
        private void ResetFightSelection()
        {
            mSelectedSkillIndex = -1;
            for (int index = 0; index < mSkillImages.Length; index++)
            {
                if (GetFightSkill(index) != null)
                {
                    mSelectedSkillIndex = index;
                    break;
                }
            }

            RefreshSkillButtons();
        }

        private PokemonSkillInfo GetFightSkill(int index)
        {
            if (mFightSkills == null || index < 0 || index >= mSkillImages.Length || index >= mFightSkills.Count)
            {
                return null;
            }

            return mFightSkills[index];
        }

        // 기술 이름과 해당 타입의 선택 상태에 맞는 버튼 이미지를 적용합니다.
        private void RefreshSkillButtons()
        {
            for (int index = 0; index < mSkillImages.Length; index++)
            {
                Image image = mSkillImages[index];
                if (image == null)
                {
                    continue;
                }

                PokemonSkillInfo skill = GetFightSkill(index);
                SetUIVisible(image.transform, skill != null);
                if (skill == null)
                {
                    continue;
                }

                bool isSelected = index == mSelectedSkillIndex;
                string spriteName = PokemonTypeCursorTable.GetSpriteName(skill.Type, isSelected);
                Sprite sprite;
                if (spriteName == null || !mFightCursorByName.TryGetValue(spriteName, out sprite))
                {
                    // 타입 이미지가 없는 경우에는 일반 타입 이미지를 사용합니다.
                    mFightCursorByName.TryGetValue(PokemonTypeCursorTable.GetSpriteName("Normal", isSelected), out sprite);
                }

                image.sprite = sprite;
                if (mSkillLabels[index] != null)
                {
                    mSkillLabels[index].text = skill.Name;
                }
            }
        }

        // 포켓몬 정보를 가져와 메시지를 시작합니다.
        private void BeginBattleIntro(object userData)
        {
            mIsIntroPlaying = false;
            mIntroMessageIndex = 0;
            mIntroElapsedSeconds = 0f;
            mIntroPlayerName = null;

            // Inspector 연결이 없으면 오브젝트 이름으로 검색합니다.
            if (mMessagePanel == null)
            {
                mMessagePanel = transform.Find("overlay_message");
            }

            if (mMessagePanel != null &&
                mMessageLabel == null)
            {
                mMessageLabel = mMessagePanel.GetComponentInChildren<TMP_Text>(true);
            }

            if (mMessagePanel == null || mMessageLabel == null)
            {
                Log.Error("BattleUI : overlay_message 또는 MessageLabel이 없습니다.");

                return;
            }

            SetUIVisible(mMessagePanel, false);

            // MyProcedureBattle에서 전달한 적 포켓몬 번호
            if ((userData is int) != true)
            {
                Log.Error("BattleUI : 적 포켓몬 ID가 전달되지 않았습니다.");

                return;
            }

            int enemyPokemonId = (int)userData;

            RPGGame game = RPGGame.Instance;

            if (game == null || game.PokemonDatabase == null || game.PokemonPartyManager == null || game.PokemonPartyManager.Count == 0)
            {
                Log.Error("BattleUI : 포켓몬 데이터베이스 또는 파티 정보가 없습니다.");

                return;
            }

            // 적 포켓몬의 도감 번호로 정보를 조회합니다.
            PokemonInfo enemyPokemon = game.PokemonDatabase.GetPokemonInfo(enemyPokemonId);

            // 현재 파티 첫 번째 포켓몬을 가져옵니다.
            PokemonData playerPokemon = game.PokemonPartyManager.GetPokemon(0);

            if (enemyPokemon == null || playerPokemon == null)
            {
                Log.Error("BattleUI : 출전 포켓몬 정보를 찾지 못했습니다.");

                return;
            }

            // 아군 포켓몬 이름을 저장합니다.
            mIntroPlayerName = playerPokemon.Name;

            mIntroMessageIndex = 0;
            mIntroElapsedSeconds = 0f;
            mIsIntroPlaying = true;

            // 포켓몬 이름의 마지막 한글 글자에 받침이 있으면 '이'를 사용합니다.
            string enemyName = enemyPokemon.Name;
            string subjectParticle = "가";
            if (!string.IsNullOrEmpty(enemyName))
            {
                char lastCharacter = enemyName[enemyName.Length - 1];
                if (lastCharacter >= '가' && lastCharacter <= '힣' && (lastCharacter - '가') % 28 != 0)
                {
                    subjectParticle = "이";
                }
            }

            // 첫 번째 메시지를 출력합니다.
            ShowBattleMessage($"야생의 {enemyName}{subjectParticle} 출현하였다!");

            Log.Info($"BattleUI : {mMessageLabel.text}");
        }

        // C키 또는 5초 경과 시 다음 메시지로 이동합니다.
        private void UpdateBattleIntro(float realElapseSeconds)
        {
            if (mIsIntroPlaying != true)
            {
                return;
            }

            mIntroElapsedSeconds += realElapseSeconds;

            bool isConfirmPressed = Input.GetKeyDown(KeyCode.C);

            // C키 입력도 없고 5초도 지나지 않았으면 유지합니다.
            if (isConfirmPressed != true && mIntroElapsedSeconds < mMessageDuration)
            {
                return;
            }

            mIntroElapsedSeconds = 0f;
            mIntroMessageIndex++;

            // 두 번째 메시지
            if (mIntroMessageIndex == 1)
            {
                mMessageLabel.text = $"가랏! {mIntroPlayerName}!";

                Log.Info($"BattleUI : 가랏! {mIntroPlayerName}!");

                return;
            }

            CompleteBattleIntro();
        }

        // 메시지를 닫고 기존 전투 메뉴로 전환합니다.
        private void CompleteBattleIntro()
        {
            mIsIntroPlaying         = false;
            mIntroMessageIndex      = 0;
            mIntroElapsedSeconds    = 0f;
            mIntroPlayerName        = null;

            // 메시지 UI 숨기기
            SetUIVisible(mMessagePanel, false);

            // FightMenu를 숨기고 기존 BottomUI 표시
            SetFightMenuVisible(false);

            Log.Info("BattleUI : 전투 시작 메시지 완료");
        }

        // <summary>
        // 명령 메뉴를 숨기고 기존 메시지 창에 대사를 표시합니다.
        // </summary>
        private void ShowBattleMessage(string message)
        {
            SetFightMenuVisible(false);
            SetUIVisible(mBottomRoot, false);
            mMessageLabel.text = message;
            SetUIVisible(mMessagePanel, true);
        }

        // <summary>
        // 도망 명령을 확인하면 성공 메시지를 먼저 표시합니다.
        // </summary>
        private void BeginBattleEscape()
        {
            if (mBattleProcedure == null)
            {
                Log.Warning("BattleUI : MyProcedureBattle을 찾을 수 없습니다.");

                return;
            }

            // 메시지 창이 없더라도 도망 명령은 처리합니다.
            if (mMessagePanel == null || mMessageLabel == null)
            {
                Log.Warning("BattleUI : 도망 메시지 창을 찾을 수 없습니다.");
                mBattleProcedure.FinishBattle();

                return;
            }

            mEscapeElapsedSeconds = 0f;
            mIsEscapePlaying = true;
            ShowBattleMessage("무사히 도망쳤다!");
        }

        // <summary>
        // C키를 다시 누르거나 5초가 지나면 전투를 종료합니다.
        // </summary>
        private void UpdateBattleEscape(float realElapseSeconds)
        {
            mEscapeElapsedSeconds += realElapseSeconds;

            if (!Input.GetKeyDown(KeyCode.C) && mEscapeElapsedSeconds < mMessageDuration)
            {
                return;
            }

            // 배틀 화면이 닫히기 전까지 메시지 창을 표시합니다.
            mBattleProcedure.FinishBattle();
        }

        // FightMenu의 입력을 처리합니다.
        private void UpdateFightMenu()
        {
            // X키를 누르면 기술 선택 메뉴를 딛고 기본 명령 메뉴로 돌아갑니다.
            if (Input.GetKeyDown(KeyCode.X) == true)
            {
                SetFightMenuVisible(false);

                return;
            }

            if (mSelectedSkillIndex < 0)
            {
                return;
            }

            // 방향키로 이동할 때 같은 행 또는 같은 열의 기술만 선택합니다.
            int nextIndex = mSelectedSkillIndex;
            if (Input.GetKeyDown(KeyCode.LeftArrow) && nextIndex % 2 == 1)
            {
                nextIndex--;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && nextIndex % 2 == 0)
            {
                nextIndex++;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                nextIndex -= 2;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                nextIndex += 2;
            }

            if (nextIndex != mSelectedSkillIndex && GetFightSkill(nextIndex) != null)
            {
                mSelectedSkillIndex = nextIndex;
                RefreshSkillButtons();
            }
        }

        // 싸운다, 가방, 포켓몬, 도망의 입력을 처리합니다.
        private void UpdateCommandMenu()
        {
            // 기본 메뉴에서는 X키를 눌러도 별도 동작을 하지 않습니다.
            if (Input.GetKeyDown(KeyCode.X) == true)
            {
                return;
            }

            // C키로 현재 선택된 명령을 실행합니다.
            if (Input.GetKeyDown(KeyCode.C) == true)
            {
                // 싸운다 버튼 선택 시 기술 메뉴를 표시합니다.
                if (mSelectedIndex == 0)
                {
                    SetFightMenuVisible(true);
                }
                else if (mSelectedIndex == 1)
                {
                    ;
                }
                else if (mSelectedIndex == 2)
                {
                    ;
                }
                else if (mSelectedIndex == 3)
                {
                    // 도망 버튼을 선택하면 종료 전에 성공 메시지를 표시합니다.
                    BeginBattleEscape();
                }

                return;
            }

            // 방향키 입련에 따라 다음 선택 인덱스를 계산합니다.
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
                if (nextIndex % 2 == 0)
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

            // 선택된 버튼이 변경되지 않았다면 종료합니다.
            if (nextIndex == mSelectedIndex)
            {
                return;
            }

            // 변경 전 버튼의 인덱스를 저장합니다.
            int previousIndex = mSelectedIndex;

            // 새로운 버튼의 인덱스를 저장합니다.
            mSelectedIndex = nextIndex;

            // 이전 버튼을 일반 Sprite로 복원합니다.
            SetButtonSprite(previousIndex, false);

            // 새로운 버튼을 선택 Sprite로 변경합니다.
            SetButtonSprite(mSelectedIndex, true);

            Log.Info($"BattleUI : Selected Index = {mSelectedIndex}");
        }

        private void SetFightMenuVisible(bool isVisible)
        {
            // FightMenu를 열려고 하지만 오브젝트가 없다면 중단합니다.
            if (isVisible == true && mFightMenu == null)
            {
                Log.Warning("BattleUI : FightMenu를 찾을 수 없습니다.");

                return;
            }

            // 현재 FightMenu의 활성화 상태를 저장합니다.
            mIsFightMenuOpen = isVisible;

            if (isVisible)
            {
                ResetFightSelection();
            }

            // FightMenu의 활성화 상태를 변경합니다.
            SetUIVisible(mFightMenu, isVisible);

            // FightMenu가 표시되면 기존 BottomUI는 숨깁니다.
            SetUIVisible(mBottomRoot, isVisible == false);
        }

        // BottomUI 자식 네 개를 버튼 순서대로 연결합니다.
        private void InitializeButtons()
        {
            mButtonImages = null;
            mNormalSprites = null;
            mSelectedSprites = null;

            // BottomUI가 연결되지 않았거나 자식 버튼이 4개 미만이면 초기화를 중단합니다.
            if (mBottomRoot == null || mBottomRoot.childCount < 4)
            {
                Log.Error("BattleUI : BottomUI 또는 버튼 4개를 찾을 수 없습니다.");

                return;
            }

            float buttonWidth = 340f;
            float buttonHeight = 120f;

            Vector2[] positions =
            {
                new Vector2(170f, 65f),
                new Vector2(530f, 65f),
                new Vector2(170f, -65f),
                new Vector2(530f, -65f)
            };

            // 각 버튼의 Image 컴포넌트를 저장할 배열입니다.
            Image[] buttonImages = new Image[4];

            // Hierarchy에 등록된 버튼 GameObject의 이름입니다.
            // 버튼을 자식 인덱스가 아닌 이름으로 탐색할 때 사용합니다.
            string[] buttonNames =
            {
                "FightButton",
                "BagButton",
                "PokemonButton",
                "RunButton"
            };

            // 선택되지 않은 버튼에 사용할 일반 Sprite입니다.
            // Inspector에 연결한 Sprite를 배열에 저장합니다.
            Sprite[] normalSprites =
            {
                mFightNormalSprite,
                mBagNormalSprite,
                mPokemonNormalSprite,
                mRunNormalSprite
            };

            // 현재 선택된 버튼에 사용할 Sprite입니다.
            // Inspector에 연결한 선택 Sprite를 배열에 저장합니다.
            Sprite[] selectedSprites =
            {
                mFightSelectedSprite,
                mBagSelectedSprite,
                mPokemonSelectedSprite,
                mRunSelectedSprite
            };

            // 4개의 버튼을 순서대로 탐색하고 초기화합니다.
            for (int Index = 0; Index < positions.Length; Index++)
            {
                // BottomUI의 자식 중 이름이 일치하는 버튼을 탐색합니다.
                Transform buttonTransform = mBottomRoot.Find(buttonNames[Index]);

                // 버튼의 UI 위치와 크기를 제어하기 위해 RectTransform으로 변환합니다.
                RectTransform buttonRect = buttonTransform as RectTransform;

                // 버튼이 없거나 RectTransform을 가지고 있지 않으면 종료합니다.
                if (buttonRect == null)
                {
                    Log.Error($"BattleUI : {buttonNames[Index]}을 찾을 수 없습니다.");

                    return;
                }

                // 버튼의 Anchor를 부모 BottomUI의 중앙으로 고정합니다.
                buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                buttonRect.anchorMax = new Vector2(0.5f, 0.5f);

                // 버튼 자체의 기준점을 중앙으로 설정합니다.
                buttonRect.pivot = new Vector2(0.5f, 0.5f);

                buttonRect.anchoredPosition = positions[Index];
                buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

                buttonRect.localScale = Vector3.one;

                Image buttonImage = buttonRect.GetComponent<Image>();

                if (buttonImage == null)
                {
                    Log.Error($"BattleUI : {buttonNames[Index]}에 Image가 없습니다.");

                    return;
                }

                // 버튼에 Unity UI Button 컴포턴트가 있는지 확인합니다.
                Button button = buttonRect.GetComponent<Button>();

                if (button != null)
                {
                    // Unity Button의 기본 색상 및 Sprite 전환을 비활성화합니다.
                    // Sprite 변경은 BattleUI의 SetButtonSprite()에서 직접 처리합니다.
                    button.transition = Selectable.Transition.None;
                }

                // Image의 색상을 흰색으로 설정하여 Sprite 원본 색상을 유지합니다.
                buttonImage.color = Color.white;

                // 현재 버튼의 Image 컴포넌틀르 배열에 저장합니다.
                buttonImages[Index] = buttonImage;

                // Inpector에 일반 Sprite가 연결되지 않은 경우입니다.
                if (normalSprites[Index] == null)
                {
                    // 현재 Image 컴포넌트에 설정된 Source Image를 일반 Sprite로 사용합니다.
                    normalSprites[Index] = buttonImage.sprite;

                    // 일반 Sprite가 연결되지 않았음을 경고합니다.
                    Log.Warning($"BattleUI:{buttonNames[Index]} 일반 Sprite 미연결." +
                        $"현재 Source Image를 사용합니다.");
                }

                // 복원할 기본 이미지조차 없다면 오류를 출력합니다.
                if (normalSprites[Index] == null)
                {
                    // 선택 Sprite가 없더라도 일반 Sprite를 사용할 수 있으므로 경고만 출력합니다.
                    Log.Error($"BattleUI : {buttonNames[Index]}에 Sprite가 없습니다.");
                }

                // 선택 상태에 사용할 Sprite가 연결되어 있는지 확인합니다.
                if (selectedSprites[Index] == null)
                {
                    Log.Warning($"BattleUI : {buttonNames[Index]} 선택 Sprite 미연결.");
                }
            }

            // 초기화한 Image와 Sprite 배열을 멤버 변수에 저장합니다.
            // 이후 OnUpdate()와 SetButtonSprite()에서 사용합니다.
            mButtonImages = buttonImages;
            mNormalSprites = normalSprites;
            mSelectedSprites = selectedSprites;
            mSelectedIndex = 0;

            // 모든 버튼에 현재 선택 상태에 맞는 Sprite를 적용합니다.
            for (int index = 0; index < mButtonImages.Length; index++)
            {
                // 선택된 버튼은 선택 Sprite를 사용하고,
                // 선택되지 않은 버튼은 일반 Sprite를 사용합니다.
                SetButtonSprite(index, index == mSelectedIndex);
            }

            // 버튼 4개의 초기화가 정상적으로 완료되었음을 출력합니다.
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

            RectTransform rect = target as RectTransform;
            RectTransform parentRect = target.parent as RectTransform;

            if (rect != null)
            {
                rect.pivot = pivot;
                rect.sizeDelta = size;

                if (parentRect != null)
                {
                    rect.anchorMin = anchor;
                    rect.anchorMax = anchor;
                    rect.anchoredPosition = anchoredPosition;
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

        // UI를 활성화/비활성화합니다.
        private void SetUIVisible(Transform status, bool isVisible)
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
            // 메시지 상태 초기화
            mIsIntroPlaying         = false;
            mIntroMessageIndex      = 0;
            mIntroElapsedSeconds    = 0f;
            mIntroPlayerName        = null;
            mIsEscapePlaying        = false;
            mEscapeElapsedSeconds   = 0f;
            SetUIVisible(mMessagePanel, false);

            // 다음 전투에 이전 포켓몬의 이름과 능력치가 남지 않도록 합니다.
            SetPokemonStatus(null, null);

            // 다음 배틀에서 기본 메뉴부터 열리도록 상태를 초기화합니다.
            SetFightMenuVisible(false);

            // BattleUI가 닫힐 때 버튼 Sprite를 기본 상태로 되돌립니다.
            if (mButtonImages != null && mNormalSprites != null)
            {
                for (int index = 0; index < mButtonImages.Length; index++)
                {
                    Image buttonImages = mButtonImages[index];

                    if (mButtonImages[index] != null)
                    {
                        buttonImages.sprite = mNormalSprites[index];
                        buttonImages.color = Color.white;
                    }
                }
            }

            mButtonImages = null;
            mNormalSprites = null;
            mSelectedSprites = null;

            mSelectedIndex = 0;

            Log.Info("BattleUI : Close");

            base.OnClose(isShutdown, userData);
        }

        // <summary>
        // 전투에 참가한 포켓몬의 실제 데이터를 각 상태창에 표시합니다.
        // </summary>
        internal void SetPokemonStatus(PokemonData enemyPokemon, PokemonData playerPokemon)
        {
            mEnemyStatusView?.SetPokemon(enemyPokemon);
            mPlayerStatusView?.SetPokemon(playerPokemon);

            IReadOnlyList<PokemonSkillInfo> skills = playerPokemon?.Skills;
            if (mFightSkills != skills)
            {
                mFightSkills = skills;
                if (mIsFightMenuOpen)
                {
                    ResetFightSelection();
                }
            }
        }

        [System.Serializable]
        private class PokemonStatusView
        {
            [SerializeField] private TMP_Text mNameLabel;
            [SerializeField] private TMP_Text mHPLabel;
            [SerializeField] private TMP_Text mLevelLabel;
            [SerializeField] private TMP_Text mActionLabel;
            [SerializeField] private RectTransform mHPFill;

            private PokemonData mDisplayedPokemon;
            private int mDisplayedHP;
            private int mDisplayedMaxHP;
            private int mDisplayedLevel;
            private int mDisplayedAction;

            public void SetPokemon(PokemonData pokemon)
            {
                if (pokemon == null)
                {
                    if (mDisplayedPokemon != null)
                    {
                        mDisplayedPokemon = null;

                        SetLabel(mNameLabel,    string.Empty);
                        SetLabel(mHPLabel,      string.Empty);
                        SetLabel(mLevelLabel,   string.Empty);
                        SetHPFill(0);
                    }

                    return;
                }

                // 값이 같은 동안에는 텍스트와 게이지를 다시 그리지 않습니다.
                if (mDisplayedPokemon == pokemon && mDisplayedHP == pokemon.CurrentHP &&
                    mDisplayedMaxHP == pokemon.MaxHP && mDisplayedLevel == pokemon.Level)
                {
                    return;
                }

                mDisplayedPokemon   = pokemon;
                mDisplayedHP        = pokemon.CurrentHP;
                mDisplayedMaxHP     = pokemon.MaxHP;
                mDisplayedLevel     = pokemon.Level;

                SetLabel(mNameLabel,    pokemon.Name);
                SetLabel(mHPLabel,      $"{pokemon.CurrentHP}/{pokemon.MaxHP}");
                SetLabel(mLevelLabel,   $"Lv. {pokemon.Level}");

                // 최대 체력이 0이면 게이지를 비워 둡니다.
                float hpRatio = 0f;

                if (pokemon.MaxHP > 0)
                {
                    hpRatio = (float)pokemon.CurrentHP / pokemon.MaxHP;
                }

                SetHPFill(hpRatio);
            }

            private static void SetLabel(TMP_Text label, string text)
            {
                if (label != null)
                {
                    label.text = text;
                }
            }

            // 게이지의 왼쪽을 고정한 상태에서 남은 체력만큼 너비를 채웁니다.
            private void SetHPFill(float ratio)
            {
                if (mHPFill != null)
                {
                    mHPFill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
                }
            }
        }
    }
}