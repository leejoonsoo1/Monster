using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class UnloadSceneSuccessEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(UnloadSceneSuccessEventArgs).GetHashCode();

        public UnloadSceneSuccessEventArgs()
        {
            SceneAssetName = null;
            UserData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public string SceneAssetName
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static UnloadSceneSuccessEventArgs Create(GameFramework.Scene.UnloadSceneSuccessEventArgs e)
        {
            UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs = ReferencePool.Acquire<UnloadSceneSuccessEventArgs>();
            unloadSceneSuccessEventArgs.SceneAssetName = e.SceneAssetName;
            unloadSceneSuccessEventArgs.UserData = e.UserData;
            return unloadSceneSuccessEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            UserData = null;
        }
    }
}
