using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadDictionaryUpdateEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDictionaryUpdateEventArgs).GetHashCode();

        public LoadDictionaryUpdateEventArgs()
        {
            DictionaryAssetName = null;
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

        public string DictionaryAssetName
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

        public static LoadDictionaryUpdateEventArgs Create(ReadDataUpdateEventArgs e)
        {
            LoadDictionaryUpdateEventArgs loadDictionaryUpdateEventArgs = ReferencePool.Acquire<LoadDictionaryUpdateEventArgs>();
            loadDictionaryUpdateEventArgs.DictionaryAssetName = e.DataAssetName;
            loadDictionaryUpdateEventArgs.Progress = e.Progress;
            loadDictionaryUpdateEventArgs.UserData = e.UserData;
            return loadDictionaryUpdateEventArgs;
        }

        public override void Clear()
        {
            DictionaryAssetName = null;
            Progress = 0f;
            UserData = null;
        }
    }
}
