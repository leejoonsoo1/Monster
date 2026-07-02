using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Monster
{
    public class RPGGame : GameBase
    {
        public static RPGGame Instance { get; private set; }

        private Player mPlayer      = null;
        private string assetPath    = "Player";
        //private EntityComponent entityComponent;
        public override GameMode GameMode => GameMode.RPG;

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

            SpawnCharacter(assetPath, new Vector3(0f, 0f, 10f));
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

        protected override void OnShowEntitySuccess(object sender, GameEventArgs e)
        {
            base.OnShowEntitySuccess(sender, e);

            var ne = (ShowEntitySuccessEventArgs)e;

            mPlayer = ne.Entity.GetComponent<Player>();

            if (mPlayer == null)
            {
                Log.Warning("Player Component not found");
                return;
            }

            Log.Info("Player Spawn Success");
        }

        protected override void OnShowEntityFailure(object sender, GameEventArgs e)
        {
            var ne = (ShowEntityFailureEventArgs)e;
            Log.Warning("Show entity failure: {0}", ne.ErrorMessage);
        }
    }
}