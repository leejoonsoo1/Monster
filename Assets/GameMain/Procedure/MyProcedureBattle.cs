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
        private const string mBattleBackgroundUIGroupName = "BattleBackground";

        // Battle에서 항상 사용하는 하단 공통 UI Form입니다.
        private const string mBattleUIAssetName = "BattleUI";

        private EventComponent mEventComponent;
        private SceneComponent mSceneComponent;

        // UGF UI를 열고 닫기 위해 사용합니다.
        private UIComponent mUIComponent;

        // 현재 열려 있는 배틀 배경 UI의 SerialId입니다.
        private int mBattleBackgroundSerialId = 0;

        // 현재 열려 있는 공통 Battle UI의 SerialId입니다.
        private int mBattleUISerialId = 0;

        private bool mSceneLoaded       = false;
        private bool mBattleFinished    = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureBattle : Enter");

            // Battle Procedure의 상태를 초기화합니다.
            mSceneLoaded    = false;
            mBattleFinished = false;

            // 현재 열려 있는 Battle Background UI의 SerialId를 초기화합니다
            mBattleBackgroundSerialId = 0;

            // BattleUI SerialiId를 초기화합니다.
            mBattleUISerialId = 0;

            // GameFramework의 EventComponent를 가져옵니다.
            mEventComponent = GameEntry.GetComponent<EventComponent>();

            // GameFramework의 SceneComponent를 가져옵니다.
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            // GameFramework의 UIComponent를 가져옵니다.
            mUIComponent    = GameEntry.GetComponent<UIComponent>();

            // EventComponent를 찾지 못했다면 이벤트를 처리를 할 수 없으므로 종료합니다.
            if (mEventComponent == null)
            {
                Log.Error("EventComponent를 찾을 수 없습니다.");

                return;
            }

            // SceneComponent를 찾지 못했다면 Battle Scene을 로드할 수 없으므로 종료합니다.
            if (mSceneComponent == null)
            {
                Log.Error("SceneComponent를 찾을 수 없습니다.");

                return;
            }

            // UIComponent를 찾지 못했다면 Battle UI Group을 생성할 수 없으므로 종료합니다.
            if (mUIComponent == null)
            {
                Log.Error("UIComponent를 찾을 수 없습니다.");

                return;
            }

            // Runtime UIManager에 Battle UI Group이 존재하는지 확인합니다.
            if (mUIComponent.HasUIGroup(mBattleUIGroupName) != true)
            {
                Log.Info("Battle UI Group이 존재하지 않습니다. 생성합니다.");

                // Battle UI Group을 Depth 0으로 생성합니다.
                bool addResult = mUIComponent.AddUIGroup(mBattleUIGroupName, 0);

                // UI Group 생성에 실패했다면 이후 OpenUIForm을 사용할 수 없으므로 종료합니다.
                if (addResult != true)
                {
                    Log.Error("Battle UI Group 생성에 실패했습니다.");

                    return;
                }
            }

            if (mUIComponent.HasUIGroup(mBattleBackgroundUIGroupName) != true)
            {
                bool addResult = mUIComponent.AddUIGroup(mBattleBackgroundUIGroupName, -1);

                if (addResult != true)
                {
                    Log.Error("BattleBackground UI Group 생성에 실패했습니다.");

                    return;
                }
            }

            // Battle Scene Load 성공 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            // Battle Scene Load 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            // UI Form 실행 성공 이벤트를 등록합니다. 
            mEventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);

            // UI Form 실행 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);

            // Battle Scene을 로드합니다.
            mSceneComponent.LoadScene(mBattleSceneAssetName);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (mSceneLoaded != true)
            {
                return;
            }

            // Battle이 아직 끝나지 않았다면 Battle Procedure를 유지합니다.
            if (mBattleFinished != true)
            {
                return;
            }

            Log.Info("MyProcedureBattle -> MyProcedureMain");

            ChangeState<MyProcedureMain>(procedureOwner);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Log.Info("MyProcedureBattle : Leave");

            if (isShutdown != true & mEventComponent != null)
            {
                if (mEventComponent.Check(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess) == true)
                {
                    mEventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
                }

                if (mEventComponent.Check(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure) == true)
                {
                    mEventComponent.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
                }

                if (mEventComponent.Check(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess) == true)
                {
                    mEventComponent.Unsubscribe(OpenUIFormSuccessEventArgs.EventId, OnOpenUIFormSuccess);
                }

                if (mEventComponent.Check(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure) == true)
                {
                    mEventComponent.Unsubscribe(OpenUIFormFailureEventArgs.EventId, OnOpenUIFormFailure);
                }
            }

            // 게임 종료 중에는 UI Form을 직접 닫지 않습니다.
            // 존재하거나 로딩 중인 UI Form만 닫습니다.
            if (isShutdown != true && mUIComponent != null)
            {
                // BattleUI가 실제로 열려 있을 때만 닫습니다.
                if (mBattleUISerialId > 0 &&
                    mUIComponent.HasUIForm(mBattleUISerialId) == true)
                {
                    // 배경 SerialId가 아닌 BattleUI SerialId를 사용합니다.
                    mUIComponent.CloseUIForm(mBattleUISerialId);
                }
            }

            // 전투 배경 UI도 별도로 닫습니다.
            if (mBattleBackgroundSerialId > 0 && 
                mUIComponent.HasUIForm(mBattleBackgroundSerialId) == true)
            {
                mUIComponent.CloseUIForm(mBattleBackgroundSerialId);
            }

            if (isShutdown != true && 
                mSceneComponent != null && 
                mSceneComponent.SceneIsLoaded(mBattleSceneAssetName) == true)
            {
                Log.Info("Battle Scene Unload");

                mSceneComponent.UnloadScene(mBattleSceneAssetName);
            }

            mSceneLoaded    = false;
            mBattleFinished = false;

            mBattleBackgroundSerialId   = 0;
            mBattleUISerialId           = 0;

            mEventComponent             = null;
            mSceneComponent             = null;
            mUIComponent                = null;

            base.OnLeave(procedureOwner, isShutdown);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs eventArgs)
        {
            // 전달받은 이벤트를 Scene Load 성공 이벤트로 변환합니다.
            LoadSceneSuccessEventArgs ne = eventArgs as LoadSceneSuccessEventArgs;

            // 정상적인 Scene Load Success 이벤트가 아니라면 처리하지 않고 종료합니다.
            if (ne == null)
            {
                return;
            }

            // 현재 로드된 Scene이 Battle Scene인지 확인합니다.
            if (ne.SceneAssetName != mBattleSceneAssetName)
            {
                return;
            }

            Log.Info("Battle Scene Load Success");

            // Battle Scene의 로드가 완료되었음을 저장합니다.
            mSceneLoaded = true;

            // UIComponent가 정상적으로 존재하는지 확인합니다.
            if (mUIComponent == null)
            {
                Log.Error("UIComponent를 찾을 수 없습니다.");

                return;
            }

            // Battle UI Group이 Runtime에 존재하는지 확인합니다.
            if (mUIComponent.HasUIGroup(mBattleUIGroupName) != true)
            {
                Log.Error(
                    $"Battle UI Group을 찾을 수 없습니다. " +
                    $"Group = {mBattleUIGroupName}");

                return;
            }

            if (mUIComponent.HasUIGroup(mBattleBackgroundUIGroupName) != true)
            {
                Log.Error("BattleBackground UI Group을 찾을 수 없습니다.");

                return;
            }

            // RPGGame이 유지되고 있는지 확인합니다.
            if (RPGGame.Instance == null)
            {
                Log.Error(
                    "RPGGame.Instance를 찾을 수 없습니다.");

                return;
            }

            // 맵에 맞는 Battle Background를 가져오기 위해 BattleBackgroundManager가 존재하는지 확인합니다.
            if (RPGGame.Instance.BattleBackgroundManager == null)
            {
                Log.Error("BattleBackgroundManager를 찾을 수 없습니다.");

                return;
            }

            // 현재 Battle이 발생한 Map 이름을 가져옵니다.
            string battleMapName = RPGGame.Instance.BattleMapName;

            // Battle Background Addressable 이름을 가져옵니다.
            string backgroundAssetName = RPGGame.Instance.BattleBackgroundManager.GetBackgroundAssetName(battleMapName);

            // 배경 Addressable 이름을 가져오지 못했다면 Background UI를 열 수 없으므로 종료합니다.
            if (string.IsNullOrEmpty(backgroundAssetName) == true)
            {
                Log.Error(
                    $"Battle Background를 찾을 수 없습니다. " +
                    $"Map = {battleMapName}");

                return;
            }

            // 배경 전용 SerialId 변수에 저장합니다.
            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleBackgroundUIGroupName, 0, false, battleMapName);

            // 실제 Background SerialId를 출력합니다.
            Log.Info(
                $"Battle Background Open : " +
                $"{backgroundAssetName}, " +
                $"Map = {battleMapName}, " +
                $"SerialId = {mBattleBackgroundSerialId}");

            // Route_bg를 배경 전용 그룹에서 엽니다.
            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleBackgroundUIGroupName, 0, false, battleMapName);

            Log.Info(
                $"Battle Background Open : {backgroundAssetName}," +
                $"SerialId = {mBattleBackgroundSerialId}");

            // background보다 위에 표시되도록 Priority를 1로 설정합니다.
            mBattleUISerialId = mUIComponent.OpenUIForm(mBattleUIAssetName, mBattleUIGroupName, 0, false, null);

            Log.Info($"Battle UI Open : " +
                $"{mBattleUIAssetName}" + 
                $"SerialId = {mBattleUISerialId}");
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

        private void OnOpenUIFormSuccess(object sender, GameEventArgs eventArgs)
        {
            // 전달받은 이벤트를 UI Form Open Success 이벤트로 변환합니다.
            OpenUIFormSuccessEventArgs ne = eventArgs as OpenUIFormSuccessEventArgs;

            // OpenUISuccessEventArgs가 아니라면 처리하지 않고 종료합니다.
            if (ne == null)
            {
                return;
            }

            // 실제로 어떤 UI Form이 열렸는지 출력합니다.
            Log.Info($"Battle UI Open Success : " + $"{ne.UIForm.UIFormAssetName}");
        }

        private void OnOpenUIFormFailure(object sender, GameEventArgs eventArgs)
        {
            // 전달받은 GameEventArgs를 UI Form 열기 실패 이벤트로 변환합니다.
            OpenUIFormFailureEventArgs ne = eventArgs as OpenUIFormFailureEventArgs;

            // OpenUIFormFialureEventArgs가 아니라면 처리하지 않고 종료합니다.
            if (ne == null)
            {
                return;
            }

            // 어떤 UI Form을 여는 데 실패했는지와 실패 원인을 Console에 출력합니다.
            Log.Error($"Battle UI Open Failure : " +
                $"{ne.UIFormAssetName}, " +
                $"Error = {ne.ErrorMessage}");
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