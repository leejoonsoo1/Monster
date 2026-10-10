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

        // Battle에서 항상 사용하는 하단 공통 UI Form입니다.
        private const string mBattleUIAssetName = "overlay_command";

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
            // 부모 Procedure의 OnEnter를 먼저 호출합니다.
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureBattle : Enter");

            // Battle Procedure의 상태를 초기화합니다.
            mSceneLoaded   = false;
            mBattleFinished = false;

            // 현재 열려 있는 Battle Background UI의/ SerialId를 초기화합니다
            mBattleBackgroundSerialId = 0;

            // GameFramework의 EventComponent를 가져옵니다.
            mEventComponent = GameEntry.GetComponent<EventComponent>();

            // GameFramework의 SceneComponent를 가져옵니다.
            mSceneComponent = GameEntry.GetComponent<SceneComponent>();

            // GameFramework의 UIComponent를 가져옵니다.
            mUIComponent = GameEntry.GetComponent<UIComponent>();

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

                Log.Info("Battle UI Group 생성 완료");
            }
            else
            {
                // 이미 Battle UI Group이 존재하는 경우입니다.
                Log.Info("Battle UI Group이 이미 존재합니다.");
            }

            // Battle Scene Load 성공 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            // Battle Scene Load 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);

            // 지금은 Background를 열지 않을 것이므로 UI Form Open 성공/실패 이벤트는 일단 사용하지 않습니다.

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

            // Battle Procedure에서 사용했던 이벤트들을 해제합니다.
            if (mEventComponent != null)
            {
                // Battle Scene Load 성공 이벤트를 해제합니다.
                mEventComponent.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

                // Battle Scene Load 실패 이벤트를 해제합니다.
                mEventComponent.Unsubscribe(LoadSceneFailureEventArgs.EventId, OnLoadSceneFailure);
            }

            // BattleBottomUI를 별도로 닫는 처리를 추가합니다.
            if (mUIComponent != null && mBattleUISerialId > 0)
            {
                mUIComponent.CloseUIForm(mBattleUISerialId);

                // BattleBottomUI의 SerialId를 초기화합니다.
                mBattleUISerialId = 0;
            }

            // Battle에서 빠져나갈 때 현재 열려 있는 배경 UI를 닫습니다.
            if (mUIComponent != null && mBattleBackgroundSerialId > 0)
            {
                mUIComponent.CloseUIForm(mBattleBackgroundSerialId);

                // 저장했던 SerialId를 초기화합니다.
                mBattleBackgroundSerialId = 0;
            }

            // 게임 종료가 아니라 다른 Procedure로 이동하는 경우 Battle Scene을 언로드합니다.
            if (isShutdown != true &&
                mSceneComponent != null &&
                mSceneComponent.SceneIsLoaded(mBattleSceneAssetName) == true)
            {
                Log.Info("Battle Scene Unload");

                mSceneComponent.UnloadScene(mBattleSceneAssetName);
            }

            // Battle Procedure에서 사용햇던 상태값을 초기화합니다.
            mSceneLoaded    = false;
            mBattleFinished = false;

            // 혹시 위에서 닫히지 않았더라도 다음 Battle 진입을 위해 SerialId를 초기화합니다.
            mBattleBackgroundSerialId = 0;

            // BattleBottom의 SerialId를 초기화
            mBattleUISerialId = 0;

            // 사용했던 Component 참조를 초기화합니다.
            mEventComponent = null;
            mSceneComponent = null;
            mUIComponent    = null;
            
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
            if (mUIComponent.HasUIGroup(
                    mBattleUIGroupName) != true)
            {
                Log.Error(
                    $"Battle UI Group을 찾을 수 없습니다. " +
                    $"Group = {mBattleUIGroupName}");

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
            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleUIGroupName, 0, false, battleMapName);

            // 실제 Background SerialId를 출력합니다.
            Log.Info(
                $"Battle Background Open : " +
                $"{backgroundAssetName}, " +
                $"Map = {battleMapName}, " +
                $"SerialId = {mBattleBackgroundSerialId}");

            // background보다 위에 표시되도록 Priority를 1로 설정합니다.
            mBattleUISerialId = mUIComponent.OpenUIForm(mBattleUIAssetName, mBattleUIGroupName, 1, false, null);

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