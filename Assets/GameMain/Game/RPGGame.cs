using GameFramework.Entity;
using GameFramework.Event;
using GameFramework.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityGameFramework.Runtime;

using ShowEntitySuccessEventArgs = UnityGameFramework.Runtime.ShowEntitySuccessEventArgs;
using ShowEntityFailureEventArgs = UnityGameFramework.Runtime.ShowEntityFailureEventArgs;

namespace Monster
{
    public class RPGGame : GameBase
    {
        public static RPGGame Instance { get; private set; }

        private Grid mGrid;
        private Player mPlayer;

        private string mAssetPath               = "Player";
        private const string PlayerGroupName    = "Player";
        
        private string mMapAssetPath            = "Map";
        private const string MapGroupName       = "Map";

        // 현재 지역에 대응되는 Battle Background를 관리합니다.
        private string mBattleMapName           = "Route";

        private TilemapManager mTilemapManager;
        private EnCounterManager mEncounterManager;

        // 맵에 따른 Battle Background를 관리합니다.
        private BattleBackgroundManager mBattleBackgroundManager;
        // 현재 Player가 위치한 Map 이름입니다.
        private string mCurrentMapName;

        private bool mBattleRequested = false;

        public TilemapManager TilemapManager
        {
            get
            {
                return mTilemapManager;
            }
        }
        
        public EnCounterManager EncounterManager
        {
            get
            {
                return mEncounterManager;
            }
        }

        public BattleBackgroundManager BattleBackgroundManager
        {
            get
            {
                return mBattleBackgroundManager;
            }
        }

        public string BattleMapName
        {
            get
            {
                return mBattleMapName;
            }
        }

        public string CurrentMapName
        {
            get
            {
                return mMapAssetPath;
            }
        }

        public bool IsBattleRequested
        {
            get
            {
                return mBattleRequested;
            }
        }

        // <summary>
        // Scene이 변경되어도 유지할 Player의 Runtime Data입니다.
        // </summary>
        public static PlayerRuntimeData RuntimePlayerData
        {
            get;
            private set;
        } = new PlayerRuntimeData();

        // private EntityComponent entityComponent;
        public override EGameMode GameMode => EGameMode.RPG;

        public override void Initialize()
        {
            base.Initialize();
            Instance = this;

            mBattleBackgroundManager = new BattleBackgroundManager();
            mBattleBackgroundManager.Initialize();

            EventComponent eventComponent = GameEntry.GetComponent<EventComponent>();

            if (eventComponent == null)
            {
                Log.Error("EventComponent를 찾을 수 없습니다.");
                
                return;
            }

            eventComponent.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
            eventComponent.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);

            SpawnMap(mMapAssetPath, Vector3.zero);
        }

        public override void Shutdown()
        {
            EventComponent eventComponent = GameEntry.GetComponent<EventComponent>();

            if(eventComponent != null)
            {
                eventComponent.Unsubscribe(ShowEntitySuccessEventArgs.EventId,
                    OnShowEntitySuccess);

                eventComponent.Unsubscribe(ShowEntityFailureEventArgs.EventId,
                    OnShowEntityFailure);
            }

            // BattleBackgroundManager 내부 데이터를 먼저 정리합니다.
            if (mBattleBackgroundManager != null)
            {
                mBattleBackgroundManager.shutdown();
                mBattleBackgroundManager = null;
            }

            mPlayer = null;
            mGrid   = null;

            mTilemapManager             = null;
            mEncounterManager           = null;

            mBattleRequested            = false;

            if (Instance == this)
            {
                Instance = null;
            }

            base.Shutdown();
        }

        private void SpawnMap(string assetPath, Vector3 position)
        {
            // 현재 생성하는 Map을 현재 Map 이름으로 저장합니다.
            mMapAssetPath = assetPath;

            int id = EntitySerialId.Next();

            EntityComponent entityComponent = GameEntry.GetComponent<EntityComponent>();

            if (entityComponent == null)
            {
                Log.Error("EntityComponent 를 찾을 수 없습니다.");

                return;
            }

            entityComponent.ShowEntity(
                id,                             
                typeof(MapEntity),                                   // 실행할 EntityLogic
                assetPath,                                           // "Map" 프리팹 Addressable 이름
                MapGroupName,                                        // Entity Group 이름
                new MapData(id, 1, position, Quaternion.identity));  // MapEntity.OnShow로 전달
        }

        private void SpawnCharacter(string assetPath, Vector3 position, Vector2 direction)
        {
            int id = EntitySerialId.Next();

            EntityComponent entityComponent = GameEntry.GetComponent<EntityComponent>();

            if (entityComponent == null)
            {
                Log.Error("EntityComponent를 찾을 수 없습니다.");

                return;
            }

            // typeof 스크립트를 붙인다.
            entityComponent.ShowEntity(
                id,
                typeof(Player),
                assetPath,
                PlayerGroupName,
                new PlayerData(id, 1, position, direction));
        }

