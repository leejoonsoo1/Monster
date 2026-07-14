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
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureIntro Enter");

            mSceneLoaded = false;
            mChangeToMain = false;

            // 현재 프로젝트의 GameEntry는 Eevent, Scene 프로퍼티가 없으므로
            // GetComponent<T>()로 직접 컴포넌트를 가져와야 함
            mEventComponent = GameEntry.GetComponent<EventComponent>();
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            if (mEventComponent == null)
            {
                Log.Error("EventComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            if (mSceneComponent == null)
            {
                Log.Error("SceneComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // Intro 씬 로드 성공 이벤트 등록
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            mSceneComponent.LoadScene(IntroSceneAssetName);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 씬이 아직 로드되지 않았으면 아무것도 하지 않음
            if (!mSceneLoaded)
            {
                return;
            }

            // 테스트용:
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