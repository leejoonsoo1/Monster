using GameFramework.Procedure;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityGameFramework.Runtime;
using GameFramework.Event;

// 프로시저가 사용하는 FSM 타입의 별칭
using ProcedureOwner =
    GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    // MyProcedureMain의 역할은 게임의 메인 플레이 상태를 관리하는 프로시저(Procedure)
    public class MyProcedureMain : MyProcedureBase
    {
        // Main 프로시저가 관리할 실제 Unity 씬
        private const string mMainSceneAssetName = "Assets/Scenes/MainScenes.unity";

        // UGF 이벤트를 관리하는 컴포넌트
        private EventComponent mEventComponent = null;

        // UGF 씬 로드 및 언로드를 관리하는 컴포넌트
        private SceneComponent mSceneComponent = null;

        // 실제 RPG 게임의 로직을 관리하는 객체
        private RPGGame mGame = null;

        // Main 씬의 로드 완료 여부
        private bool mSceneLoaded = false;

        // RPGGame.Initialize()가 실행되었는지 여부
        private bool mGameInitialized = false;

        // OnInit is called when the procedure is initialized.
        protected override void OnInit(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            //mPlayer.Initialize();
        }

        // <summary>
        // MyProcedureMain에 진입할 때 한 번 호출됩니다.
        // 필요한 컴포넌트를 가져오고 Main 씬을 로드합니다.
        // </summary>
        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("ProcedureMain: Enter");

            // 프로시저에 다시 진입할 수도 있으므로 상태를 초기화합니다.
            mSceneLoaded = false;
            mGameInitialized = false;
            mGame = null;

            // 현재 GameFramework 오브젝트에서 필요한 컴포넌트를 가져옵니다.
            mEventComponent = GameEntry.GetComponent<EventComponent>();
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            // EventComponent가 없으면 씬 로드 완료 이벤트를 받을 수 없습니다.
            if (mEventComponent == null)
            {
                Log.Error("EventComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // SceneComponent가 없으면 UGF 방식으로 씬을 로드할 수 없습니다.
            if (mSceneComponent == null)
            {
                Log.Error("SceneComponent가 GameFramework 오브젝트에 없습니다.");

                return;
            }

            // Main 씬 로드 성공 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            // Main 씬 로드 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            // Main 씬이 이미 로드되어 있다면 다시 로드하지 않습니다.
            if (mSceneComponent.SceneIsLoaded(mMainSceneAssetName))
            {
                Log.Info("Main Scene은 이미 로드되어 있습니다.");

                mSceneLoaded = true;

                InitializeGame();
                return;
            }

            // Main 씬을 로드합니다.
            mSceneComponent.LoadScene(mMainSceneAssetName);
        }

        // <summary>
        // MyProcedureMain이 활성화된 동안 매 프레임 호출됩니다.
        // 실제 플레이어 입력이나 게임 상태 갱신이 필요할 때 사용합니다.
        // </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // Main 씬이 로드되지 않았다면 게임 로직을 처리하지 않습니다.
            if (!mSceneLoaded)
            {
                return;
            }

            // RPGGame.Initailize()는 씬 로드 성공 시 이미 한 번 호출됩니다.
            // 따라서 여기서 매 프레임 Initialize()를 호출하지 않습니다.

            /*
            if (mGame != null)
            {
                mGame.Update(elapseSeconds, realElapseSeconds);
            }
            */
        }


        // <summary>
        // MyProcedureMain에서 빠져나갈 떄 호출됩니다.
        // 이벤트 구독을 해제하고 Main 씬을 언로드합니다.
        // </summary>
        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Log.Info("MyProcedureMain : Leave");

            // 등록했던 씬 로드 이벤트를 해제합니다.
            if (mEventComponent != null)
            {
                mEventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
                mEventComponent.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            }

            // 게임 종료가 아니라 다른 프로시저로 이동하는 경우
            // Main 씬이 로드되어 있으면 언로드 합니다.
            if (!isShutdown && mSceneComponent != null && mSceneComponent.SceneIsLoaded(mMainSceneAssetName))
            {
                mSceneComponent.UnloadScene(mMainSceneAssetName);
            }

            // 사용하던 객체와 상태를 초기화합니다.
            mSceneLoaded        = false;
            mGameInitialized    = false;
            mEventComponent     = null;
            mSceneComponent     = null;

            //mPlayer?.Shutdown();
            //mGame = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        // OnDestroy is called when the procedure is destroyed.
        protected override void OnDestroy(ProcedureOwner procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        // 네이티브 대화상자 사용 여부
        public override bool UseNativeDialog => false;


        // <summary>
        // UGF에서 씬 로드가 성공했을 때 호출됩니다.
        // Main 씬의 로드 이벤트인지 확인한 뒤 게임을 초기화합니다.
        // </summary>
        private void OnLoadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            // 씬 로드 성공 이벤트 데이터로 형변환합니다.
            LoadSceneSuccessEventArgs ne =
                eventArgs as LoadSceneSuccessEventArgs;

            // 이벤트 형변환에 실패하면 처리하지 않습니다.
            if (ne == null)
            {
                return;
            }

            // Main 씬이 아닌 다른 씬의 실패 이벤트는 무시합니다.
            if (ne.SceneAssetName != mMainSceneAssetName)
            {
                return;
            }

            Log.Info("Main Scene Load Success");

            // Main 씬이 정상적으로 로드되었음을 기록합니다.
            mSceneLoaded = true;

            // Main 씬 로드가 끝난 뒤 RPGGame을 초기화합니다.
            InitializeGame();
        }


        // <summary>
        // UGF에서 씬 로드가 실패했을 때 호출됩니다.
        // Main 씬 로드 실패인 경우 오류 내용을 출력합니다.
        // </summary>
        private void OnLoadSceneFailure(object sender, GameEventArgs eventArgs)
        {
            LoadSceneFailureEventArgs ne = eventArgs as LoadSceneFailureEventArgs;

            // 이벤트 형변환에 실패하면 처리하지 않습니다.
            if (ne == null)
            {
                return;
            }

            if (ne.SceneAssetName != mMainSceneAssetName)
            {
                return;
            }

            Log.Error("Main Scene Load Failure: {0}", ne.ErrorMessage);
        }

        // <summary>
        // RPGGame 객체를 생성하고 게임을 초기화합니다
        // 중복 실행을 막기 위해 한 번만 호출됩니다.
        // </summary>
        private void InitializeGame()
        {
            // 이미 초기화했다면 다시 실행하지 않습니다.
            if (mGameInitialized)
            {
                return;
            }

            // Main 씬이 준비되지 않았다면 초기화하지 않습니다.
            if (!mSceneLoaded)
            {
                return;
            }

            // Initialize 중복 실행을 방지합니다.
            mGameInitialized = true;

            // 실제 RPG 게임 객체를 생성합니다.
            mGame = new RPGGame();

            if (mGame == null)
            {
                Log.Error("RPGGame 객체 생성에 실패했습니다.");

                return;
            }

            // 이벤트 등록과 플레이어 생성을 포함한 게임 초기화를 실행합니다.
            mGame.Initialize();
         }
    }
}