        protected override void OnShowEntitySuccess(object sender, GameEventArgs gEvent)
        {
            base.OnShowEntitySuccess(sender, gEvent);

            ShowEntitySuccessEventArgs args = (ShowEntitySuccessEventArgs) gEvent;

            if (args.Entity == null)
            {
                Log.Error("RPGGame : 생성된 Entity가 null입니다.");

                return;
            }

            string entityGroupName = args.Entity.EntityGroup.Name;

            if (entityGroupName == MapGroupName)
            {
                InitializeMap(args);

                return;
            }

            if (entityGroupName == PlayerGroupName)
            {

                InitializePlayer(args);

                return;
            }
        }

        private void InitializeMap(ShowEntitySuccessEventArgs args)
        {
            // 방금 생성된 Map Entity 안에서 Grid를 찾습니다.
            mGrid = args.Entity.GetComponentInChildren<Grid>(true);

            if (mGrid == null)
            {
                Log.Error("RPGGame : Map Entity 안에서 Grid를 찾지 못했습니다.");

                return;
            }

            Log.Info("RPGGame : Grid 찾기 성공");

            // Grid 아래의 모든 Tilemap을 가져옵니다.
            Tilemap[] tilemaps = mGrid.GetComponentsInChildren<Tilemap>(true);

            if (tilemaps == null || tilemaps.Length == 0)
            {
                Log.Error("RPGGame : Grid 아래에서 Tilemap을 찾지 못했습니다.");

                return;
            }

            Log.Info($"RPGGame : Tilemap 개수 = {tilemaps.Length}");


            // =========================================
            // TilemapManager 생성
            // =========================================

            // TilemapManager는 MonoBehaviour가 아니므로
            // FindAnyObjectByType을 사용하지 않습니다.
            mTilemapManager = new TilemapManager();

            // Map 안에서 찾은 Tilemap들을 전달합니다.
            mTilemapManager.Initialize(tilemaps);

            Log.Info("RPGGame : TilemapManager Initialize Success");

            // =========================================
            // EnCounterManager 생성
            // =========================================

            mEncounterManager = new EnCounterManager();

            // EncounterManager가
            // Tilemap 정보를 사용할 수 있게 전달합니다.
            mEncounterManager.Initialize(mTilemapManager);

            Log.Info("RPGGame : EnCounterManager Initialize Success");

            // =========================================
            // Map 관련 초기화가 모두 끝난 다음
            // Player를 생성합니다.
            // =========================================

            // 처음 게임을 시작했을 때 사용할 기본 위치입니다.
            Vector3 spawnPosition = new Vector3(0f, 0f, 10f);

            // 처음 게임을 시작했을 때 바라볼 기본 방향입니다.
            Vector2 spawnDirecion = Vector2.down;

            // Battle Scene으로 넘어가기 전에 저장했던
            // Player의 필드 정보가 있는지 확인합니다.
            if (RuntimePlayerData.mHasSavedFieldState)
            {
                // 저장되어 있던 Grid Cell의 정중앙 위치를 구합니다.
                Vector3 savedCellCenter = mGrid.GetCellCenterWorld(RuntimePlayerData.mCellPosition);

                // 저장된 셀의 중앙을 Player 생성 위치로 사용합니다.
                spawnPosition = savedCellCenter;

                // 저장된 바라보는 방향도 가져옵니다.
                spawnDirecion = RuntimePlayerData.mDirection;

                Log.Info($"RPGGame : Player Runtime Load - " +
                    $"Cell = {RuntimePlayerData.mCellPosition}," +
                    $"Direction = {RuntimePlayerData.mDirection}");
            }

            SpawnCharacter(mAssetPath, spawnPosition, Vector2.down);
        }

        // =========================================
        // Player 초기화
        // =========================================
        private void InitializePlayer(ShowEntitySuccessEventArgs args)
        {
            mPlayer = args.Entity.GetComponent<Player>();

            if (mPlayer == null)
            {
                Log.Warning("RPGGame : Player Component를 찾지 못했습니다.");

                return;
            }

            Log.Info("RPGGame : Player Component 찾기 성공");

            // =========================================
            // Camera 연결
            // =========================================
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Log.Warning("RPGGame : Main Camera를 찾을 수 없습니다.");

                return;
            }

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

            if (cameraFollow == null)
            {
                Log.Warning("RPGGame : CameraFollow Component를 찾을 수 없습니다.");

                return;
            }

            // 생성된 Player를 카메라 Target으로 설정
            cameraFollow.mTarget = mPlayer.transform;

            Log.Info("RPGGame : Camera Target Setting Success");
        }
        
        protected override void OnShowEntityFailure(object sender, GameEventArgs gEvent)
        {
            var vEvent = (ShowEntityFailureEventArgs)gEvent;

            Log.Warning("Show entity failure: {0}", vEvent.ErrorMessage);
        }

        // <summary>
        // 필드에서 랜덤 인카운터가 발생했을 때 호출합니다.
        // </summary>
        public void RequestBattle()
        {
            if (mBattleRequested)
            {
                return;
            }

            Log.Info("RPGGame : Battle Encounter");

            mBattleRequested = true;
        }

        // <summary>
        // Battle 전환 요청을 초기화합니다.
        // <summary>
        public void ClearBattleRequest()
        {
            mBattleRequested = false;
        }
    }
}