using GameFramework.Event;
using UnityEngine;
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
        //private EntityComponent entityComponent;
        public override EGameMode GameMode => EGameMode.RPG;

        public override void Initialize()
        {
            base.Initialize();
            Instance = this;

            var events = GameEntry.GetComponent<EventComponent>();

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

            ShowEntitySuccessEventArgs gPlayer = (ShowEntitySuccessEventArgs)gEvent;

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
    }
}