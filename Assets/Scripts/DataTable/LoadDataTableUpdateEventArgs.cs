using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadDataTableUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableUpdateEventArgs).GetHashCode();

        public LoadDataTableUpdateEventArgs()
        {
            DataTableAssetName = null;
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

        public string DataTableAssetName
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

        public static LoadDataTableUpdateEventArgs Create(ReadDataUpdateEventArgs e)
        {
            LoadDataTableUpdateEventArgs loadDataTableUpdateEventArgs = ReferencePool.Acquire<LoadDataTableUpdateEventArgs>();
            loadDataTableUpdateEventArgs.DataTableAssetName = e.DataAssetName;
            loadDataTableUpdateEventArgs.Progress = e.Progress;
            loadDataTableUpdateEventArgs.UserData = e.UserData;
            return loadDataTableUpdateEventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}
