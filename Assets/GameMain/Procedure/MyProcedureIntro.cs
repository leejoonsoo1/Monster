using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

// 프로시저가 사용하는 FSM 타입의 별칭
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    // MyProcedureIntro의 역할은 게임의 인트로 상태를 관리하는 프로시저(Procedure)
    public class MyProcedureIntro : MyProcedureBase
    {
        // Intro 프로시저가 관리할 실제 Unity 씬
        private const string IntroSceneAssetName = "Assets/Scenes/Logo.unity";

        private EventComponent mEventComponent;
        private SceneComponent mSceneComponent;

        // 인트로 씬의 로드 완료 여부
        private bool mSceneLoaded = false;

        // 다음 프로시저로 이동할지 여부
        private bool mChangeToMain = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            // 부모 Procedure의 OnEnter를 먼저 호출합니다.
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureIntro Enter");

            // Intro Procedure에서 사용하는 상태값을 초기화합니다.
            mSceneLoaded    = false;
            mChangeToMain   = false;

            // GameFramework의 EventComponent를 가져옵니다. 
            // Scene Load 성공 이벤트를 받을 때 사용합니다.
            mEventComponent = GameEntry.GetComponent<EventComponent>();

            // GameFramework의 SceneComponent를 가져옵니다.
            // Logo Scene을 로드할 때 사용합니다.
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            // GameFramework의 UIComponent를 가져옵니다.
            // 게임 시작 시 Battle UI Group을 미리 생성할 때 사용합니다.
            UIComponent uiComponent = GameEntry.GetComponent<UIComponent>();

            // EventComponent가 존재하지 않는다면
            // Scene Load 이벤트를 처리할 수 없으므로 종료합니다.
            if (mEventComponent == null)
            {
                Log.Error("EventComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // SceneComponent가 존재하지 않는다면
            // Logo Scene을 로드할 수 없으므로 종료합니다.
            if (mSceneComponent == null)
            {
                Log.Error("SceneComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // UIComponent가 존재하지 않는다면
            // Battle UI Group을 생성할 수 없으므로 종료합니다.
            if (uiComponent == null)
            {
                Log.Error("UIComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // 게임 시작 시 Battle UI Group을 미리 생성합니다.
            // Runtime UIManager에 Batle UI Group이
            // 이미 존재하는지 먼저 확인합니다.
            if (uiComponent.HasUIGroup("Battle") != true)
            {
                Log.Info("Battle UI Group 생성 시작");

                // Battle이라는 이름의 UI Group을
                // Depth 0으로 생성합니다.
                bool addResult = uiComponent.AddUIGroup("Battle", 0);

                // UI Group 생성에 실패했다면
                // 이후 Battle에서 OpenUIForm을 사용할 수 없습니다.
                if (addResult != true)
                {
                    Log.Error("Batle UI Group 생성 실패");

                    return;
                }

                Log.Info("Battle UI Group 생성 완료");
            }
            else
            {
                // 이미 등록되어 있다면 다시 생성하지 않습니다.
                Log.Info("Battle UI Group이 이미 존재합니다.");
            }

            // Battle UI Group이 실제 Runtime UIManager에
            // 등록되었는지 마지막으로 확인합니다.
            if (uiComponent.HasUIGroup("Battle") != true)
            {
                Log.Error("Battle UI Group이 Runtime에 존재하지 않습니다.");

                return;
            }

            Log.Info("Battle UI Group Runtime 확인 완료");

            // Battle UI Group 생성이 끝난 후
            // 기존 Intro Scene 로드를 진행합니다.

            // Intro Scene Load 성공 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            // Logo Scene을 로드합니다.
            mSceneComponent.LoadScene(IntroSceneAssetName);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 씬이 아직 로드되지 않았으면 아무것도 하지 않음
            if (mSceneLoaded != true)
            {
                return;
            }

            // 아무 키나 누르면 Main 프로시저로 이동
            if (Input.anyKeyDown)
            {
                mChangeToMain = true;
            }

            if (mChangeToMain)
            {
                ChangeState<MyProcedureMain>(procedureOwner);
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Log.Info("MyProcedureIntro: Leave");

            // 이벤트 구독 해제
            if (mEventComponent != null)
            {
                mEventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            }

            // Intro 씬이 로드되어 있으면 언로드
            // 씬 언로드는 SceneComponent로 실행
            if (mSceneComponent != null && mSceneComponent.SceneIsLoaded(IntroSceneAssetName))
            {
                mSceneComponent.UnloadScene(IntroSceneAssetName);
            }

            mSceneLoaded    = false;
            mEventComponent = null;
            mSceneComponent = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            LoadSceneSuccessEventArgs ne = eventArgs as LoadSceneSuccessEventArgs;

            if (ne == null)
            {
                return;
            }

            // 다른 씬의 로드 이벤트는 무시
            if (ne.SceneAssetName != IntroSceneAssetName)
            {
                return;
            }

            Log.Info("Logo Scene Load Success");

            mSceneLoaded = true;
        }
    };
}