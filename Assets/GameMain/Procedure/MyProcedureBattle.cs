using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Monster
{
    public class MyProcedureBattle : MyProcedureBase
    {
        private const string mBattleSceneAssetName = "Assets/Scenes/Battle.unity";

        // UI Component에 등록한 Battle UI Group 이름입니다.
        private const string mBattleUIGroupName = "Battle";
        private const string mBattleUIAssetName = "BattleUI";

        private const string mBattleBackgroundUIGroupName = "BattleBackground";

        // Battle에서 항상 사용하는 하단 공통 UI Form입니다.
        private EventComponent mEventComponent;
        private SceneComponent mSceneComponent;

        // UGF UI를 열고 닫기 위해 사용합니다.
        private UIComponent mUIComponent;
        private BattlePokemonSpawner mPokemonSpawner;

        // 현재 테스트용 상대 포켓몬의 도감 번호입니다.
        private int mEnemyPokemonId = 133;

        // 현재 열려 있는 배틀 배경 UI의 SerialId입니다.
        private int mBattleBackgroundSerialId = 0;

        // 현재 열려 있는 공통 Battle UI의 SerialId입니다.
        private int mBattleUISerialId = 0;

        private bool mSceneLoaded       = false;
        private bool mBattleFinished    = false;
        private bool mPokemonSpawnRequested = false;

        public override bool UseNativeDialog => false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            Log.Info("MyProcedureBattle : Enter");

            // Battle Procedure의 상태를 초기화합니다.
            mSceneLoaded            = false;
            mBattleFinished         = false;
            mPokemonSpawnRequested  = false;

            // 현재 열려 있는 Battle Background UI의 SerialId를 초기화합니다
            mBattleBackgroundSerialId   = 0;

            // BattleUI SerialiId를 초기화합니다.
            mBattleUISerialId           = 0;

            // GameFramework의 EventComponent를 가져옵니다.
            mEventComponent     = GameEntry.GetComponent<EventComponent>();

            // GameFramework의 SceneComponent를 가져옵니다.
            mSceneComponent     = GameEntry.GetComponent<SceneComponent>();

            // GameFramework의 UIComponent를 가져옵니다.
            mUIComponent        = GameEntry.GetComponent<UIComponent>();

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
            mEventComponent.Subscribe(LoadSceneSuccessEventArgs.EventId,    OnLoadSceneSuccess);

            // Battle Scene Load 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(LoadSceneFailureEventArgs.EventId,    OnLoadSceneFailure);

            // UI Form 실행 성공 이벤트를 등록합니다. 
            mEventComponent.Subscribe(OpenUIFormSuccessEventArgs.EventId,   OnOpenUIFormSuccess);

            // UI Form 실행 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(OpenUIFormFailureEventArgs.EventId,   OnOpenUIFormFailure);

            // 전투 포켓몬 생성 성공 이벤트를 등록합니다.
            mEventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowBattleEntitySuccess);

            // 전투 포켓몬 생성 실패 이벤트를 등록합니다.
            mEventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowBattleEntityFailure);

            // Battle Scene을 로드합니다.
            mSceneComponent.LoadScene(mBattleSceneAssetName);
        }

        /// <summary>
        /// 배틀 중 포켓몬 소환과 적 체력 갱신을 처리하고, 종료 요청이 있으면 필드로 복귀합니다.
        /// </summary>
        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // Battle Scene이 준비되기 전에는 입력과 배틀 진행을 처리하지 않습니다.
            if (mSceneLoaded != true)
            {
                return;
            }

            if (mBattleFinished != true)
            {
                // JSON 로딩과 초기 파티 생성이 늦어져도 준비된 프레임에 소환합니다.
                TrySpawnBattlePokemon();

                UpdateBattlePokemon();
            }

            // 적 처치나 도망 등으로 종료 요청이 설정되기 전에는 현재 배틀을 유지합니다.
            if (mBattleFinished != true)
            {
                return;
            }

            Log.Info("MyProcedureBattle -> MyProcedureMain");

            // Main 초기화 시 저장한 필드 위치와 방향을 복원합니다.
            ChangeState<MyProcedureMain>(procedureOwner);
        }

        // <summary>
        // 소환된 엔티티에서 전투 데이터를 가져오고, 피해 처리 뒤에 상태창을 갱신합니다.
        // </summary>
        private void UpdateBattlePokemon()
        {
            if (mPokemonSpawner == null)
            {
                return;
            }

            EntityComponent entityComponent = GameEntry.GetComponent<EntityComponent>();
            int enemyEntityId               = mPokemonSpawner.EnemyEntityId;

            PokemonData enemyStats          = GetBattlePokemonStats(entityComponent, enemyEntityId);
            PokemonData playerStats         = GetBattlePokemonStats(entityComponent, mPokemonSpawner.PlayerEntityId);

            BattleUI battleUI               = mUIComponent.GetUIForm(mBattleUISerialId)?.Logic as BattleUI;

            // 도망 메시지가 표시되는 동안에는 D키 테스트로 배틀이 먼저 종료되지 않게 합니다.
            if (enemyStats != null && (battleUI == null || !battleUI.IsEscapePlaying))
            {
                UpdateEnemyPokemon(entityComponent, enemyEntityId, enemyStats, Input.GetKeyDown(KeyCode.D));
            }

            // UI와 엔티티의 로딩 순서에 관계없이 준비된 포켓몬부터 표시합니다.
            if (battleUI != null)
            {
                battleUI.SetPokemonStatus(enemyStats, playerStats);
            }

            if (Input.GetKeyDown(KeyCode.T) == true)
            {
                PokemonData firstAttacker = DetermineFirstAttacker(playerStats, enemyStats);

                if (firstAttacker != null)
                {
                    Log.Info($"선공 포켓몬 : {firstAttacker.Name}");
                }
            }
        }

        // 선공 포켓몬을 결정합니다.
        private PokemonData DetermineFirstAttacker(PokemonData playerStats, PokemonData enemyStats)
        {
            if (playerStats == null || enemyStats == null)
            {
                return null;
            }

            // 스피드가 높은 포켓몬이 선공합니다.
            if (playerStats.Speed > enemyStats.Speed)
            {
                return playerStats;
            }
            else if (playerStats.Speed < enemyStats.Speed)
            {
                return enemyStats;
            }
            else
            {
                // 스피드가 같으면 무작위로 선공을 결정합니다.
                if (Random.Range(0, 2) == 0)
                {
                    return playerStats;
                }
                else
                {
                    return enemyStats;
                }
            }
        }

        // 스포너가 발급한 Entity ID로 현재 전투 중인 포켓몬의 능력치를 찾습니다.
        private static PokemonData GetBattlePokemonStats(EntityComponent entityComponent, int entityId)
        {
            if (entityComponent == null || entityId == 0)
            {
                return null;
            }

            BattlePokemon pokemon = entityComponent.GetEntity(entityId)?.Logic as BattlePokemon;

            if (pokemon == null)
            {
                return null;
            }

            if (pokemon.PokemonData == null)
            {
                return null;
            }

            return pokemon.PokemonData.Stats;
        }

        // D키로 적의 체력을 5 줄이고, 체력이 0이면 기존 배틀 종료 흐름을 실행합니다.
        private void UpdateEnemyPokemon(EntityComponent entityComponent, int enemyEntityId, PokemonData enemyStats, bool damageEnemy)
        {
            if (damageEnemy)
            {
                // 적 포켓몬 체력 5 감소. SetHP에서 0 미만이 되지 않도록 제한합니다.
                enemyStats.SetHP(enemyStats.CurrentHP - 5);

                Log.Info($"{enemyStats.Name} HP : {enemyStats.CurrentHP}/{enemyStats.MaxHP}");
            }

            // 적 체력이 0 이하면 숨긴 뒤 기존 종료 흐름으로 필드 위치를 복원합니다.
            if (enemyStats.CurrentHP <= 0)
            {
                Log.Info($"{enemyStats.Name} 쓰러짐");

                // 적을 먼저 회수하고 종료를 요청합니다.
                // 이후 OnUpdate의 Main 전환과 OnLeave의 UI/Entity 정리가 이어집니다.
                entityComponent.HideEntity(enemyEntityId);

                FinishBattle();
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            Log.Info("MyProcedureBattle : Leave");

            // UI 상태와 관계없이 생성하거나 로딩 중인 배틀 Entity를 회수합니다.
            if (mPokemonSpawner != null)
            {
                mPokemonSpawner.DespawnBattlePokemon();

                mPokemonSpawner = null;
            }

            if (isShutdown != true && mEventComponent != null)
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

                if (mEventComponent.Check(ShowEntitySuccessEventArgs.EventId, OnShowBattleEntitySuccess) == true)
                {
                    mEventComponent.Unsubscribe(ShowEntitySuccessEventArgs.EventId, OnShowBattleEntitySuccess);
                }

                if (mEventComponent.Check(ShowEntityFailureEventArgs.EventId, OnShowBattleEntityFailure) == true)
                {
                    mEventComponent.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnShowBattleEntityFailure);
                }
            }

            // 게임 종료 중에는 UI Form을 직접 닫지 않습니다.
            // 존재하거나 로딩 중인 UI Form만 닫습니다.
            if (isShutdown != true && mUIComponent != null)
            {
                // 로딩 중인 UI도 취소하여 복귀 후 배틀 UI가 나타나지 않게 합니다.
                if (mBattleUISerialId > 0 && (mUIComponent.HasUIForm(mBattleUISerialId) || mUIComponent.IsLoadingUIForm(mBattleUISerialId)))
                {
                    // 배경 SerialId가 아닌 BattleUI SerialId를 사용합니다.
                    mUIComponent.CloseUIForm(mBattleUISerialId);
                }

                if (mBattleBackgroundSerialId > 0 && (mUIComponent.HasUIForm(mBattleBackgroundSerialId) || mUIComponent.IsLoadingUIForm(mBattleBackgroundSerialId)))
                {
                    mUIComponent.CloseUIForm(mBattleBackgroundSerialId);
                }
            }

            if (isShutdown != true && mSceneComponent != null && mSceneComponent.SceneIsLoaded(mBattleSceneAssetName) == true)
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

            // 배경 UI 로딩 전에 몬스터 소환을 요청합니다.
            EntityComponent entityComponent = GameEntry.GetComponent<EntityComponent>();

            if (entityComponent == null)
            {
                Log.Error("EntityComponent를 찾을 수 없습니다.");

                return;
            }

            // 기존 Spawner가 있다면 재사용합니다.
            mPokemonSpawner = GameEntry.GetComponent<BattlePokemonSpawner>();

            if (mPokemonSpawner == null)
            {
                // Spawnwer가 없다면 EntityComponent와 같은 오브젝트에 생성합니다.
                mPokemonSpawner = entityComponent.gameObject.AddComponent<BattlePokemonSpawner>();

                Log.Info("MyProcedureBattle : BattlePokemonSpawner 생성 완료");
            }

            TrySpawnBattlePokemon();

            // mBattle UI Group을 확인합니다.
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
                Log.Error($"Battle Background를 찾을 수 없습니다. " + $"Map = {battleMapName}");

                return;
            }

            // Route_bg를 배경 전용 그룹에서 엽니다.
            mBattleBackgroundSerialId = mUIComponent.OpenUIForm(backgroundAssetName, mBattleBackgroundUIGroupName, 0, false, battleMapName);

            Log.Info(
                $"Battle Background Open : {backgroundAssetName}," +
                $"SerialId = {mBattleBackgroundSerialId}");

            // background보다 위에 표시되도록 Priority를 1로 설정합니다.
            mBattleUISerialId = mUIComponent.OpenUIForm(mBattleUIAssetName, mBattleUIGroupName, 0, false, mEnemyPokemonId);

            Log.Info($"Battle UI Open : {mBattleUIAssetName}," +
                $"SerialId = {mBattleUISerialId}");
        }

        /// <summary>
        /// JSON 로딩과 파티 구성이 완료되면 전투 포켓몬 생성을 한 번만 요청합니다.
        /// </summary>
        private void TrySpawnBattlePokemon()
        {
            // 종료됐거나 이미 요청했다면 중복 소환하지 않습니다. 필요한 객체도 확인합니다.
            if (mBattleFinished || mPokemonSpawnRequested || mPokemonSpawner == null || RPGGame.Instance == null)
            {
                return;
            }

            PokemonDatabase database = RPGGame.Instance.PokemonDatabase;
            PokemonPartyManager party = RPGGame.Instance.PokemonPartyManager;
            if (database == null || !database.mIsLoaded || party == null || party.Count == 0)
            {
                // 아직 준비 중이면 반환하고, OnUpdate에서 다음 프레임에 재확인합니다.
                return;
            }

            // 실제 소환 호출 전에 표시하여 반복 갱신에서도 한 번만 요청되게 합니다.
            mPokemonSpawnRequested = true;
            mPokemonSpawner.SpawnBattlePokemon(mEnemyPokemonId);
            Log.Info($"MyProcedureBattle : 전투 포켓몬 생성 요청 완료, EnemyPokemonId = {mEnemyPokemonId}");
        }

        // UGF에서 전투 포켓몬 생성 성공 여부를 확인합니다.
        private void OnShowBattleEntitySuccess(object sender, GameEventArgs eventArgs)
        {
            ShowEntitySuccessEventArgs args = eventArgs as ShowEntitySuccessEventArgs;

            if (args == null || args.Entity == null || args.Entity.EntityGroup == null)
            {
                return;
            }

            // BattlePokemon 그룹에 속한 엔티티만 처리합니다.
            if (args.Entity.EntityGroup.Name != "BattlePokemon")
            {
                return;
            }

            Log.Info($"[Battle Debug] ShowEntity 성공 :" +
                $"EntityId = {args.Entity.Id}," +
                $"Asset = {args.Entity.EntityAssetName}");
        }

        // UGF에서 전투 포켓몬 생성이 실패한 경우 로그를 출력합니다.
        private void OnShowBattleEntityFailure(object sender, GameEventArgs eventArgs)
        {
            ShowEntityFailureEventArgs ne = eventArgs as ShowEntityFailureEventArgs;

            if (ne == null)
            {
                return;
            }

            if (ne.EntityGroupName != "BattlePokemon")
            {
                return;
            }

            Log.Error($"Battle Pokemon 생성 실패 : EntityId = {ne.EntityId}, Asset = {ne.EntityAssetName}, Error = {ne.ErrorMessage}");
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

        /// <summary>
        /// 배틀 종료 플래그를 설정합니다. 실제 Main 전환은 OnUpdate에서 처리합니다.
        /// 여러 곳에서 종료를 요청하더라도 최초 요청만 처리합니다.
        /// </summary>
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