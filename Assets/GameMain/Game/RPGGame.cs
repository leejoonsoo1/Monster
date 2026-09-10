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

        private TilemapManager mTilemapManager;
        private EnCounterManager mEncounterManager;

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

        // private EntityComponent entityComponent;
        public override EGameMode GameMode => EGameMode.RPG;

        public override void Initialize()
        {
            base.Initialize();
            Instance = this;

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

        private void SpawnMap(string assetPath, Vector3 position)
        {
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
                "Map",                                               // Entity Group 이름
                new MapData(id, 1, position, Quaternion.identity));  // MapEntity.OnShow로 전달
        }

        private void SpawnCharacter(string assetPath, Vector3 position)
        {
            int id = EntitySerialId.Next();

            // typeof 스크립트를 붙인다.
            GameEntry.GetComponent<EntityComponent>().ShowEntity(
                id,
                typeof(Player),
                assetPath,
                "Player",
                new PlayerData(id, 1, position));
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
            SpawnCharacter(mAssetPath, new Vector3(0f, 0f, 10f));
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
    }
}