using GameFramework;
using GameFramework.Event;
using GameFramework.Network;

namespace UnityGameFramework.Runtime
{
    public sealed class NetworkCustomErrorEventArgs : GameEventArgs
    {
        public static readonly int EventId = typeof(NetworkCustomErrorEventArgs).GetHashCode();

        public NetworkCustomErrorEventArgs()
        {
            NetworkChannel = null;
            CustomErrorData = null;
        }

        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        public object CustomErrorData
        {
            get;
            private set;
        }

        public static NetworkCustomErrorEventArgs Create(GameFramework.Network.NetworkCustomErrorEventArgs e)
        {
            NetworkCustomErrorEventArgs networkCustomErrorEventArgs = ReferencePool.Acquire<NetworkCustomErrorEventArgs>();
            networkCustomErrorEventArgs.NetworkChannel = e.NetworkChannel;
            networkCustomErrorEventArgs.CustomErrorData = e.CustomErrorData;
            return networkCustomErrorEventArgs;
        }

        public override void Clear()
        {
            NetworkChannel = null;
            CustomErrorData = null;
        }
    }
}
