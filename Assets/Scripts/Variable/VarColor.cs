using GameFramework;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed class VarColor : Variable<Color>
    {
        public VarColor()
        {
        }

        public static implicit operator VarColor(Color value)
        {
            VarColor varValue = ReferencePool.Acquire<VarColor>();
            varValue.Value = value;
            return varValue;
        }

        public static implicit operator Color(VarColor value)
        {
            return value.Value;
        }
    }
}
