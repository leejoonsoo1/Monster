using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarQuaternion : Variable<Quaternion>
    {
        public VarQuaternion()
        {
        }

        public static implicit operator VarQuaternion(Quaternion value)
        {
            VarQuaternion varValue = ReferencePool.Acquire<VarQuaternion>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Quaternion(VarQuaternion value)
        {
            return value.Value;
        }
    }
}
