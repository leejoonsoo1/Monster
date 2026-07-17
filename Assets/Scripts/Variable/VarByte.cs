using GameFramework;

namespace UnityGameFramework.Runtime
{
    public sealed class VarByte : Variable<byte>
    {
        public VarByte()
        {
        }

        public static implicit operator VarByte(byte value)
        {
            VarByte varValue = ReferencePool.Acquire<VarByte>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator byte(VarByte value)
        {
            return value.Value;
        }
    }
}
