using GameFramework;

namespace UnityGameFramework.Runtime
{
    public sealed class DefaultSettingSerializer : GameFrameworkSerializer<DefaultSetting>
    {
        private static readonly byte[] Header = new byte[] { (byte)'G', (byte)'F', (byte)'S' };

        public DefaultSettingSerializer()
        {
        }

        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
