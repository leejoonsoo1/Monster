using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadSceneUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadSceneUpdateEventArgs).GetHashCode();

        public LoadSceneUpdateEventArgs()
        {
            SceneAssetName = null;
            Progress = 0f;
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

        public float Progress
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadSceneUpdateEventArgs Create(GameFramework.Scene.LoadSceneUpdateEventArgs e)
        {
            LoadSceneUpdateEventArgs loadSceneUpdateEventArgs = ReferencePool.Acquire<LoadSceneUpdateEventArgs>();
            loadSceneUpdateEventArgs.SceneAssetName = e.SceneAssetName;
            loadSceneUpdateEventArgs.Progress = e.Progress;
            loadSceneUpdateEventArgs.UserData = e.UserData;
            return loadSceneUpdateEventArgs;
        }

        public override void Clear()
        {
            SceneAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}
