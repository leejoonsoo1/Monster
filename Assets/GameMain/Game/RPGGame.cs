using GameFramework.Event;
using GameFramework.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityGameFramework.Runtime;

// 배틀 관련 규칙, 몬스터 체력 관련 규칙,
// 1. 이동해서 만나는 것 까지만 생각.
// 2. 
namespace Monster
{
    public class RPGGame : GameBase
    {
        public static RPGGame Instance { get; private set; }

        private Player mPlayer      = null;
        private string mAssetPath   = "Player";

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

            // TilemapManager는 더 이상 MonoBehaviour가 아니기 때문에
            // FindAnyObjectByType<TilemapManager>()를 사용할 수 없습니다.
            Grid grid = Object.FindAnyObjectByType<Grid>();

            if (grid == null)
            {
                Log.Error("RPGGame : Grid를 찾을 수 없습니다.");

                return;
            }

            // Grid 아래에 있는 모든 Tilemap 컴포넌트를 가져옵니다.
            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

            if (tilemaps == null || tilemaps.Length == 0)
            {
                Log.Error("RPGGame : Grid 아래에서 Tilemap을 찾을 수 없습니다.");

                return;
            }

            // 일반 C# 클래스인 TilemapManager를 직접 생성합니다.
            mTilemapManager = new TilemapManager();

            // 실제 Unity Tilemap들을 TilemapManager에 전달합니다.
            mTilemapManager.Initialize(tilemaps);

            // 일반 C# 클래스인 TilemapManager를 직접 생성합니다.
            mEncounterManager = new EnCounterManager();

            // RPGGame이 생성한 TilemapManager를 전달합니다.
            mEncounterManager.Initialize(mTilemapManager);

            EventComponent events = GameEntry.GetComponent<EventComponent>();

            if (events != null)
            {
                events.Subscribe(ShowEntitySuccessEventArgs.EventId, OnShowEntitySuccess);
                events.Subscribe(ShowEntityFailureEventArgs.EventId, OnShowEntityFailure);
            }

            SpawnCharacter(mAssetPath, new Vector3(0f, 0f, 10f));
        }

        private void SpawnCharacter(string assetPath, Vector3 position)
        {
            int id = EntitySerialId.Next();

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

            ShowEntitySuccessEventArgs gPlayer = (ShowEntitySuccessEventArgs) gEvent;

            mPlayer = gPlayer.Entity.GetComponent<Player>();

            if (mPlayer == null)
            {
                Log.Warning("Player Component not found");

                return;
            }

            Log.Info("Player Spawn Success");

            // Main Camera에서 CameraFollow 컴포넌트를 가져온다.
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

            if (cameraFollow == null)
            {
                Log.Warning("CameraFollow Component not found");

                return;
            }

            // 카메라가 생성된 Player를 따라가도록 설정
            cameraFollow.mTarget = mPlayer.transform;

            Log.Info("Camera Target Setting Success");
        }

        protected override void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            var ne = (ShowEntityFailureEventArgs)e;

            Log.Warning("Show entity failure: {0}", ne.ErrorMessage);
        }

        private void CheckEncounter()
        {
            if (RPGGame.Instance == null)
            {
                return;
            }

            EnCounterManager encounterManager = RPGGame.Instance.EncounterManager;

            if (encounterManager == null)
            {
                return;
            }
        }
    }
}