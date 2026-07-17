using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadDataTableFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDataTableFailureEventArgs).GetHashCode();

        public LoadDataTableFailureEventArgs()
        {
            DataTableAssetName = null;
            ErrorMessage = null;
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

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public static LoadDataTableFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadDataTableFailureEventArgs loadDataTableFailureEventArgs = ReferencePool.Acquire<LoadDataTableFailureEventArgs>();
            loadDataTableFailureEventArgs.DataTableAssetName = e.DataAssetName;
            loadDataTableFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadDataTableFailureEventArgs.UserData = e.UserData;
            return loadDataTableFailureEventArgs;
        }

        public override void Clear()
        {
            DataTableAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
