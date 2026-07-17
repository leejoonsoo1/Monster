using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    public sealed class LoadDictionaryFailureEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(LoadDictionaryFailureEventArgs).GetHashCode();

        public LoadDictionaryFailureEventArgs()
        {
            DictionaryAssetName = null;
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

        public string DictionaryAssetName
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

        public static LoadDictionaryFailureEventArgs Create(ReadDataFailureEventArgs e)
        {
            LoadDictionaryFailureEventArgs loadDictionaryFailureEventArgs = ReferencePool.Acquire<LoadDictionaryFailureEventArgs>();
            loadDictionaryFailureEventArgs.DictionaryAssetName = e.DataAssetName;
            loadDictionaryFailureEventArgs.ErrorMessage = e.ErrorMessage;
            loadDictionaryFailureEventArgs.UserData = e.UserData;
            return loadDictionaryFailureEventArgs;
        }

        public override void Clear()
        {
            DictionaryAssetName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
