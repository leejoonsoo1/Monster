using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarVector3 : Variable<Vector3>
    {
        public VarVector3()
        {
        }

        public static implicit operator VarVector3(Vector3 value)
        {
            VarVector3 varValue = ReferencePool.Acquire<VarVector3>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Vector3(VarVector3 value)
        {
            return value.Value;
        }
    }
}
