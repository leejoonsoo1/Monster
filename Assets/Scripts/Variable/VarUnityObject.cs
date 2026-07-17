using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarUnityObject : Variable<Object>
    {
        public VarUnityObject()
        {
        }

        public static implicit operator VarUnityObject(Object value)
        {
            VarUnityObject varValue = ReferencePool.Acquire<VarUnityObject>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Object(VarUnityObject value)
        {
            return value.Value;
        }
    }
}
