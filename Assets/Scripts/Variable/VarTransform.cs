using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarTransform : Variable<Transform>
    {
        public VarTransform()
        {
        }

        public static implicit operator VarTransform(Transform value)
        {
            VarTransform varValue = ReferencePool.Acquire<VarTransform>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Transform(VarTransform value)
        {
            return value.Value;
        }
    }
}
