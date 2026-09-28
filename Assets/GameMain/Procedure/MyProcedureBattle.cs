using GameFramework.Event;
using UnityGameFramework.Runtime;

using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    public class MyProcedureBattle : MyProcedureBase
    {
        private const string mBattleSceneAssetName = "Assets/Scenes/Battle.unity";

        private EventComponent mEventComponent;
        private SceneComponent mSceneComponent;

        private bool mSceneLoaded = false;

        private bool mBattleFinished = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureBattle : Enter");

            mSceneLoaded = false;

            mEventComponent = GameEntry.GetComponent<EventComponent>();
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            if (mEventComponent == null)
            {
                Log.Error("EventComponent를 찾을 수 없습니다.");
                
                return;
            }

            if (mSceneComponent == null)
            {
                Log.Error("SceneComponent를 찾을 수 없습니다.");

                return;
            }

            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            mSceneComponent.LoadScene(mBattleSceneAssetName);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!mSceneLoaded)
            {
                return;
            }

            // Battle이 아직 끝나지 않았다면
            // Battle Procedure를 유지합니다.
            if (!mBattleFinished)
            {
                return;
            }

            Log.Info("MyProcedureBattle -> MyProcedureMain");

            ChangeState<MyProcedureMain>(procedureOwner);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            if (mEventComponent != null)
            {
                mEventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
                mEventComponent.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            }

            if (!isShutdown &&
                mSceneComponent != null &&
                mSceneComponent.SceneIsLoaded(mBattleSceneAssetName))
            {
                mSceneComponent.UnloadScene(mBattleSceneAssetName);
            }

            mSceneLoaded = false;

            base.OnLeave(procedureOwner, isShutdown);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            LoadSceneSuccessEventArgs ne = eventArgs as LoadSceneSuccessEventArgs;

            if (ne == null)
            {
                return;
            }

            if (ne.SceneAssetName != mBattleSceneAssetName)
            {
                return;
            }

            Log.Info("Battle Scene Load Success");

            mSceneLoaded = true;
        }

        private void OnLoadSceneFailure(object sender, GameEventArgs eventArgs)
        {
            LoadSceneFailureEventArgs ne = eventArgs as LoadSceneFailureEventArgs;

            if (ne == null)
            {
                return;
            }

            if (ne.SceneAssetName != mBattleSceneAssetName)
            {
                return;
            }

            Log.Error("Battle Scene Load Failure : {0}", ne.ErrorMessage);
        }

        // <summary>
        // Battle 종료를 요청합니다.
        // </summary>
        public void FinishBattle()
        {
            if (mBattleFinished)
            {
                return;
            }

            Log.Info("Battle Finish");

            mBattleFinished = true;
        }
    }
}