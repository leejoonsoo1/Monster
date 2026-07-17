using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarVector4 : Variable<Vector4>
    {
        public VarVector4()
        {
        }

        public static implicit operator VarVector4(Vector4 value)
        {
            VarVector4 varValue = ReferencePool.Acquire<VarVector4>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Vector4(VarVector4 value)
        {
            return value.Value;
        }
    }
}
