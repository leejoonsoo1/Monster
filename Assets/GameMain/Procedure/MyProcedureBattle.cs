using GameFramework.Event;
using UnityGameFramework.Runtime;

using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    public class MyProcedureBattle : MyProcedureBase
    {
        private const string mBattleSceneAssetName = "Assets/Scenes/Battle.unity";

        // UI Component에 등록한 Battle UI Group 이름입니다.
        private const string mBattleUIGroupName = "Battle";

        private EventComponent mEventComponent;
        private SceneComponent mSceneComponent;

        // UGF UI를 열고 닫기 위해 사용합니다.
        private UIComponent mUIComponent;

        // 현재 열려 있는 배틀 배경 UI의 SerialId입니다.
        private int mBattleBackgroundSerialId = 0;

        private bool mSceneLoaded       = false;
        private bool mBattleFinished    = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureBattle : Enter");

            mSceneLoaded                = false;
            mBattleFinished             = false;
            mBattleBackgroundSerialId   = 0;

            mEventComponent = GameEntry.GetComponent<EventComponent>();
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();
            mUIComponent    = GameEntry.GetComponent<UIComponent>();

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

            if (mUIComponent == null)
            {
                Log.Error("UIComponent를 찾을 수 없습니다.");

                return;
            }

            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            mSceneComponent.LoadScene(mBattleSceneAssetName);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (mSceneLoaded != true)
            {
                return;
            }

            // Battle이 아직 끝나지 않았다면
            // Battle Procedure를 유지합니다.
            if (mBattleFinished != true)
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

            // Battle에서 빠져나갈 때 현재 열려 있는 배경 UI를 닫습니다.
            if (mUIComponent != null && mBattleBackgroundSerialId > 0)
            {
                mUIComponent.CloseUIForm(mBattleBackgroundSerialId);

                mBattleBackgroundSerialId = 0;
            }

            if (isShutdown != true &&
                mSceneComponent != null &&
                mSceneComponent.SceneIsLoaded(mBattleSceneAssetName))
            {
                mSceneComponent.UnloadScene(mBattleSceneAssetName);
            }

            mSceneLoaded    = false;
            mBattleFinished = false;

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

            // RPGGame이 존재하는지 확인합니다.
            if (RPGGame.Instance == null)
            {
                Log.Error("RPGGame.Instance를 찾을 수 없습니다.");

                return;
            }

            // Battle Background Manager가 존재하는지 확인합니다.
            if (RPGGame.Instance.BattleBackgroundManager == null)
            {
                Log.Error("BattleBackgroundManager를 찾을 수 없습니다.");

                return;
            }

            //  현재 Map 이름에 맞는 Battle Background 경로를 가져옵니다.
            string battleMapName = RPGGame.Instance.BattleMapName;

            // 해당 맵의 배틀 배경 Addressable 이름을 가져옵니다.
            string backgroundAssetName = RPGGame.Instance.BattleBackgroundManager.GetBackgroundAssetName(battleMapName);

            if (string.IsNullOrEmpty(backgroundAssetName) == true)
            {
                Log.Error($"Battle Background를 찾을 수 없습니다." + $"Map = {battleMapName}");

                return;
            }

            Log.Info($"Battle Background : " + $"{battleMapName} -> {backgroundAssetName}");

            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleUIGroupName, 0, false, null);

            if (RPGGame.Instance == null)
            {
                Log.Error("RPGGame.Instance를 찾을 수 없습니다.");

                return;
            }

            if (RPGGame.Instance.BattleBackgroundManager == null)
            {
                Log.Error("BattleBackgroundManager를 찾을 수 없습니다.");

                return;
            }

            battleMapName = RPGGame.Instance.BattleMapName;
            backgroundAssetName  = RPGGame.Instance.BattleBackgroundManager.GetBackgroundAssetName(battleMapName);

            if (string.IsNullOrEmpty(backgroundAssetName) == true)
            {
                Log.Error($"Battle Background를 찾을 수 없습니다." + $"Map = {battleMapName}");

                return;
            }

            Log.Info("Battle Backgrond : " + $"{battleMapName} -> {backgroundAssetName}");

            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleUIGroupName, 0, false, null);
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